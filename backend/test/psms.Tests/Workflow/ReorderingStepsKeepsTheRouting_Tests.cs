using Microsoft.EntityFrameworkCore;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.WorkflowSteps;
using psms.Workflow.WorkflowSteps.Dto;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Workflow;

/// <summary>
/// Moving a step does not quietly re-aim the arrows pointing at it.
/// <para>
/// Routing is recorded as step <i>numbers</i>. Reorder reassigned those numbers
/// and left every target alone, so "send this back to the committee" kept
/// saying step 5 while something else moved into fifth place. Nothing failed;
/// a rejected application simply went somewhere nobody chose — including, in
/// the worst arrangement, forward onto the terminal step, which completes the
/// workflow instead of returning it.
/// </para>
/// <para>
/// The definition editor's arrows are what a school uses to shape its own
/// process, so this is the one operation that has to be safe.
/// </para>
/// </summary>
public class ReorderingStepsKeepsTheRouting_Tests : psmsTestBase
{
    private readonly IWorkflowStepAppService _steps;

    public ReorderingStepsKeepsTheRouting_Tests()
    {
        LoginAsDefaultTenantAdmin();
        _steps = Resolve<IWorkflowStepAppService>();
    }

    private Guid SeedFourSteps()
    {
        var definitionId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.WorkflowDefinitions.Add(new WorkflowDefinition
            {
                Id = definitionId,
                TenantId = 1,
                Name = "Shuffle Me",
                EntityType = WorkflowEntityType.Disciplinary,
                IsActive = true,
            });

            var shape = new (string Name, int Order, bool Terminal, int? Reject)[]
            {
                ("First", 1, false, null),
                ("Second", 2, false, null),
                ("Third", 3, false, null),
                ("Decide", 4, true, 3),   // sends rejections back to Third
            };

            foreach (var s in shape)
            {
                context.WorkflowSteps.Add(new WorkflowStep
                {
                    Id = Guid.NewGuid(),
                    WorkflowDefinitionId = definitionId,
                    StepOrder = s.Order,
                    Name = s.Name,
                    AssignedRole = "Admin",
                    ActionType = s.Terminal ? WorkflowActionType.Approve : WorkflowActionType.Review,
                    IsTerminal = s.Terminal,
                    NextStepOnReject = s.Reject,
                });
            }

            context.SaveChanges();
        });

        return definitionId;
    }

    private List<WorkflowStep> StepsOf(Guid definitionId) =>
        UsingDbContext(1, context => context.WorkflowSteps
            .Where(s => s.WorkflowDefinitionId == definitionId && !s.IsDeleted)
            .OrderBy(s => s.StepOrder)
            .ToList());

    private async Task ReorderAsync(Guid definitionId, params string[] names)
    {
        var byName = StepsOf(definitionId).ToDictionary(s => s.Name, s => s.Id);
        await _steps.ReorderAsync(new ReorderWorkflowStepsDto
        {
            WorkflowDefinitionId = definitionId,
            StepIds = names.Select(n => byName[n]).ToList(),
        });
    }

    [Fact]
    public async Task The_new_order_is_saved()
    {
        var definitionId = SeedFourSteps();

        await ReorderAsync(definitionId, "Second", "First", "Third", "Decide");

        StepsOf(definitionId).Select(s => s.Name)
            .ShouldBe(new[] { "Second", "First", "Third", "Decide" });
    }

    [Fact]
    public async Task A_send_back_still_reaches_the_step_it_named()
    {
        // Decide pointed at step 3, which was Third. Move Third to the top and
        // step 3 becomes something else entirely.
        var definitionId = SeedFourSteps();

        await ReorderAsync(definitionId, "Third", "First", "Second", "Decide");

        var steps = StepsOf(definitionId);
        var decide = steps.Single(s => s.Name == "Decide");
        var third = steps.Single(s => s.Name == "Third");

        decide.NextStepOnReject.ShouldBe(third.StepOrder);
        decide.NextStepOnReject.ShouldBe(1);
    }

    [Fact]
    public async Task And_never_ends_up_aimed_at_the_terminal_step()
    {
        // The worst outcome of leaving targets alone: a rejection that completes
        // the workflow because the number now belongs to the decision itself.
        var definitionId = SeedFourSteps();

        await ReorderAsync(definitionId, "First", "Second", "Decide", "Third");

        var steps = StepsOf(definitionId);
        var decide = steps.Single(s => s.Name == "Decide");

        steps.Single(s => s.StepOrder == decide.NextStepOnReject.Value)
            .IsTerminal.ShouldBeFalse();
    }

    [Fact]
    public async Task A_forward_target_moves_with_its_step_too()
    {
        var definitionId = SeedFourSteps();
        UsingDbContext(1, context =>
        {
            var first = context.WorkflowSteps
                .Single(s => s.WorkflowDefinitionId == definitionId && s.Name == "First");
            first.NextStepOnApprove = 3; // skip ahead to Third
            context.SaveChanges();
        });

        await ReorderAsync(definitionId, "Third", "First", "Second", "Decide");

        var steps = StepsOf(definitionId);
        steps.Single(s => s.Name == "First").NextStepOnApprove
            .ShouldBe(steps.Single(s => s.Name == "Third").StepOrder);
    }

    [Fact]
    public async Task An_application_in_flight_still_knows_where_it_is()
    {
        var definitionId = SeedFourSteps();
        var secondId = StepsOf(definitionId).Single(s => s.Name == "Second").Id;
        var instanceId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.WorkflowInstances.Add(new WorkflowInstance
            {
                Id = instanceId,
                TenantId = 1,
                WorkflowDefinitionId = definitionId,
                EntityType = WorkflowEntityType.Disciplinary,
                EntityId = Guid.NewGuid(),
                Status = WorkflowStatus.InProgress,
                CurrentStepId = secondId,
                CurrentStepOrder = 2,
            });
            context.SaveChanges();
        });

        await ReorderAsync(definitionId, "Second", "First", "Third", "Decide");

        var after = UsingDbContext(1, c => c.WorkflowInstances.Single(i => i.Id == instanceId));
        after.CurrentStepId.ShouldBe(secondId);
        after.CurrentStepOrder.ShouldBe(1); // Second is now first
    }

    [Fact]
    public async Task Reordering_the_same_way_twice_changes_nothing()
    {
        var definitionId = SeedFourSteps();

        await ReorderAsync(definitionId, "Second", "First", "Third", "Decide");
        var once = StepsOf(definitionId).Select(s => $"{s.StepOrder}:{s.Name}:{s.NextStepOnReject}").ToList();

        await ReorderAsync(definitionId, "Second", "First", "Third", "Decide");
        StepsOf(definitionId).Select(s => $"{s.StepOrder}:{s.Name}:{s.NextStepOnReject}").ShouldBe(once);
    }
}
