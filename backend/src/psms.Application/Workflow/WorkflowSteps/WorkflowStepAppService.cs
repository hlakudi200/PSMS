using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Authorization.Users;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Engine;
using psms.Workflow.Shared;
using psms.Workflow.WorkflowSteps.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.WorkflowSteps;

[AbpAuthorize(PermissionNames.Workflow_Definitions)]
public class WorkflowStepAppService : ApplicationService, IWorkflowStepAppService
{
    private readonly IRepository<WorkflowStep, Guid> _stepRepository;
    private readonly IRepository<WorkflowDefinition, Guid> _definitionRepository;
    private readonly IRepository<WorkflowInstance, Guid> _instanceRepository;
    private readonly UserManager _userManager;
    private readonly WorkflowExtensionRegistry _extensions;

    public WorkflowStepAppService(
        IRepository<WorkflowStep, Guid> stepRepository,
        IRepository<WorkflowDefinition, Guid> definitionRepository,
        IRepository<WorkflowInstance, Guid> instanceRepository,
        UserManager userManager,
        WorkflowExtensionRegistry extensions)
    {
        _extensions = extensions;
        _stepRepository = stepRepository;
        _definitionRepository = definitionRepository;
        _instanceRepository = instanceRepository;
        _userManager = userManager;
    }

    /// <summary>
    /// WF-05: when a step is pinned to a specific user, that user must hold the
    /// step's required role — otherwise the step would be unactionable (the
    /// act-time check requires the role too). Validated against the current
    /// tenant's users, which also blocks assigning a user from another tenant.
    /// </summary>
    private async Task EnsureAssignedUserHoldsRoleAsync(long assignedUserId, string assignedRole)
    {
        var user = await _userManager.FindByIdAsync(assignedUserId.ToString());
        if (user == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.AssignedUserMissingRole,
                "The selected user does not exist in this tenant.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Any(r => r.Equals(assignedRole, StringComparison.OrdinalIgnoreCase)))
            throw new UserFriendlyException(WorkflowExceptionCodes.AssignedUserMissingRole,
                $"The selected user does not hold the step's required role '{assignedRole}'.");
    }

    /// <summary>
    /// WF-30/31/32: a step may only reference guards, effects and decision schemas
    /// registered for the definition's entity type — a typo would otherwise block
    /// every instance at run time.
    /// </summary>
    private void EnsureExtensionKeysExist(WorkflowEntityType entityType, string guardKey, string entryEffectKey, string exitEffectKey, string decisionSchemaKey)
    {
        if (!_extensions.GuardExists(guardKey, entityType))
            throw new UserFriendlyException(WorkflowExceptionCodes.ExtensionNotFound, $"Unknown guard '{guardKey}' for {entityType}.");
        if (!_extensions.EffectExists(entryEffectKey, entityType))
            throw new UserFriendlyException(WorkflowExceptionCodes.ExtensionNotFound, $"Unknown entry effect '{entryEffectKey}' for {entityType}.");
        if (!_extensions.EffectExists(exitEffectKey, entityType))
            throw new UserFriendlyException(WorkflowExceptionCodes.ExtensionNotFound, $"Unknown exit effect '{exitEffectKey}' for {entityType}.");
        if (!_extensions.DecisionSchemaExists(decisionSchemaKey, entityType))
            throw new UserFriendlyException(WorkflowExceptionCodes.ExtensionNotFound, $"Unknown decision schema '{decisionSchemaKey}' for {entityType}.");
    }

    /// <summary>
    /// WF-33: a running instance reads the LIVE step rows, so editing steps under it
    /// would rewire an approval mid-flight. Steps are locked while any instance of
    /// the definition is active; make changes on a clone (WorkflowDefinition/Clone)
    /// and activate that instead.
    /// </summary>
    private async Task EnsureNoActiveInstancesAsync(Guid definitionId)
    {
        var active = await _instanceRepository.GetAll().AnyAsync(i =>
            i.WorkflowDefinitionId == definitionId
            && i.TenantId == AbpSession.TenantId
            && (i.Status == WorkflowStatus.NotStarted || i.Status == WorkflowStatus.InProgress));
        if (active)
            throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionHasActiveInstancesCannotModifySteps,
                "This definition has workflows in progress, so its steps are locked. Clone it as a new version, edit the clone, then activate it.");
    }

