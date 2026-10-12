using Microsoft.EntityFrameworkCore;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Seed;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Workflow;

/// <summary>
/// The placement assessment, and moving the steps after it.
/// <para>
/// A school decides per grade whether applicants sit an assessment, and that
/// answer had nowhere to land: the admissions workflow went from the interview
/// straight to the committee. <c>IsAssessmentRequired</c>, the entity behind
/// it, its whole service and the skip rule written for it all governed a step
/// that did not exist.
/// </para>
/// <para>
/// Adding it between the interview and the committee moves the two steps after
/// it down one, which is the part worth testing: schools are already running
/// applications through this workflow.
/// </para>
/// </summary>
public class AddingTheAssessmentStep_Tests : psmsTestBase
{
    private const string Assessment = "Placement Assessment";

    public AddingTheAssessmentStep_Tests()
    {
        LoginAsDefaultTenantAdmin();
    }

    /// <summary>The workflow exactly as it was seeded before the assessment existed.</summary>
    private Guid SeedTheOldShape()
    {
        var definitionId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            // The tenant is seeded with a current-shape workflow; this is the
            // school that was set up before, so the new one goes away first.
            var already = context.WorkflowDefinitions
                .Include(d => d.Steps)
                .Where(d => d.EntityType == WorkflowEntityType.Application)
                .ToList();
            foreach (var definition in already)
            {
                context.WorkflowSteps.RemoveRange(definition.Steps);
                context.WorkflowDefinitions.Remove(definition);
            }

            context.WorkflowDefinitions.Add(new WorkflowDefinition
            {
                Id = definitionId,
                TenantId = 1,
                Name = "Admissions Approval",
                EntityType = WorkflowEntityType.Application,
                IsActive = true,
            });

            var shape = new (string Name, int Order, string Role, int? RejectsTo)[]
            {
                ("Admin Review", 1, "Admin", null),
                ("Document Verification", 2, "Admin", null),
                ("Interview", 3, "Teacher", null),
                ("Committee Review", 4, "Admin", null),
                ("Principal Decision", 5, "Principal", 4),
            };

            foreach (var step in shape)
            {
                context.WorkflowSteps.Add(new WorkflowStep
                {
                    Id = Guid.NewGuid(),
                    WorkflowDefinitionId = definitionId,
                    StepOrder = step.Order,
                    Name = step.Name,
                    AssignedRole = step.Role,
                    ActionType = step.Name == "Principal Decision"
                        ? WorkflowActionType.Approve
                        : WorkflowActionType.Review,
                    IsTerminal = step.Name == "Principal Decision",
                    NextStepOnReject = step.RejectsTo,
                });
            }

            context.SaveChanges();
        });

        return definitionId;
    }

    /// <summary>
    /// The seeder is an application service and wants an ambient unit of work;
    /// resolving it straight from the container leaves it holding a context
    /// that has already been disposed.
    /// </summary>
    private async Task RunTheSeederAsync()
    {
        using var uow = Resolve<Abp.Domain.Uow.IUnitOfWorkManager>().Begin();
        await Resolve<WorkflowSeedService>().SeedDefaultsAsync();
        await uow.CompleteAsync();
    }

    private List<WorkflowStep> StepsOf(Guid definitionId) =>
        UsingDbContext(1, context => context.WorkflowSteps
            .Where(s => s.WorkflowDefinitionId == definitionId && !s.IsDeleted)
            .OrderBy(s => s.StepOrder)
            .ToList());

    [Fact]
    public async Task A_school_set_up_before_it_existed_gets_the_step()
    {
        var definitionId = SeedTheOldShape();

        await RunTheSeederAsync();

        /* The same seeding pass also retires Document Verification and
           Committee Review, so a school that predates the assessment ends up
           with the four steps that decide something. */
        StepsOf(definitionId).Select(s => s.Name).ShouldBe(new[]
        {
            "Admin Review",
            "Interview",
            Assessment,
            "Principal Decision",
        });
    }

    [Fact]
    public async Task It_sits_after_the_interview_and_before_the_decision()
    {
        var definitionId = SeedTheOldShape();
        await RunTheSeederAsync();

        var steps = StepsOf(definitionId);

        steps.Single(s => s.Name == Assessment).StepOrder
            .ShouldBe(steps.Single(s => s.Name == "Interview").StepOrder + 1);
        steps.Single(s => s.Name == "Principal Decision").StepOrder
            .ShouldBe(steps.Single(s => s.Name == Assessment).StepOrder + 1);
    }

    [Fact]
    public async Task The_decision_sends_rejections_to_a_step_that_still_exists()
    {
        /* It pointed at step 4, which the insert turned into the assessment and
           the removal turned into nothing. Left alone, a principal sending an
           application back would have aimed it at whatever happened to be
           fourth — or at a number with no step behind it at all. */
        var definitionId = SeedTheOldShape();
        await RunTheSeederAsync();

        var steps = StepsOf(definitionId);
        var target = steps.Single(s => s.Name == "Principal Decision").NextStepOnReject;

        target.ShouldNotBeNull();
        steps.Select(s => s.StepOrder).ShouldContain(target.Value);
        steps.Single(s => s.StepOrder == target.Value).IsTerminal.ShouldBeFalse();
    }

    [Fact]
    public async Task An_application_part_way_through_stays_on_the_step_it_was_on()
    {
        var definitionId = SeedTheOldShape();
        var interviewId = StepsOf(definitionId).Single(s => s.Name == "Interview").Id;
        var instanceId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.WorkflowInstances.Add(new WorkflowInstance
            {
                Id = instanceId,
                TenantId = 1,
                WorkflowDefinitionId = definitionId,
                EntityType = WorkflowEntityType.Application,
                EntityId = Guid.NewGuid(),
                Status = WorkflowStatus.InProgress,
                CurrentStepId = interviewId,
                CurrentStepOrder = 3,
            });
            context.SaveChanges();
        });

        await RunTheSeederAsync();

        var after = UsingDbContext(1, context => context.WorkflowInstances.Single(i => i.Id == instanceId));
        var interviewNow = StepsOf(definitionId).Single(s => s.Name == "Interview");

        // Same step, and the number it records still describes where that is.
        after.CurrentStepId.ShouldBe(interviewId);
        after.CurrentStepOrder.ShouldBe(interviewNow.StepOrder);
    }

    [Fact]
    public async Task An_application_before_the_insertion_point_is_untouched()
    {
        // The first step is before everything the seeding pass touches, so an
        // application waiting there should not move at all.
        var definitionId = SeedTheOldShape();
        var reviewId = StepsOf(definitionId).Single(s => s.Name == "Admin Review").Id;
        var instanceId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.WorkflowInstances.Add(new WorkflowInstance
            {
                Id = instanceId,
                TenantId = 1,
                WorkflowDefinitionId = definitionId,
                EntityType = WorkflowEntityType.Application,
                EntityId = Guid.NewGuid(),
                Status = WorkflowStatus.InProgress,
                CurrentStepId = reviewId,
                CurrentStepOrder = 1,
            });
            context.SaveChanges();
        });

        await RunTheSeederAsync();

        var after = UsingDbContext(1, context => context.WorkflowInstances.Single(i => i.Id == instanceId));
        after.CurrentStepId.ShouldBe(reviewId);
        after.CurrentStepOrder.ShouldBe(1);
    }

    [Fact]
    public async Task The_step_belongs_to_a_teacher_and_is_skipped_where_the_grade_does_not_assess()
    {
        var definitionId = SeedTheOldShape();
        await RunTheSeederAsync();

        var assessment = StepsOf(definitionId).Single(s => s.Name == Assessment);

        assessment.AssignedRole.ShouldBe("Teacher");
        assessment.SkipWhenKey.ShouldBe("application.assessment-not-required");
        assessment.IsOptional.ShouldBeTrue();
    }

    [Fact]
    public async Task Running_the_seeder_twice_does_not_add_it_twice()
    {
        var definitionId = SeedTheOldShape();

        await RunTheSeederAsync();
        await RunTheSeederAsync();

        StepsOf(definitionId).Count(s => s.Name == Assessment).ShouldBe(1);
    }
}
