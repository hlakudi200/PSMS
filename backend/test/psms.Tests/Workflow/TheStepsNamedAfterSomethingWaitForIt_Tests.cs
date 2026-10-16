using Microsoft.EntityFrameworkCore;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Engine;
using psms.Workflow.Seed;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Workflow;

/// <summary>
/// The Interview and Placement Assessment steps wait for the thing they are
/// named after.
/// <para>
/// Both were plain review steps. Walking a Grade 5 application — a grade whose
/// own settings require an interview <i>and</i> an assessment — all the way to
/// a decision on the deployed system, each step offered an action and a comment
/// box, was forwarded, and recorded nothing:
/// <c>AdmissionInterview/GetByApplication</c> and
/// <c>AdmissionAssessment/GetByApplication</c> both still answered null. The
/// principal then decided with neither behind the decision.
/// </para>
/// <para>
/// The guards for exactly this were written, registered with the engine, and
/// attached to no step. So were the other three.
/// </para>
/// </summary>
public class TheStepsNamedAfterSomethingWaitForIt_Tests : psmsTestBase
{
    private const string InterviewGuard = "application.interview-recorded";
    private const string AssessmentGuard = "application.assessment-recorded";

    public TheStepsNamedAfterSomethingWaitForIt_Tests()
    {
        LoginAsDefaultTenantAdmin();
    }

    private async Task RunTheSeederAsync()
    {
        using var uow = Resolve<Abp.Domain.Uow.IUnitOfWorkManager>().Begin();
        await Resolve<WorkflowSeedService>().SeedDefaultsAsync();
        await uow.CompleteAsync();
    }

    private WorkflowStep Step(string name) =>
        UsingDbContext(1, context => context.WorkflowSteps
            .Include(s => s.WorkflowDefinition)
            .Where(s => s.WorkflowDefinition.EntityType == WorkflowEntityType.Application && !s.IsDeleted)
            .OrderBy(s => s.StepOrder)
            .ToList()
            .LastOrDefault(s => s.Name == name));

    [Fact]
    public async Task A_freshly_seeded_school_gets_both_guards()
    {
        await RunTheSeederAsync();

        Step("Interview").GuardKey.ShouldBe(InterviewGuard);
        Step("Placement Assessment").GuardKey.ShouldBe(AssessmentGuard);
    }

    [Fact]
    public async Task And_so_does_a_school_seeded_before_they_were_attached()
    {
        await RunTheSeederAsync();

        // Strip them, as every school seeded before this had them.
        foreach (var name in new[] { "Interview", "Placement Assessment" })
        {
            var id = Step(name).Id;
            UsingDbContext(1, context =>
            {
                context.WorkflowSteps.Single(s => s.Id == id).GuardKey = null;
                context.SaveChanges();
            });
        }

        await RunTheSeederAsync();

        Step("Interview").GuardKey.ShouldBe(InterviewGuard);
        Step("Placement Assessment").GuardKey.ShouldBe(AssessmentGuard);
    }

    [Fact]
    public async Task A_guard_the_school_chose_for_itself_is_left_alone()
    {
        await RunTheSeederAsync();

        var id = Step("Interview").Id;
        UsingDbContext(1, context =>
        {
            context.WorkflowSteps.Single(s => s.Id == id).GuardKey = "application.fee-paid";
            context.SaveChanges();
        });

        await RunTheSeederAsync();

        Step("Interview").GuardKey.ShouldBe("application.fee-paid");
    }

    [Fact]
    public async Task Running_it_twice_changes_nothing_the_second_time()
    {
        await RunTheSeederAsync();
        var first = Step("Interview").LastModificationTime;

        await RunTheSeederAsync();

        Step("Interview").GuardKey.ShouldBe(InterviewGuard);
        Step("Interview").LastModificationTime.ShouldBe(first);
    }

    [Fact]
    public async Task The_steps_keep_the_escape_hatch_they_had()
    {
        /* A guard the holder cannot satisfy must not be a dead end. Both steps
           stay optional, so a teacher can waive with a reason — recorded —
           and a grade that requires neither never sees them at all, because
           the skip rules are evaluated before any guard is. */
        await RunTheSeederAsync();

        foreach (var name in new[] { "Interview", "Placement Assessment" })
        {
            var step = Step(name);
            step.IsOptional.ShouldBeTrue($"{name} must stay waivable");
            step.SkipWhenKey.ShouldNotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task The_guards_are_ones_the_engine_can_actually_resolve()
    {
        /* A key with no registered guard behind it throws when the step is
           reached, which is a worse failure than the one being fixed. */
        await RunTheSeederAsync();

        using var uow = Resolve<Abp.Domain.Uow.IUnitOfWorkManager>().Begin();
        var registry = Resolve<WorkflowExtensionRegistry>();

        foreach (var name in new[] { "Interview", "Placement Assessment" })
        {
            var step = Step(name);
            Should.NotThrow(() => registry.GetGuard(step.GuardKey, WorkflowEntityType.Application));
        }

        await uow.CompleteAsync();
    }

    [Fact]
    public async Task An_application_with_no_interview_cannot_pass_the_interview_step()
    {
        await RunTheSeederAsync();

        using var uow = Resolve<Abp.Domain.Uow.IUnitOfWorkManager>().Begin();
        var guard = Resolve<WorkflowExtensionRegistry>()
            .GetGuard(InterviewGuard, WorkflowEntityType.Application);

        var result = await guard.EvaluateAsync(Guid.NewGuid());

        result.Satisfied.ShouldBeFalse();
        // The teacher holding the step cannot book an interview, so the message
        // has to say who does.
        result.Message.ShouldContain("office");

        await uow.CompleteAsync();
    }
}