    private static string Key(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    [AbpAuthorize(PermissionNames.Workflow_Definitions_View)]
    public async Task<WorkflowStepDto> GetAsync(Guid id)
    {
        var step = await _stepRepository
            .GetAll()
            .Where(s => s.WorkflowDefinition.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (step == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.StepNotFound,
                "Workflow step not found.");

        return ObjectMapper.Map<WorkflowStepDto>(step);
    }

    [AbpAuthorize(PermissionNames.Workflow_Definitions_View)]
    public async Task<List<WorkflowStepDto>> GetByDefinitionAsync(Guid definitionId)
    {
        // Verify definition belongs to tenant
        var definitionExists = await _definitionRepository
            .GetAll()
            .AnyAsync(d => d.Id == definitionId && d.TenantId == AbpSession.TenantId);

        if (!definitionExists)
            throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionNotFound,
                "Workflow definition not found.");

        var steps = await _stepRepository
            .GetAll()
            .Where(s => s.WorkflowDefinitionId == definitionId)
            .OrderBy(s => s.StepOrder)
            .ToListAsync();

        return ObjectMapper.Map<List<WorkflowStepDto>>(steps);
    }

    [AbpAuthorize(PermissionNames.Workflow_Definitions_Edit)]
    public async Task<WorkflowStepDto> CreateAsync(CreateWorkflowStepDto input)
    {
        // Verify definition belongs to tenant
        var definition = await _definitionRepository
            .FirstOrDefaultAsync(d => d.Id == input.WorkflowDefinitionId && d.TenantId == AbpSession.TenantId);

        if (definition == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionNotFound,
                "Workflow definition not found.");

        await EnsureNoActiveInstancesAsync(definition.Id);

        // Check duplicate step order
        var orderExists = await _stepRepository
            .GetAll()
            .AnyAsync(s => s.WorkflowDefinitionId == input.WorkflowDefinitionId
                && s.StepOrder == input.StepOrder);

        if (orderExists)
            throw new UserFriendlyException(WorkflowExceptionCodes.StepOrderDuplicate,
                $"Step order {input.StepOrder} already exists in this definition.");

        // WF-05: a user-pinned step requires that user to hold the step's role.
        if (input.AssignedUserId.HasValue)
            await EnsureAssignedUserHoldsRoleAsync(input.AssignedUserId.Value, input.AssignedRole.Trim());

        EnsureExtensionKeysExist(definition.EntityType, input.GuardKey, input.EntryEffectKey, input.ExitEffectKey, input.DecisionSchemaKey);

        var step = new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowDefinitionId = input.WorkflowDefinitionId,
            StepOrder = input.StepOrder,
            Name = input.Name.Trim(),
            Description = input.Description?.Trim(),
            AssignedRole = input.AssignedRole.Trim(),
            ActionType = input.ActionType,
            IsTerminal = input.IsTerminal,
            NextStepOnApprove = input.NextStepOnApprove,
            NextStepOnReject = input.NextStepOnReject,
            IsCommentRequired = input.IsCommentRequired,
            AssignedUserId = input.AssignedUserId,
            SlaHours = input.SlaHours,
            GuardKey = Key(input.GuardKey),
            EntryEffectKey = Key(input.EntryEffectKey),
            ExitEffectKey = Key(input.ExitEffectKey),
            DecisionSchemaKey = Key(input.DecisionSchemaKey),
            IsOptional = input.IsOptional
        };

        await _stepRepository.InsertAsync(step);

        // Bump definition version
        definition.BumpVersion();
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<WorkflowStepDto>(step);
    }

