using Microsoft.EntityFrameworkCore;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Seed;
using psms.Workflow.WorkflowInstances;
using psms.Workflow.WorkflowInstances.Dto;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Workflow;

/// <summary>
/// A final step with no decision form on it says so before the principal fills
/// the dialog in.
/// <para>
/// Schools set up before the decision form was attached still have a Principal
/// Decision step with nothing to record. The dialog shows an action and a
/// comment box, exactly like every other step; the refusal only arrives once
/// everything has been filled in and OK pressed. The detail reads the
/// misconfiguration up front so the screen can warn instead.
/// </para>
/// </summary>
public class TheStepThatCannotDecide_Tests : psmsTestBase
{
    public TheStepThatCannotDecide_Tests()
    {
        LoginAsDefaultTenantAdmin();
    }

    private async Task RunTheSeederAsync()
    {
        using var uow = Resolve<Abp.Domain.Uow.IUnitOfWorkManager>().Begin();
        await Resolve<WorkflowSeedService>().SeedDefaultsAsync();
        await uow.CompleteAsync();
    }

    private WorkflowStep TerminalStep() =>
        UsingDbContext(1, context => context.WorkflowSteps
            .Include(s => s.WorkflowDefinition)
            .Where(s => s.WorkflowDefinition.EntityType == WorkflowEntityType.Application && !s.IsDeleted)
            .OrderBy(s => s.StepOrder)
            .ToList()
            .Last(s => s.IsTerminal));

    /// <summary>Parks an application instance on the terminal step.</summary>
    private Guid StandOnTheFinalStep()
    {
        var step = TerminalStep();
        var id = Guid.NewGuid();
        UsingDbContext(1, context =>
        {
            context.WorkflowInstances.Add(new WorkflowInstance
            {
                Id = id,
                TenantId = 1,
                WorkflowDefinitionId = step.WorkflowDefinitionId,
                EntityType = WorkflowEntityType.Application,
                EntityId = Guid.NewGuid(),
                Status = WorkflowStatus.InProgress,
                CurrentStepId = step.Id,
                CurrentStepOrder = step.StepOrder,
                StartedDate = DateTime.Now,
            });
            context.SaveChanges();
        });
        return id;
    }

    private void StripTheDecisionForm()
    {
        var step = TerminalStep();
        UsingDbContext(1, context =>
        {
            var row = context.WorkflowSteps.Single(s => s.Id == step.Id);
            row.DecisionSchemaKey = null;
            context.SaveChanges();
        });
    }

    private async Task<WorkflowInstanceDto> ReadAsync(Guid id)
    {
        using var uow = Resolve<Abp.Domain.Uow.IUnitOfWorkManager>().Begin();
        var dto = await Resolve<IWorkflowInstanceAppService>().GetAsync(id);
        await uow.CompleteAsync();
        return dto;
    }

    [Fact]
    public async Task A_properly_seeded_step_is_not_flagged()
    {
        await RunTheSeederAsync();
        var id = StandOnTheFinalStep();

        var dto = await ReadAsync(id);

        dto.CurrentStepDecisionSchema.ShouldNotBeNull();
        dto.CurrentStepIsMissingItsDecisionForm.ShouldBeFalse();
    }

    [Fact]
    public async Task A_final_step_without_the_form_is_flagged()
    {
        await RunTheSeederAsync();
        StripTheDecisionForm();
        var id = StandOnTheFinalStep();

        var dto = await ReadAsync(id);

        dto.CurrentStepDecisionSchema.ShouldBeNull();
        dto.CurrentStepIsMissingItsDecisionForm.ShouldBeTrue();
    }

    [Fact]
    public async Task An_earlier_step_without_a_form_is_not_flagged()
    {
        // Only the step that ends the workflow has to record a decision; the
        // review steps before it are meant to carry nothing.
        await RunTheSeederAsync();
        var first = UsingDbContext(1, context => context.WorkflowSteps
            .Include(s => s.WorkflowDefinition)
            .Where(s => s.WorkflowDefinition.EntityType == WorkflowEntityType.Application && !s.IsDeleted)
            .OrderBy(s => s.StepOrder)
            .First());

        var id = Guid.NewGuid();
        UsingDbContext(1, context =>
        {
            context.WorkflowInstances.Add(new WorkflowInstance
            {
                Id = id,
                TenantId = 1,
                WorkflowDefinitionId = first.WorkflowDefinitionId,
                EntityType = WorkflowEntityType.Application,
                EntityId = Guid.NewGuid(),
                Status = WorkflowStatus.InProgress,
                CurrentStepId = first.Id,
                CurrentStepOrder = first.StepOrder,
                StartedDate = DateTime.Now,
            });
            context.SaveChanges();
        });

        (await ReadAsync(id)).CurrentStepIsMissingItsDecisionForm.ShouldBeFalse();
    }
}
