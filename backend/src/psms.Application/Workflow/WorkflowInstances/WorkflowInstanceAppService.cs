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
using psms.Workflow.Engine;
using psms.Workflow.Shared;
using Newtonsoft.Json;
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
    private readonly WorkflowEntitySummaryProvider _entitySummaryProvider;
    private readonly WorkflowExtensionRegistry _extensions;
    private readonly WorkflowActorResolver _actors;
    private readonly WorkflowNotifier _notifier;

    public WorkflowInstanceAppService(
        IRepository<WorkflowInstance, Guid> instanceRepository,
        IRepository<WorkflowDefinition, Guid> definitionRepository,
        IRepository<WorkflowStep, Guid> stepRepository,
        IRepository<WorkflowTransition, Guid> transitionRepository,
        IRepository<WorkflowDelegation, Guid> delegationRepository,
        UserManager userManager,
        WorkflowEntityBridgeService bridgeService,
        WorkflowEntitySummaryProvider entitySummaryProvider,
        WorkflowExtensionRegistry extensions,
        WorkflowActorResolver actors,
        WorkflowNotifier notifier)
    {
        _notifier = notifier;
        _actors = actors;
        _extensions = extensions;
        _instanceRepository = instanceRepository;
        _definitionRepository = definitionRepository;
        _stepRepository = stepRepository;
        _transitionRepository = transitionRepository;
        _delegationRepository = delegationRepository;
        _userManager = userManager;
        _bridgeService = bridgeService;
        _entitySummaryProvider = entitySummaryProvider;
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

        return await ToDetailDtoAsync(instance);
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

        return await ToDetailDtoAsync(instance);
    }

    /// <summary>
    /// WF-30/32: the detail DTO carries the current step's guard status and
    /// decision schema so the UI can disable the forward action, list what is
    /// missing, and render the decision fields — without a second round trip.
    /// </summary>
    private async Task<WorkflowInstanceDto> ToDetailDtoAsync(WorkflowInstance instance)
    {
        var dto = ObjectMapper.Map<WorkflowInstanceDto>(instance);
        var step = instance.CurrentStep;
        if (step == null || instance.Status != WorkflowStatus.InProgress) return dto;

        dto.CurrentStepIsOptional = step.IsOptional;
        dto.CurrentStepGuardKey = step.GuardKey;

        if (!string.IsNullOrWhiteSpace(step.GuardKey))
        {
            var guard = _extensions.GetGuard(step.GuardKey, instance.EntityType);
            var result = await guard.EvaluateAsync(instance.EntityId);
            dto.CurrentStepGuard = new WorkflowGuardStatusDto
            {
                Key = guard.Key,
                DisplayName = guard.DisplayName,
                Satisfied = result.Satisfied,
                Message = result.Message,
            };
        }

        if (!string.IsNullOrWhiteSpace(step.DecisionSchemaKey))
        {
            var schema = _extensions.GetDecisionSchema(step.DecisionSchemaKey, instance.EntityType);
            dto.CurrentStepDecisionSchema = WorkflowDecisionSchemaDto.From(schema);
        }

        return dto;
    }

    /// <summary>WF-34: tenant-wide list — requires ViewAll (approvers use GetMyPending).</summary>
    [AbpAuthorize(PermissionNames.Workflow_Instances_ViewAll)]
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

        var dtos = ObjectMapper.Map<List<WorkflowInstanceListDto>>(items);
        await PopulateSubjectLabelsAsync(dtos);
        return new PagedResultDto<WorkflowInstanceListDto>(totalCount, dtos);
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
            // WF-33: never start on an inactive definition or one for another entity type —
            // it would route through the wrong roles and write back to the wrong record.
            if (!definition.IsActive)
                throw new UserFriendlyException(WorkflowExceptionCodes.NoActiveDefinition,
                    "That workflow definition is not active. Activate it first or start on the active one.");
            if (definition.EntityType != input.EntityType)
                throw new UserFriendlyException(WorkflowExceptionCodes.InvalidTransition,
                    $"Definition '{definition.Name}' is for {definition.EntityType}, not {input.EntityType}.");
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

        // WF-31: the first step is ENTERED at start, so its entry effect runs here
        // too — AdvanceAsync only sees steps entered by a later transition.
        if (!string.IsNullOrWhiteSpace(firstStep.EntryEffectKey))
        {
            await _extensions.GetEffect(firstStep.EntryEffectKey, input.EntityType).ApplyAsync(new WorkflowEffectContext
            {
                TenantId = AbpSession.TenantId,
                EntityType = input.EntityType,
                EntityId = input.EntityId,
                ActorUserId = AbpSession.UserId.Value,
                Comment = "Workflow started.",
                Decision = WorkflowDecision.Empty,
            });
        }

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

        // WF-37: tell the first step's assignees
        await _notifier.StepAssignedAsync(instance, firstStep, definition.Name, $"wf-start-{instance.Id}");

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
        await ValidateUserCanActOnStep(currentStep, instance.EntityType);

        // Comment-required validation
        if (currentStep.IsCommentRequired && string.IsNullOrWhiteSpace(input.Comment))
            throw new UserFriendlyException(WorkflowExceptionCodes.CommentRequired,
                "A comment is required for this step.");

        var allSteps = instance.WorkflowDefinition.Steps.OrderBy(s => s.StepOrder).ToList();
        Guid? toStepId = null;
        WorkflowStep enteredStep = null;

        // Forward actions (Submit / Review / Approve) all mean "this step is done —
        // pass it on": advance to the next step, or complete the workflow if this
        // is the terminal step. WF-30: Waive is a forward action on an OPTIONAL step
        // that skips the guard with a mandatory reason. Reject routes to the
        // configured reject step (or terminates). Revise sends the item back one
        // step for changes (stays In Progress).
        var isWaive = input.Action == WorkflowActionType.Waive;
        var isForward = isWaive
            || input.Action == WorkflowActionType.Approve
            || input.Action == WorkflowActionType.Review
            || input.Action == WorkflowActionType.Submit;

        var decision = new WorkflowDecision(input.Decision);
        var guardOverridden = false;

        if (isWaive)
        {
            if (!currentStep.IsOptional)
                throw new UserFriendlyException(WorkflowExceptionCodes.WaiveNotAllowed,
                    "This step is not optional and cannot be waived.");
            if (string.IsNullOrWhiteSpace(input.Comment))
                throw new UserFriendlyException(WorkflowExceptionCodes.CommentRequired,
                    "A reason is required to waive a step.");
        }
        else if (isForward)
        {
            // WF-30: exit criterion. A failing guard blocks the advance unless the
            // actor holds the override permission and gives a reason.
            if (!string.IsNullOrWhiteSpace(currentStep.GuardKey))
            {
                var guard = _extensions.GetGuard(currentStep.GuardKey, instance.EntityType);
                var guardResult = await guard.EvaluateAsync(instance.EntityId);
                if (!guardResult.Satisfied)
                {
                    if (!input.OverrideGuard)
                        throw new UserFriendlyException(WorkflowExceptionCodes.GuardNotSatisfied,
                            $"{guard.DisplayName}: {guardResult.Message}");
                    if (!await PermissionChecker.IsGrantedAsync(PermissionNames.Workflow_Instances_OverrideGuard))
                        throw new UserFriendlyException(WorkflowExceptionCodes.GuardOverrideNotAllowed,
                            "You are not permitted to override this step's criteria.");
                    if (string.IsNullOrWhiteSpace(input.Comment))
                        throw new UserFriendlyException(WorkflowExceptionCodes.CommentRequired,
                            "A reason is required to override the step's criteria.");
                    guardOverridden = true;
                }
            }

            // WF-32: the step's decision fields, validated by their schema.
            if (!string.IsNullOrWhiteSpace(currentStep.DecisionSchemaKey))
            {
                var schema = _extensions.GetDecisionSchema(currentStep.DecisionSchemaKey, instance.EntityType);
                var error = schema.Validate(decision);
                if (error != null)
                    throw new UserFriendlyException(WorkflowExceptionCodes.DecisionInvalid, error);
            }
        }

        var effectContext = new WorkflowEffectContext
        {
            TenantId = AbpSession.TenantId,
            EntityType = instance.EntityType,
            EntityId = instance.EntityId,
            ActorUserId = AbpSession.UserId.Value,
            Comment = input.Comment,
            Decision = decision,
        };

        if (isForward)
        {
            // WF-31: exit effect of the step being left (not on waive — the work
            // the effect records was not done).
            if (!isWaive && !string.IsNullOrWhiteSpace(currentStep.ExitEffectKey))
                await _extensions.GetEffect(currentStep.ExitEffectKey, instance.EntityType).ApplyAsync(effectContext);

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
                    if (currentStep.NextStepOnApprove.HasValue)
                        // WF-33: a configured target that does not exist is a
                        // misconfiguration, never an implicit approval.
                        throw new UserFriendlyException(WorkflowExceptionCodes.InvalidNextStep,
                            $"Step '{currentStep.Name}' routes to step order {nextOrder}, which does not exist. Fix the definition.");
                    instance.Complete(AbpSession.UserId.Value, input.Comment);
                }
                else
                {
                    instance.AdvanceTo(nextStep.StepOrder, nextStep.Id, nextStep.SlaHours);
                    toStepId = nextStep.Id;
                    enteredStep = nextStep;
                }
            }
        }
        else if (input.Action == WorkflowActionType.Reject)
        {
            if (currentStep.NextStepOnReject.HasValue)
            {
                var rejectStep = allSteps.FirstOrDefault(s => s.StepOrder == currentStep.NextStepOnReject.Value);
                if (rejectStep == null)
                    throw new UserFriendlyException(WorkflowExceptionCodes.InvalidNextStep,
                        $"Step '{currentStep.Name}' routes rejections to step order {currentStep.NextStepOnReject.Value}, which does not exist. Fix the definition.");
                instance.AdvanceTo(rejectStep.StepOrder, rejectStep.Id, rejectStep.SlaHours);
                toStepId = rejectStep.Id;
                enteredStep = rejectStep;
            }
            else
            {
                instance.Reject(AbpSession.UserId.Value, input.Comment);
            }
        }
        else if (input.Action == WorkflowActionType.Revise)
        {
            // Send back to the immediately previous step (by order) for changes.
            var previousStep = allSteps
                .Where(s => s.StepOrder < currentStep.StepOrder)
                .OrderByDescending(s => s.StepOrder)
                .FirstOrDefault();

            if (previousStep == null)
                throw new UserFriendlyException(WorkflowExceptionCodes.InvalidTransition,
                    "There is no earlier step to send this back to for revision.");

            instance.AdvanceTo(previousStep.StepOrder, previousStep.Id, previousStep.SlaHours);
            toStepId = previousStep.Id;
            enteredStep = previousStep;
        }
        else
        {
            // Cancel / Recall have their own endpoints; anything else is invalid here.
            throw new UserFriendlyException(WorkflowExceptionCodes.InvalidTransition,
                "Invalid action. Use Submit, Review, Approve, Reject, Waive, or Send for Revision.");
        }

        // WF-31: entry effect of the step being entered.
        if (enteredStep != null && !string.IsNullOrWhiteSpace(enteredStep.EntryEffectKey))
            await _extensions.GetEffect(enteredStep.EntryEffectKey, instance.EntityType).ApplyAsync(effectContext);

        // WF-31: terminal write-back runs BEFORE the save, in the same unit of work,
        // and propagates failures — the workflow never reads Completed/Rejected
        // while the record is untouched.
        if (instance.Status == WorkflowStatus.Completed)
            await _bridgeService.OnWorkflowCompletedAsync(effectContext);
        else if (instance.Status == WorkflowStatus.Rejected)
            await _bridgeService.OnWorkflowRejectedAsync(effectContext);

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
            TransitionDate = DateTime.UtcNow,
            DecisionJson = decision.IsEmpty ? null : JsonConvert.SerializeObject(decision.Raw),
            IsWaived = isWaive,
            IsGuardOverridden = guardOverridden,
        };

        await _instanceRepository.UpdateAsync(instance);
        await _transitionRepository.InsertAsync(transition);
        await CurrentUnitOfWork.SaveChangesAsync();

        // WF-37: notify the next step's assignees, or the initiator of the outcome
        var definitionName = instance.WorkflowDefinition?.Name ?? "Workflow";
        if (enteredStep != null)
            await _notifier.StepAssignedAsync(instance, enteredStep, definitionName, $"wf-step-{transition.Id}");
        else if (instance.Status == WorkflowStatus.Completed)
            await _notifier.OutcomeAsync(instance, definitionName, "approved", input.Comment, AbpSession.UserId);
        else if (instance.Status == WorkflowStatus.Rejected)
            await _notifier.OutcomeAsync(instance, definitionName, "rejected", input.Comment, AbpSession.UserId);

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

        // WF-31: a cancelled approval returns the record to its requester instead
        // of stranding it mid-flight.
        await _bridgeService.OnWorkflowCancelledAsync(BuildContext(instance, comment));

        var cancelledDefinition = await _definitionRepository.FirstOrDefaultAsync(instance.WorkflowDefinitionId);
        await _notifier.OutcomeAsync(instance, cancelledDefinition?.Name ?? "Workflow", "cancelled", comment, AbpSession.UserId);

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

        // WF-31: a recalled approval returns the record to its requester.
        await _bridgeService.OnWorkflowRecalledAsync(BuildContext(instance, comment));

        var recalledDefinition = await _definitionRepository.FirstOrDefaultAsync(instance.WorkflowDefinitionId);
        await _notifier.OutcomeAsync(instance, recalledDefinition?.Name ?? "Workflow", "recalled", comment, AbpSession.UserId);

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

    [AbpAuthorize(PermissionNames.Workflow_Instances_ViewAll)]
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

        var dtos = ObjectMapper.Map<List<WorkflowInstanceListDto>>(items);
        await PopulateSubjectLabelsAsync(dtos);
        return new PagedResultDto<WorkflowInstanceListDto>(totalCount, dtos);
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

    [AbpAuthorize(PermissionNames.Workflow_Instances_ViewAll)]
    public async Task<PagedResultDto<WorkflowInstanceListDto>> GetPendingForRoleAsync(
        string roleName, PagedAndSortedResultRequestDto input)
    {
        var query = _instanceRepository
            .GetAll()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentStep)
            .Where(i => i.TenantId == AbpSession.TenantId
                && i.Status == WorkflowStatus.InProgress
                && i.CurrentStep.AssignedRole.ToLower() == roleName.ToLower()
                // WF-34: a step pinned to another user is not pending for the role at large
                && (i.CurrentStep.AssignedUserId == null || i.CurrentStep.AssignedUserId == AbpSession.UserId));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "StartedDate ASC")
            .PageBy(input)
            .ToListAsync();

        var dtos = ObjectMapper.Map<List<WorkflowInstanceListDto>>(items);
        await PopulateSubjectLabelsAsync(dtos);
        return new PagedResultDto<WorkflowInstanceListDto>(totalCount, dtos);
    }

    /// <summary>
    /// WF-03/WF-34: the "my tasks" list — in-progress instances whose CURRENT step
    /// the logged-in user may act on, decided by the same WorkflowActorResolver the
    /// act check and the dashboard count use (direct role, pinned user, or an
    /// in-scope delegation from someone who holds the role). The candidate set is
    /// narrowed in SQL to the roles the user can act for, then filtered exactly and
    /// paged in memory — a personal inbox is small, and this removes the previous
    /// 1000-row keyword-search cap and its wrong totals.
    /// </summary>
    [AbpAuthorize(PermissionNames.Workflow_Instances_View)]
    public async Task<PagedResultDto<WorkflowInstanceListDto>> GetMyPendingAsync(
        GetMyPendingInput input)
    {
        var actor = await _actors.GetCurrentAsync();
        if (actor == null || actor.ActionableRoles.Count == 0)
            return new PagedResultDto<WorkflowInstanceListDto>(0, new List<WorkflowInstanceListDto>());

        var roles = actor.ActionableRoles.Select(r => r.ToLower()).ToList();

        var candidates = await _instanceRepository
            .GetAll()
            .Include(i => i.WorkflowDefinition)
            .Include(i => i.CurrentStep)
            .Where(i => i.TenantId == AbpSession.TenantId
                && i.Status == WorkflowStatus.InProgress
                && i.CurrentStep != null
                && i.CurrentStep.AssignedRole != null
                && roles.Contains(i.CurrentStep.AssignedRole.ToLower()))
            .OrderBy(input.Sorting ?? "StartedDate ASC")
            .ToListAsync();

        var actionable = candidates
            .Where(i => WorkflowActorResolver.CanAct(actor, i.CurrentStep, i.EntityType))
            .ToList();

        var all = ObjectMapper.Map<List<WorkflowInstanceListDto>>(actionable);
        await PopulateSubjectLabelsAsync(all);

        var keyword = input.Keyword?.Trim();
        if (!string.IsNullOrEmpty(keyword))
        {
            var k = keyword.ToLowerInvariant();
            all = all.Where(d =>
                    (!string.IsNullOrEmpty(d.SubjectLabel) && d.SubjectLabel.ToLowerInvariant().Contains(k))
                    || (!string.IsNullOrEmpty(d.WorkflowDefinitionName) && d.WorkflowDefinitionName.ToLowerInvariant().Contains(k))
                    || (!string.IsNullOrEmpty(d.CurrentStepName) && d.CurrentStepName.ToLowerInvariant().Contains(k)))
                .ToList();
        }

        var paged = all.Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
        return new PagedResultDto<WorkflowInstanceListDto>(all.Count, paged);
    }

    /// <summary>
    /// WF-09: a human-readable summary of the entity this instance is about, so
    /// an approver sees what they're approving. View-gated + tenant-scoped (same
    /// as Get); the summary itself is built from tenant-filtered entity reads.
    /// </summary>
    [AbpAuthorize(PermissionNames.Workflow_Instances_View)]
    public async Task<WorkflowEntitySummaryDto> GetEntitySummaryAsync(Guid instanceId)
    {
        var instance = await _instanceRepository
            .GetAll()
            .FirstOrDefaultAsync(i => i.Id == instanceId && i.TenantId == AbpSession.TenantId);

        if (instance == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.InstanceNotFound,
                "Workflow instance not found.");

        return await _entitySummaryProvider.GetSummaryAsync(instance.EntityType, instance.EntityId);
    }

    /// <summary>
    /// WF-20: attach a per-row subject label (e.g. the student's name for a report)
    /// to a page of list DTOs, so an approver can tell rows apart without opening
    /// each. Batched in the provider — ~1–2 lightweight queries per page, not one
    /// per row. SubjectLabel isn't AutoMapped (no source property), so it's set here.
    /// </summary>
    private async Task PopulateSubjectLabelsAsync(List<WorkflowInstanceListDto> items)
    {
        if (items == null || items.Count == 0) return;

        var entities = items
            .Select(i => new KeyValuePair<WorkflowEntityType, Guid>(i.EntityType, i.EntityId))
            .ToList();

        var labels = await _entitySummaryProvider.GetSubjectLabelsAsync(entities);

        foreach (var item in items)
            if (labels.TryGetValue(item.EntityId, out var label))
                item.SubjectLabel = label;
    }

    /// <summary>
    /// Validates the current user is authorized to act on the given step.
    /// Checks: specific user assignment → role match → active delegation.
    /// </summary>
    private WorkflowEffectContext BuildContext(WorkflowInstance instance, string comment) => new()
    {
        TenantId = AbpSession.TenantId,
        EntityType = instance.EntityType,
        EntityId = instance.EntityId,
        ActorUserId = AbpSession.UserId.Value,
        Comment = comment,
        Decision = WorkflowDecision.Empty,
    };

    /// <summary>WF-34: one resolver decides act rights for the act check, My Approvals and the dashboard.</summary>
    private async Task ValidateUserCanActOnStep(WorkflowStep step, WorkflowEntityType entityType)
    {
        var actor = await _actors.GetCurrentAsync();
        if (actor == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.UnauthorizedAction,
                "You must be logged in to perform this action.");

        if (WorkflowActorResolver.CanAct(actor, step, entityType)) return;

        var code = step.AssignedUserId == actor.UserId
            ? WorkflowExceptionCodes.AssignedUserMissingRole
            : WorkflowExceptionCodes.UserNotAssignedToStep;
        throw new UserFriendlyException(code, WorkflowActorResolver.ExplainDenial(actor, step));
    }

    private async Task<string> GetCurrentUserName()
    {
        var userId = AbpSession.UserId;
        if (!userId.HasValue) return "System";

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        return user?.UserName ?? "Unknown";
    }
}
