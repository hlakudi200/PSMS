using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Authorization.Users;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Shared;
using psms.Workflow.WorkflowInstances.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Workflow.WorkflowInstances;

[AbpAuthorize(PermissionNames.Workflow_Instances)]
public class WorkflowInstanceAppService : ApplicationService, IWorkflowInstanceAppService
{
    private readonly IRepository<WorkflowInstance, Guid> _instanceRepository;
    private readonly IRepository<WorkflowDefinition, Guid> _definitionRepository;
    private readonly IRepository<WorkflowStep, Guid> _stepRepository;
    private readonly IRepository<WorkflowTransition, Guid> _transitionRepository;
    private readonly IRepository<WorkflowDelegation, Guid> _delegationRepository;
    private readonly UserManager _userManager;
    private readonly WorkflowEntityBridgeService _bridgeService;

    public WorkflowInstanceAppService(
        IRepository<WorkflowInstance, Guid> instanceRepository,
        IRepository<WorkflowDefinition, Guid> definitionRepository,
        IRepository<WorkflowStep, Guid> stepRepository,
        IRepository<WorkflowTransition, Guid> transitionRepository,
        IRepository<WorkflowDelegation, Guid> delegationRepository,
        UserManager userManager,
        WorkflowEntityBridgeService bridgeService)
    {
        _instanceRepository = instanceRepository;
        _definitionRepository = definitionRepository;
        _stepRepository = stepRepository;
        _transitionRepository = transitionRepository;
        _delegationRepository = delegationRepository;
        _userManager = userManager;
        _bridgeService = bridgeService;
    }

