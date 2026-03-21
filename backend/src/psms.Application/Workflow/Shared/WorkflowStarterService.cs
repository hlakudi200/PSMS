using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Shared;

/// <summary>
/// Internal service for starting workflows programmatically from other modules.
/// Does NOT require workflow permissions — used for auto-start scenarios
/// (e.g., auto-start admissions workflow when payment is received).
/// NOT an API endpoint.
/// </summary>
public class WorkflowStarterService : ITransientDependency
{
    private readonly IRepository<WorkflowInstance, Guid> _instanceRepository;
    private readonly IRepository<WorkflowDefinition, Guid> _definitionRepository;
    private readonly IRepository<WorkflowTransition, Guid> _transitionRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    public ILogger Logger { get; set; } = NullLogger.Instance;

    public WorkflowStarterService(
        IRepository<WorkflowInstance, Guid> instanceRepository,
        IRepository<WorkflowDefinition, Guid> definitionRepository,
        IRepository<WorkflowTransition, Guid> transitionRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _instanceRepository = instanceRepository;
        _definitionRepository = definitionRepository;
        _transitionRepository = transitionRepository;
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// Starts a workflow for the given entity if an active definition exists.
    /// Silently skips if no active definition is configured or if a workflow already exists.
    /// </summary>
    /// <returns>True if workflow was started, false if skipped.</returns>
    public async Task<bool> TryStartWorkflowAsync(
        int? tenantId,
        WorkflowEntityType entityType,
        Guid entityId,
        long initiatorUserId,
        string initiatorUserName)
    {
        // Check for existing active instance
        var hasExisting = await _instanceRepository
            .GetAll()
            .AnyAsync(i => i.TenantId == tenantId
                && i.EntityType == entityType
                && i.EntityId == entityId
                && (i.Status == WorkflowStatus.NotStarted || i.Status == WorkflowStatus.InProgress));

        if (hasExisting)
        {
            Logger.Info($"Workflow already exists for {entityType} {entityId}, skipping auto-start.");
            return false;
        }

        // Find active definition for entity type
        var definition = await _definitionRepository
            .GetAll()
            .Include(d => d.Steps)
            .FirstOrDefaultAsync(d => d.TenantId == tenantId
                && d.EntityType == entityType
                && d.IsActive);

        if (definition == null)
        {
            Logger.Info($"No active workflow definition for {entityType} in tenant {tenantId}, skipping auto-start.");
            return false;
        }

        var firstStep = definition.Steps.OrderBy(s => s.StepOrder).FirstOrDefault();
        if (firstStep == null)
        {
            Logger.Warn($"Workflow definition {definition.Id} has no steps, skipping auto-start.");
            return false;
        }

        var instance = new WorkflowInstance
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkflowDefinitionId = definition.Id,
            WorkflowDefinitionVersion = definition.Version,
            EntityType = entityType,
            EntityId = entityId,
            CurrentStepDueDate = firstStep.SlaHours.HasValue
                ? DateTime.UtcNow.AddHours(firstStep.SlaHours.Value)
                : null
        };

        instance.Start(firstStep.Id);

        var transition = new WorkflowTransition
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WorkflowInstanceId = instance.Id,
            FromStepId = firstStep.Id,
            ToStepId = firstStep.Id,
            Action = WorkflowActionType.Submit,
            ActorUserId = initiatorUserId,
            ActorUserName = initiatorUserName,
            Comment = "Workflow auto-started.",
            TransitionDate = DateTime.UtcNow
        };

        await _instanceRepository.InsertAsync(instance);
        await _transitionRepository.InsertAsync(transition);
        await _unitOfWorkManager.Current.SaveChangesAsync();

        Logger.Info($"Auto-started {entityType} workflow {instance.Id} for entity {entityId}.");
        return true;
    }
}
