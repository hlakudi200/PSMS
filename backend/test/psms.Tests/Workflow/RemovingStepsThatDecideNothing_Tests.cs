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
/// Retiring the two admissions steps that decided nothing.
/// <para>
/// <b>Document Verification</b> verified nothing: the guard written for it was
/// attached to no step, and there was no way to mark a document verified at
/// all. <b>Committee Review</b> named a committee the system cannot express —
/// a step has one role and at most one fixed person — and sat between two other
/// Admin steps doing what the principal's decision already does.
/// </para>
/// <para>
/// Removing steps is harder than adding one: applications are sitting on them
/// right now, routing targets are recorded as step numbers, and so is each
/// instance's position. These cover all three.
/// </para>
/// </summary>
public class RemovingStepsThatDecideNothing_Tests : psmsTestBase
{
    public RemovingStepsThatDecideNothing_Tests()
    {
        LoginAsDefaultTenantAdmin();
    }

    private async Task RunTheSeederAsync()
    {
        using var uow = Resolve<Abp.Domain.Uow.IUnitOfWorkManager>().Begin();
        await Resolve<WorkflowSeedService>().SeedDefaultsAsync();
        await uow.CompleteAsync();
    }

    /// <summary>The six-step shape a school was seeded with before this.</summary>
    private Guid SeedTheOldShape()
    {
        var definitionId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            foreach (var old in context.WorkflowDefinitions
                         .Include(d => d.Steps)
                         .Where(d => d.EntityType == WorkflowEntityType.Application)
                         .ToList())
            {
                context.WorkflowSteps.RemoveRange(old.Steps);
                context.WorkflowDefinitions.Remove(old);
            }

            context.WorkflowDefinitions.Add(new WorkflowDefinition
            {
                Id = definitionId,
                TenantId = 1,
                Name = "Admissions Approval",
                EntityType = WorkflowEntityType.Application,
                IsActive = true,
            });

            var shape = new (string Name, int Order, string Role, bool Terminal, int? Reject)[]
            {
                ("Admin Review", 1, "Admin", false, null),
                ("Document Verification", 2, "Admin", false, null),
                ("Interview", 3, "Teacher", false, null),
                ("Placement Assessment", 4, "Teacher", false, null),
                ("Committee Review", 5, "Admin", false, null),
                ("Principal Decision", 6, "Principal", true, 5),
            };

            foreach (var s in shape)
            {
                context.WorkflowSteps.Add(new WorkflowStep
                {
                    Id = Guid.NewGuid(),
                    WorkflowDefinitionId = definitionId,
                    StepOrder = s.Order,
                    Name = s.Name,
                    AssignedRole = s.Role,
                    ActionType = s.Terminal ? WorkflowActionType.Approve : WorkflowActionType.Review,
                    IsTerminal = s.Terminal,
                    NextStepOnReject = s.Reject,
                    DecisionSchemaKey = s.Terminal ? "application.decision" : null,
                });
            }

            context.SaveChanges();
        });

        return definitionId;
    }

    /// <summary>
    /// The steps as the engine sees them. A removed step is soft-deleted —
    /// WorkflowStep is a FullAuditedEntity — so it stays in the table with its
    /// old number and must be filtered out, exactly as ABP's own query filter
    /// does on every production read.
    /// </summary>
    private List<WorkflowStep> StepsOf(Guid definitionId) =>
        UsingDbContext(1, context => context.WorkflowSteps
            .Where(s => s.WorkflowDefinitionId == definitionId && !s.IsDeleted)
            .OrderBy(s => s.StepOrder)
            .ToList());

    private Guid PutAnApplicationOn(Guid definitionId, string stepName)
    {
        var step = StepsOf(definitionId).Single(s => s.Name == stepName);
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
                CurrentStepId = step.Id,
                CurrentStepOrder = step.StepOrder,
            });
            context.SaveChanges();
        });

        return instanceId;
    }

    private WorkflowInstance Reload(Guid instanceId) =>
        UsingDbContext(1, context => context.WorkflowInstances.Single(i => i.Id == instanceId));

    [Fact]
    public async Task A_school_seeded_before_this_keeps_only_the_steps_that_decide()
    {
        var definitionId = SeedTheOldShape();

        await RunTheSeederAsync();

        StepsOf(definitionId).Select(s => s.Name).ShouldBe(new[]
        {
            "Admin Review",
            "Interview",
            "Placement Assessment",
            "Principal Decision",
        });
    }

    [Fact]
    public async Task And_they_are_numbered_one_to_four()
    {
        var definitionId = SeedTheOldShape();
        await RunTheSeederAsync();

        StepsOf(definitionId).Select(s => s.StepOrder).ShouldBe(new[] { 1, 2, 3, 4 });
    }

    [Fact]
    public async Task Sending_a_decision_back_now_reaches_the_review()
    {
        /* It pointed at step 5, the committee. Left alone it would point at
           whatever ended up fifth — and with only four steps left, at nothing
           at all, which the engine treats as a misconfiguration rather than an
           approval. */
        var definitionId = SeedTheOldShape();
        await RunTheSeederAsync();

        var steps = StepsOf(definitionId);
        var decision = steps.Single(s => s.Name == "Principal Decision");

        decision.NextStepOnReject.ShouldBe(steps.Single(s => s.Name == "Admin Review").StepOrder);
    }

    [Fact]
    public async Task An_application_waiting_on_document_verification_moves_on_to_the_interview()
    {
        // Forward, not back: the review before it was done, and a school tidying
        // up its workflow should not cost a family that progress.
        var definitionId = SeedTheOldShape();
        var instanceId = PutAnApplicationOn(definitionId, "Document Verification");

        await RunTheSeederAsync();

        var interview = StepsOf(definitionId).Single(s => s.Name == "Interview");
        var after = Reload(instanceId);

        after.CurrentStepId.ShouldBe(interview.Id);
        after.CurrentStepOrder.ShouldBe(interview.StepOrder);
    }

    [Fact]
    public async Task An_application_waiting_on_the_committee_moves_on_to_the_principal()
    {
        var definitionId = SeedTheOldShape();
        var instanceId = PutAnApplicationOn(definitionId, "Committee Review");

        await RunTheSeederAsync();

        var decision = StepsOf(definitionId).Single(s => s.Name == "Principal Decision");
        var after = Reload(instanceId);

        after.CurrentStepId.ShouldBe(decision.Id);
        after.CurrentStepOrder.ShouldBe(decision.StepOrder);
    }

    [Fact]
    public async Task An_application_on_a_surviving_step_stays_where_it_is()
    {
        // Its step does not move; the number recording where that is does.
        var definitionId = SeedTheOldShape();
        var interviewIdBefore = StepsOf(definitionId).Single(s => s.Name == "Interview").Id;
        var instanceId = PutAnApplicationOn(definitionId, "Interview");

        await RunTheSeederAsync();

        var after = Reload(instanceId);
        var interviewNow = StepsOf(definitionId).Single(s => s.Name == "Interview");

        after.CurrentStepId.ShouldBe(interviewIdBefore);
        after.CurrentStepOrder.ShouldBe(interviewNow.StepOrder);
        after.CurrentStepOrder.ShouldBe(2);
    }

    [Fact]
    public async Task The_history_says_why_an_application_moved()
    {
        var definitionId = SeedTheOldShape();
        var instanceId = PutAnApplicationOn(definitionId, "Committee Review");

        await RunTheSeederAsync();

        var written = UsingDbContext(1, context => context.WorkflowTransitions
            .Where(t => t.WorkflowInstanceId == instanceId)
            .ToList());

        written.ShouldNotBeEmpty();
        written.Last().Comment.ShouldContain("Committee Review");
        written.Last().Comment.ShouldContain("removed");
        written.Last().IsWaived.ShouldBeTrue();
    }

    [Fact]
    public async Task A_school_that_made_the_committee_its_own_keeps_it()
    {
        // Renamed, so it is somebody's deliberate stage now rather than the
        // placeholder we shipped.
        var definitionId = SeedTheOldShape();
        UsingDbContext(1, context =>
        {
            var committee = context.WorkflowSteps
                .Single(s => s.WorkflowDefinitionId == definitionId && s.Name == "Committee Review");
            committee.Name = "Governing Body Panel";
            context.SaveChanges();
        });

        await RunTheSeederAsync();

        StepsOf(definitionId).Select(s => s.Name).ShouldContain("Governing Body Panel");
    }

    [Fact]
    public async Task A_step_reassigned_to_another_role_is_left_alone()
    {
        var definitionId = SeedTheOldShape();
        UsingDbContext(1, context =>
        {
            var documents = context.WorkflowSteps
                .Single(s => s.WorkflowDefinitionId == definitionId && s.Name == "Document Verification");
            documents.AssignedRole = "AdmissionsOfficer";
            context.SaveChanges();
        });

        await RunTheSeederAsync();

        StepsOf(definitionId).Select(s => s.Name).ShouldContain("Document Verification");
    }

    [Fact]
    public async Task Running_the_seeder_twice_changes_nothing_the_second_time()
    {
        var definitionId = SeedTheOldShape();

        await RunTheSeederAsync();
        var afterOnce = StepsOf(definitionId).Select(s => $"{s.StepOrder}:{s.Name}").ToList();

        await RunTheSeederAsync();
        var afterTwice = StepsOf(definitionId).Select(s => $"{s.StepOrder}:{s.Name}").ToList();

        afterTwice.ShouldBe(afterOnce);
    }

    [Fact]
    public async Task A_fresh_school_is_seeded_with_the_four_steps_directly()
    {
        UsingDbContext(1, context =>
        {
            foreach (var old in context.WorkflowDefinitions
                         .Include(d => d.Steps)
                         .Where(d => d.EntityType == WorkflowEntityType.Application)
                         .ToList())
            {
                context.WorkflowSteps.RemoveRange(old.Steps);
                context.WorkflowDefinitions.Remove(old);
            }
            context.SaveChanges();
        });

        await RunTheSeederAsync();

        var definitionId = UsingDbContext(1, context => context.WorkflowDefinitions
            .Single(d => d.EntityType == WorkflowEntityType.Application).Id);

        StepsOf(definitionId).Select(s => s.Name).ShouldBe(new[]
        {
            "Admin Review",
            "Interview",
            "Placement Assessment",
            "Principal Decision",
        });
    }
}