    [AbpAuthorize(PermissionNames.Workflow_Instances_View)]
    public async Task<WorkflowInstanceDto> GetAsync(Guid id)
    {
        var instance = await _instanceRepository
            .GetAll()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentStep)
            .Include(i => i.Transitions.OrderBy(t => t.TransitionDate))
                .ThenInclude(t => t.FromStep)
            .Include(i => i.Transitions)
                .ThenInclude(t => t.ToStep)
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == AbpSession.TenantId);

        if (instance == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.InstanceNotFound,
                "Workflow instance not found.");

        return ObjectMapper.Map<WorkflowInstanceDto>(instance);
    }

    [AbpAuthorize(PermissionNames.Workflow_Instances_View)]
    public async Task<WorkflowInstanceDto> GetByEntityAsync(WorkflowEntityType entityType, Guid entityId)
    {
        var instance = await _instanceRepository
            .GetAll()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentStep)
            .Include(i => i.Transitions.OrderBy(t => t.TransitionDate))
                .ThenInclude(t => t.FromStep)
            .Include(i => i.Transitions)
                .ThenInclude(t => t.ToStep)
            .FirstOrDefaultAsync(i => i.TenantId == AbpSession.TenantId
                && i.EntityType == entityType
                && i.EntityId == entityId
                && (i.Status == WorkflowStatus.NotStarted || i.Status == WorkflowStatus.InProgress));

        if (instance == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.InstanceNotFound,
                "No active workflow instance found for this entity.");

        return ObjectMapper.Map<WorkflowInstanceDto>(instance);
    }

    [AbpAuthorize(PermissionNames.Workflow_Instances_View)]
    public async Task<PagedResultDto<WorkflowInstanceListDto>> GetAllAsync(GetWorkflowInstancesInput input)
    {
        var query = _instanceRepository
            .GetAll()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentStep)
            .Where(i => i.TenantId == AbpSession.TenantId)
            .WhereIf(input.EntityType.HasValue, i => i.EntityType == input.EntityType.Value)
            .WhereIf(input.Status.HasValue, i => i.Status == input.Status.Value)
            .WhereIf(input.EntityId.HasValue, i => i.EntityId == input.EntityId.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                i => i.WorkflowDefinition.Name.ToLower().Contains(input.Search.Trim().ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<WorkflowInstanceListDto>(
            totalCount,
            ObjectMapper.Map<List<WorkflowInstanceListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Workflow_Instances_Start)]
    public async Task<WorkflowInstanceDto> StartAsync(StartWorkflowInput input)
    {
        // Check for existing active instance
        var existingInstance = await _instanceRepository
            .GetAll()
            .AnyAsync(i => i.TenantId == AbpSession.TenantId
                && i.EntityType == input.EntityType
                && i.EntityId == input.EntityId
                && (i.Status == WorkflowStatus.NotStarted || i.Status == WorkflowStatus.InProgress));

        if (existingInstance)
            throw new UserFriendlyException(WorkflowExceptionCodes.InstanceAlreadyExists,
                "An active workflow already exists for this entity.");

        // Resolve definition
        WorkflowDefinition definition;
        if (input.WorkflowDefinitionId.HasValue)
        {
            definition = await _definitionRepository
                .GetAll()
                .Include(d => d.Steps)
                .FirstOrDefaultAsync(d => d.Id == input.WorkflowDefinitionId.Value
                    && d.TenantId == AbpSession.TenantId);

            if (definition == null)
                throw new UserFriendlyException(WorkflowExceptionCodes.DefinitionNotFound,
                    "Workflow definition not found.");
        }
        else
        {
            // Use active definition for the entity type
            definition = await _definitionRepository
                .GetAll()
                .Include(d => d.Steps)
                .FirstOrDefaultAsync(d => d.TenantId == AbpSession.TenantId
                    && d.EntityType == input.EntityType
                    && d.IsActive);

            if (definition == null)
                throw new UserFriendlyException(WorkflowExceptionCodes.NoActiveDefinition,
                    "No active workflow definition found for this entity type. Please configure one first.");
        }

        // Get first step
        var firstStep = definition.Steps.OrderBy(s => s.StepOrder).FirstOrDefault();
        if (firstStep == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.StepNotFound,
                "Workflow definition has no steps configured.");

        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            TenantId = AbpSession.TenantId,
            WorkflowDefinitionId = definition.Id,
            WorkflowDefinitionVersion = definition.Version,
            EntityType = input.EntityType,
            EntityId = input.EntityId,
            CurrentStepDueDate = firstStep.SlaHours.HasValue
                ? DateTime.UtcNow.AddHours(firstStep.SlaHours.Value)
                : null
        };

        instance.Start(firstStep.Id);

        // Create initial transition
        var transition = new WorkflowTransition
        {
            Id = Guid.NewGuid(),
            TenantId = AbpSession.TenantId,
            WorkflowInstanceId = instance.Id,
            FromStepId = firstStep.Id,
            ToStepId = firstStep.Id,
            Action = WorkflowActionType.Submit,
            ActorUserId = AbpSession.UserId.Value,
            ActorUserName = await GetCurrentUserName(),
            Comment = "Workflow started.",
            TransitionDate = DateTime.UtcNow
        };

        await _instanceRepository.InsertAsync(instance);
        await _transitionRepository.InsertAsync(transition);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(instance.Id);
    }

    [AbpAuthorize(PermissionNames.Workflow_Instances_Advance)]
    public async Task<WorkflowInstanceDto> AdvanceAsync(Guid instanceId, AdvanceWorkflowInput input)
    {
        var instance = await _instanceRepository
            .GetAll()
            .Include(i => i.WorkflowDefinition)
                .ThenInclude(d => d.Steps)
            .Include(i => i.CurrentStep)
            .FirstOrDefaultAsync(i => i.Id == instanceId && i.TenantId == AbpSession.TenantId);

        if (instance == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.InstanceNotFound,
                "Workflow instance not found.");

        if (instance.Status != WorkflowStatus.InProgress)
            throw new UserFriendlyException(WorkflowExceptionCodes.InstanceNotInProgress,
                "Workflow is not in progress.");

        var currentStep = instance.CurrentStep;
        if (currentStep == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.StepNotFound,
                "Current step not found.");

        // Role/user validation — ensure current user is authorized for this step
        await ValidateUserCanActOnStep(currentStep);

        // Comment-required validation
        if (currentStep.IsCommentRequired && string.IsNullOrWhiteSpace(input.Comment))
            throw new UserFriendlyException(WorkflowExceptionCodes.CommentRequired,
                "A comment is required for this step.");

        // Validate action is appropriate for step
        if (input.Action != WorkflowActionType.Approve
            && input.Action != WorkflowActionType.Reject
            && input.Action != WorkflowActionType.Review
            && input.Action != WorkflowActionType.Revise)
        {
            throw new UserFriendlyException(WorkflowExceptionCodes.InvalidTransition,
                "Invalid action. Use Approve, Reject, Review, or Revise.");
        }

        var allSteps = instance.WorkflowDefinition.Steps.OrderBy(s => s.StepOrder).ToList();
        Guid? toStepId = null;
        WorkflowStep nextTargetStep = null;

        if (input.Action == WorkflowActionType.Approve)
        {
            if (currentStep.IsTerminal)
            {
                instance.Complete(AbpSession.UserId.Value, input.Comment);
            }
            else
            {
                var nextOrder = currentStep.NextStepOnApprove ?? (currentStep.StepOrder + 1);
                var nextStep = allSteps.FirstOrDefault(s => s.StepOrder == nextOrder);

                if (nextStep == null)
                {
                    instance.Complete(AbpSession.UserId.Value, input.Comment);
                }
                else
                {
                    instance.AdvanceTo(nextStep.StepOrder, nextStep.Id, nextStep.SlaHours);
                    toStepId = nextStep.Id;
                    nextTargetStep = nextStep;
                }
            }
        }
        else if (input.Action == WorkflowActionType.Reject)
        {
            if (currentStep.NextStepOnReject.HasValue)
            {
                var rejectStep = allSteps.FirstOrDefault(s => s.StepOrder == currentStep.NextStepOnReject.Value);
                if (rejectStep != null)
                {
                    instance.AdvanceTo(rejectStep.StepOrder, rejectStep.Id, rejectStep.SlaHours);
                    toStepId = rejectStep.Id;
                }
                else
                {
                    instance.Reject(AbpSession.UserId.Value, input.Comment);
                }
            }
            else
            {
                instance.Reject(AbpSession.UserId.Value, input.Comment);
            }
        }
        else if (input.Action == WorkflowActionType.Review || input.Action == WorkflowActionType.Revise)
        {
            toStepId = currentStep.Id;
        }

        // Record the transition
        var transition = new WorkflowTransition
        {
            Id = Guid.NewGuid(),
            TenantId = AbpSession.TenantId,
            WorkflowInstanceId = instance.Id,
            FromStepId = currentStep.Id,
            ToStepId = toStepId,
            Action = input.Action,
            ActorUserId = AbpSession.UserId.Value,
            ActorUserName = await GetCurrentUserName(),
            Comment = input.Comment,
            AttachmentUrl = input.AttachmentUrl,
            TransitionDate = DateTime.UtcNow
        };

        await _instanceRepository.UpdateAsync(instance);
        await _transitionRepository.InsertAsync(transition);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Bridge: sync workflow result to the linked domain entity
        if (instance.Status == WorkflowStatus.Completed)
        {
            await _bridgeService.OnWorkflowCompletedAsync(
                instance.EntityType, instance.EntityId, AbpSession.UserId.Value);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        else if (instance.Status == WorkflowStatus.Rejected)
        {
            await _bridgeService.OnWorkflowRejectedAsync(
                instance.EntityType, instance.EntityId, AbpSession.UserId.Value, input.Comment);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        return await GetAsync(instanceId);
    }

    [AbpAuthorize(PermissionNames.Workflow_Instances_Cancel)]
    public async Task<WorkflowInstanceDto> CancelAsync(Guid instanceId, string comment)
    {
        var instance = await _instanceRepository
            .FirstOrDefaultAsync(i => i.Id == instanceId && i.TenantId == AbpSession.TenantId);

        if (instance == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.InstanceNotFound,
                "Workflow instance not found.");

        try
        {
            instance.Cancel(AbpSession.UserId.Value, comment);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(WorkflowExceptionCodes.InvalidTransition, ex.Message);
        }

        // Record cancellation transition
        if (instance.CurrentStepId.HasValue)
        {
            var transition = new WorkflowTransition
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                WorkflowInstanceId = instance.Id,
                FromStepId = instance.CurrentStepId.Value,
                ToStepId = null,
                Action = WorkflowActionType.Cancel,
                ActorUserId = AbpSession.UserId.Value,
                ActorUserName = await GetCurrentUserName(),
                Comment = comment,
                TransitionDate = DateTime.UtcNow
            };

            await _transitionRepository.InsertAsync(transition);
        }

        await _instanceRepository.UpdateAsync(instance);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(instanceId);
    }

    [AbpAuthorize(PermissionNames.Workflow_Instances_Recall)]
    public async Task<WorkflowInstanceDto> RecallAsync(Guid instanceId, string comment)
    {
        var instance = await _instanceRepository
            .FirstOrDefaultAsync(i => i.Id == instanceId && i.TenantId == AbpSession.TenantId);

        if (instance == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.InstanceNotFound,
                "Workflow instance not found.");

        try
        {
            instance.Recall(AbpSession.UserId.Value, comment);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(WorkflowExceptionCodes.RecallNotAllowed, ex.Message);
        }

        // Record recall transition
        if (instance.CurrentStepId.HasValue)
        {
            var transition = new WorkflowTransition
            {
                Id = Guid.NewGuid(),
                TenantId = AbpSession.TenantId,
                WorkflowInstanceId = instance.Id,
                FromStepId = instance.CurrentStepId.Value,
                ToStepId = null,
                Action = WorkflowActionType.Recall,
                ActorUserId = AbpSession.UserId.Value,
                ActorUserName = await GetCurrentUserName(),
                Comment = comment,
                TransitionDate = DateTime.UtcNow
            };

            await _transitionRepository.InsertAsync(transition);
        }

        await _instanceRepository.UpdateAsync(instance);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(instanceId);
    }

    [AbpAuthorize(PermissionNames.Workflow_Instances_BatchAdvance)]
    public async Task<BatchAdvanceResultDto> BatchAdvanceAsync(BatchAdvanceInput input)
    {
        var result = new BatchAdvanceResultDto();

        foreach (var instanceId in input.InstanceIds)
        {
            try
            {
                await AdvanceAsync(instanceId, new AdvanceWorkflowInput
                {
                    Action = input.Action,
                    Comment = input.Comment
                });
                result.SuccessCount++;
            }
            catch (UserFriendlyException ex)
            {
                result.FailedCount++;
                result.Failures.Add(new BatchAdvanceFailureDto
                {
                    InstanceId = instanceId,
                    Error = ex.Message
                });
            }
        }

        return result;
    }

    [AbpAuthorize(PermissionNames.Workflow_Instances_View)]
    public async Task<PagedResultDto<WorkflowInstanceListDto>> GetOverdueAsync(PagedAndSortedResultRequestDto input)
    {
        var now = DateTime.UtcNow;
        var query = _instanceRepository
            .GetAll()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentStep)
            .Where(i => i.TenantId == AbpSession.TenantId
                && i.Status == WorkflowStatus.InProgress
                && i.CurrentStepDueDate.HasValue
                && i.CurrentStepDueDate.Value < now);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CurrentStepDueDate ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<WorkflowInstanceListDto>(
            totalCount,
            ObjectMapper.Map<List<WorkflowInstanceListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Workflow_Instances_ViewHistory)]
    public async Task<List<WorkflowTransitionDto>> GetHistoryAsync(Guid instanceId)
    {
        // Verify instance belongs to tenant
        var instanceExists = await _instanceRepository
            .GetAll()
            .AnyAsync(i => i.Id == instanceId && i.TenantId == AbpSession.TenantId);

        if (!instanceExists)
            throw new UserFriendlyException(WorkflowExceptionCodes.InstanceNotFound,
                "Workflow instance not found.");

        var transitions = await _transitionRepository
            .GetAll()
            .Include(t => t.FromStep)
            .Include(t => t.ToStep)
            .Where(t => t.WorkflowInstanceId == instanceId && t.TenantId == AbpSession.TenantId)
            .OrderBy(t => t.TransitionDate)
            .ToListAsync();

        return ObjectMapper.Map<List<WorkflowTransitionDto>>(transitions);
    }

    [AbpAuthorize(PermissionNames.Workflow_Instances_View)]
    public async Task<PagedResultDto<WorkflowInstanceListDto>> GetPendingForRoleAsync(
        string roleName, PagedAndSortedResultRequestDto input)
    {
        var query = _instanceRepository
            .GetAll()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentStep)
            .Where(i => i.TenantId == AbpSession.TenantId
                && i.Status == WorkflowStatus.InProgress
                && i.CurrentStep.AssignedRole == roleName);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "StartedDate ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<WorkflowInstanceListDto>(
            totalCount,
            ObjectMapper.Map<List<WorkflowInstanceListDto>>(items));
    }

    /// <summary>
    /// WF-03: the "my tasks" list — in-progress instances whose CURRENT step is
    /// actionable by the logged-in user. The user must hold the step's role
    /// (case-insensitively, mirroring ValidateUserCanActOnStep) AND the step must
    /// be either unassigned-to-a-specific-user or pinned to them. Unlike
    /// GetPendingForRole, which matches a role tenant-wide and ignores per-user
    /// assignment, this is scoped to the caller so e.g. a teacher only sees items
    /// they can actually act on.
    ///
    /// Requiring the role on the user-pinned branch too keeps the list in lock-step
    /// with the act-time check (WF-05): a pinned user who later loses the role is
    /// neither shown the item nor allowed to advance it.
    /// NOTE (v1): delegated items (acting on behalf of another user/role) are NOT
    /// included here yet, even though ValidateUserCanActOnStep honours delegations
    /// at action time.
    /// </summary>
    [AbpAuthorize(PermissionNames.Workflow_Instances_View)]
    public async Task<PagedResultDto<WorkflowInstanceListDto>> GetMyPendingAsync(
        PagedAndSortedResultRequestDto input)
    {
        var userId = AbpSession.UserId;
        if (!userId.HasValue)
            return new PagedResultDto<WorkflowInstanceListDto>(0, new List<WorkflowInstanceListDto>());

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        // Lower-cased so the role comparison is case-insensitive in SQL (matches
        // the case-insensitive check in ValidateUserCanActOnStep).
        var roles = (user != null
                ? await _userManager.GetRolesAsync(user)
                : new List<string>())
            .Select(r => r.ToLower())
            .ToList();

        var query = _instanceRepository
            .GetAll()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentStep)
            .Where(i => i.TenantId == AbpSession.TenantId
                && i.Status == WorkflowStatus.InProgress
                && i.CurrentStep != null
                && i.CurrentStep.AssignedRole != null
                && roles.Contains(i.CurrentStep.AssignedRole.ToLower())
                && (i.CurrentStep.AssignedUserId == null
                    || i.CurrentStep.AssignedUserId == userId.Value));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "StartedDate ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<WorkflowInstanceListDto>(
            totalCount,
            ObjectMapper.Map<List<WorkflowInstanceListDto>>(items));
    }

    /// <summary>
    /// Validates the current user is authorized to act on the given step.
    /// Checks: specific user assignment → role match → active delegation.
    /// </summary>
    private async Task ValidateUserCanActOnStep(WorkflowStep step)
    {
        var userId = AbpSession.UserId;
        if (!userId.HasValue)
            throw new UserFriendlyException(WorkflowExceptionCodes.UnauthorizedAction,
                "You must be logged in to perform this action.");

        // If step is assigned to a specific user
        if (step.AssignedUserId.HasValue)
        {
            if (step.AssignedUserId.Value == userId.Value)
            {
                // WF-05: config-time guarantees the assignee held the role, but a
                // role can be revoked afterwards — re-check so a stale assignee
                // can't act without the step's required role.
                var assignedUser = await _userManager.FindByIdAsync(userId.Value.ToString());
                var assignedRoles = assignedUser != null
                    ? await _userManager.GetRolesAsync(assignedUser)
                    : new List<string>();
                if (assignedRoles.Any(r => r.Equals(step.AssignedRole, StringComparison.OrdinalIgnoreCase)))
                    return;

                throw new UserFriendlyException(WorkflowExceptionCodes.AssignedUserMissingRole,
                    $"You no longer hold the required role '{step.AssignedRole}' for this step.");
            }

            // Check if current user has an active delegation from the assigned user
            if (await HasActiveDelegationFromUser(step.AssignedUserId.Value, userId.Value, null, null))
                return;

            throw new UserFriendlyException(WorkflowExceptionCodes.UserNotAssignedToStep,
                "This step is assigned to a specific user. You are not authorized to act on it.");
        }

        // Validate by role
        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.UnauthorizedAction,
                "User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Any(r => r.Equals(step.AssignedRole, StringComparison.OrdinalIgnoreCase)))
            return;

        // Check if someone with the required role delegated to the current user
        if (await HasActiveDelegationForRole(userId.Value, step.AssignedRole))
            return;

        throw new UserFriendlyException(WorkflowExceptionCodes.UserNotAssignedToStep,
            $"You do not have the required role '{step.AssignedRole}' to act on this step.");
    }

    /// <summary>
    /// Checks if there's an active delegation from a specific user to the current user.
    /// </summary>
    private async Task<bool> HasActiveDelegationFromUser(
        long delegatorUserId, long delegateUserId,
        WorkflowEntityType? entityType, string role)
    {
        var now = DateTime.UtcNow;
        return await _delegationRepository
            .GetAll()
            .AnyAsync(d => d.TenantId == AbpSession.TenantId
                && d.DelegatorUserId == delegatorUserId
                && d.DelegateUserId == delegateUserId
                && d.IsActive
                && d.StartDate <= now
                && d.EndDate >= now
                && (!d.EntityType.HasValue || !entityType.HasValue || d.EntityType == entityType)
                && (d.AssignedRole == null || role == null || d.AssignedRole == role));
    }

    /// <summary>
    /// Checks if any user with the required role has delegated to the current user.
    /// </summary>
    private async Task<bool> HasActiveDelegationForRole(long delegateUserId, string requiredRole)
    {
        var now = DateTime.UtcNow;
        return await _delegationRepository
            .GetAll()
            .AnyAsync(d => d.TenantId == AbpSession.TenantId
                && d.DelegateUserId == delegateUserId
                && d.IsActive
                && d.StartDate <= now
                && d.EndDate >= now
                && (d.AssignedRole == null || d.AssignedRole == requiredRole));
    }

    private async Task<string> GetCurrentUserName()
    {
        var userId = AbpSession.UserId;
        if (!userId.HasValue) return "System";

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        return user?.UserName ?? "Unknown";
    }
}
