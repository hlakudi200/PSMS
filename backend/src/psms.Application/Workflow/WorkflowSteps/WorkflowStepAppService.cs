using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Authorization.Users;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
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

    public WorkflowStepAppService(
        IRepository<WorkflowStep, Guid> stepRepository,
        IRepository<WorkflowDefinition, Guid> definitionRepository,
        IRepository<WorkflowInstance, Guid> instanceRepository,
        UserManager userManager)
    {
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
            GuardExpression = input.GuardExpression?.Trim()
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

        if (input.ClearGuardExpression) step.GuardExpression = null;
        else if (input.GuardExpression != null) step.GuardExpression = input.GuardExpression.Trim();

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

        // Assign new order
        for (var i = 0; i < input.StepIds.Count; i++)
        {
            var step = steps.First(s => s.Id == input.StepIds[i]);
            step.StepOrder = i + 1;
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetByDefinitionAsync(input.WorkflowDefinitionId);
    }
}