    [AbpAuthorize(PermissionNames.Workflow_Definitions_Edit)]
    public async Task<WorkflowStepDto> UpdateAsync(Guid id, UpdateWorkflowStepDto input)
    {
        var step = await _stepRepository
            .GetAll()
            .Include(s => s.WorkflowDefinition)
            .Where(s => s.WorkflowDefinition.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (step == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.StepNotFound,
                "Workflow step not found.");

        await EnsureNoActiveInstancesAsync(step.WorkflowDefinitionId);

        if (input.Name != null) step.Name = input.Name.Trim();
        if (input.Description != null) step.Description = input.Description.Trim();
        if (input.AssignedRole != null) step.AssignedRole = input.AssignedRole.Trim();
        if (input.ActionType.HasValue) step.ActionType = input.ActionType.Value;
        if (input.IsTerminal.HasValue) step.IsTerminal = input.IsTerminal.Value;
        if (input.IsCommentRequired.HasValue) step.IsCommentRequired = input.IsCommentRequired.Value;

        if (input.ClearNextStepOnApprove) step.NextStepOnApprove = null;
        else if (input.NextStepOnApprove.HasValue) step.NextStepOnApprove = input.NextStepOnApprove;

        if (input.ClearNextStepOnReject) step.NextStepOnReject = null;
        else if (input.NextStepOnReject.HasValue) step.NextStepOnReject = input.NextStepOnReject;

        if (input.ClearAssignedUserId) step.AssignedUserId = null;
        else if (input.AssignedUserId.HasValue) step.AssignedUserId = input.AssignedUserId;

        if (input.ClearSlaHours) step.SlaHours = null;
        else if (input.SlaHours.HasValue) step.SlaHours = input.SlaHours;

        if (input.ClearGuardKey) step.GuardKey = null;
        else if (input.GuardKey != null) step.GuardKey = Key(input.GuardKey);
        if (input.ClearEntryEffectKey) step.EntryEffectKey = null;
        else if (input.EntryEffectKey != null) step.EntryEffectKey = Key(input.EntryEffectKey);
        if (input.ClearExitEffectKey) step.ExitEffectKey = null;
        else if (input.ExitEffectKey != null) step.ExitEffectKey = Key(input.ExitEffectKey);
        if (input.ClearDecisionSchemaKey) step.DecisionSchemaKey = null;
        else if (input.DecisionSchemaKey != null) step.DecisionSchemaKey = Key(input.DecisionSchemaKey);
        if (input.IsOptional.HasValue) step.IsOptional = input.IsOptional.Value;
        EnsureExtensionKeysExist(step.WorkflowDefinition.EntityType, step.GuardKey, step.EntryEffectKey, step.ExitEffectKey, step.DecisionSchemaKey);

        // WF-05: validate the FINAL state — a user-pinned step (after applying any
        // role/user change above) requires that user to hold the step's role.
        if (step.AssignedUserId.HasValue)
            await EnsureAssignedUserHoldsRoleAsync(step.AssignedUserId.Value, step.AssignedRole);

        // Bump definition version
        step.WorkflowDefinition.BumpVersion();

        await _stepRepository.UpdateAsync(step);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<WorkflowStepDto>(step);
    }

    [AbpAuthorize(PermissionNames.Workflow_Definitions_Edit)]
    public async Task DeleteAsync(Guid id)
    {
        var step = await _stepRepository
            .GetAll()
            .Include(s => s.WorkflowDefinition)
            .Where(s => s.WorkflowDefinition.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (step == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.StepNotFound,
                "Workflow step not found.");

        // Check if any active instances are at this step
        var inUse = await _instanceRepository
            .GetAll()
            .AnyAsync(i => i.CurrentStepId == id
                && i.TenantId == AbpSession.TenantId
                && (i.Status == WorkflowStatus.NotStarted || i.Status == WorkflowStatus.InProgress));

        if (inUse)
            throw new UserFriendlyException(WorkflowExceptionCodes.StepInUseByInstances,
                "Cannot delete a step that is currently active in a workflow instance.");

        await EnsureNoActiveInstancesAsync(step.WorkflowDefinitionId);

        // WF-33: another step routing to this one by order would be left pointing at nothing.
        var referencedBy = await _stepRepository.GetAll()
            .Where(s => s.WorkflowDefinitionId == step.WorkflowDefinitionId && s.Id != step.Id
                && (s.NextStepOnApprove == step.StepOrder || s.NextStepOnReject == step.StepOrder))
            .Select(s => s.Name)
            .ToListAsync();
        if (referencedBy.Count > 0)
            throw new UserFriendlyException(WorkflowExceptionCodes.InvalidNextStep,
                $"Step '{step.Name}' is the next-step target of: {string.Join(", ", referencedBy)}. Re-route those steps first.");

        // Bump definition version
        step.WorkflowDefinition.BumpVersion();

        await _stepRepository.DeleteAsync(step);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Workflow_Definitions_Edit)]
    public async Task<List<WorkflowStepDto>> ReorderAsync(ReorderWorkflowStepsDto input)
    {
        // Verify definition belongs to tenant
        var definitionExists = await _definitionRepository
            .GetAll()
            .AnyAsync(d => d.Id == input.WorkflowDefinitionId && d.TenantId == AbpSession.TenantId);

        if (!definitionExists)
            throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionNotFound,
                "Workflow definition not found.");

        var steps = await _stepRepository
            .GetAll()
            .Where(s => s.WorkflowDefinitionId == input.WorkflowDefinitionId)
            .ToListAsync();

        // Verify all step IDs match
        if (steps.Count != input.StepIds.Count || !steps.All(s => input.StepIds.Contains(s.Id)))
            throw new UserFriendlyException(WorkflowExceptionCodes.InvalidNextStep,
                "Step IDs do not match the steps in this definition.");

        await EnsureNoActiveInstancesAsync(input.WorkflowDefinitionId);

        // WF-33: NextStepOnApprove / NextStepOnReject are stored as ORDERS, so they
        // must follow their target step to its new position or the graph is rewired.
        var oldToNew = new Dictionary<int, int>();
        for (var i = 0; i < input.StepIds.Count; i++)
            oldToNew[steps.First(s => s.Id == input.StepIds[i]).StepOrder] = i + 1;

        foreach (var step in steps)
        {
            if (step.NextStepOnApprove.HasValue && oldToNew.TryGetValue(step.NextStepOnApprove.Value, out var a)) step.NextStepOnApprove = a;
            if (step.NextStepOnReject.HasValue && oldToNew.TryGetValue(step.NextStepOnReject.Value, out var r)) step.NextStepOnReject = r;
        }
        // Two passes so the unique (DefinitionId, StepOrder) index is never hit mid-way.
        foreach (var step in steps) step.StepOrder = -oldToNew[step.StepOrder];
        await CurrentUnitOfWork.SaveChangesAsync();
        foreach (var step in steps) step.StepOrder = -step.StepOrder;

        var definition = await _definitionRepository.GetAsync(input.WorkflowDefinitionId);
        definition.BumpVersion();
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetByDefinitionAsync(input.WorkflowDefinitionId);
    }
}
