using Abp.Dependency;
using Abp.UI;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Engine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Workflow.Shared;

/// <summary>
/// Bridges terminal workflow outcomes to the linked domain record by dispatching
/// to the entity type's registered <see cref="IWorkflowEntityHandler"/>.
///
/// WF-31: this used to be a switch over eight entity types that swallowed every
/// failure with a log line, so a workflow could read Completed while the record
/// stayed untouched. It now runs INSIDE the advance transaction and lets the
/// handler's UserFriendlyException propagate, rolling the transition back. An
/// entity type without a handler is a configuration error, not a silent no-op.
/// </summary>
public class WorkflowEntityBridgeService : ITransientDependency
{
    private readonly WorkflowExtensionRegistry _registry;

    public WorkflowEntityBridgeService(WorkflowExtensionRegistry registry)
    {
        _registry = registry;
    }

    public Task OnWorkflowCompletedAsync(WorkflowEffectContext context) =>
        Handler(context.EntityType).OnCompletedAsync(context);

    public Task OnWorkflowRejectedAsync(WorkflowEffectContext context) =>
        Handler(context.EntityType).OnRejectedAsync(context);

    public Task OnWorkflowCancelledAsync(WorkflowEffectContext context) =>
        Handler(context.EntityType).OnCancelledAsync(context);

    public Task OnWorkflowRecalledAsync(WorkflowEffectContext context) =>
        Handler(context.EntityType).OnRecalledAsync(context);

    /// <summary>Convenience for callers that only have the raw pieces (kept for source compatibility).</summary>
    public Task OnWorkflowCompletedAsync(WorkflowEntityType entityType, Guid entityId, long approvedByUserId, int? tenantId = null,
        string comment = null, IDictionary<string, object> decision = null) =>
        OnWorkflowCompletedAsync(new WorkflowEffectContext
        {
            TenantId = tenantId, EntityType = entityType, EntityId = entityId, ActorUserId = approvedByUserId,
            Comment = comment, Decision = new WorkflowDecision(decision),
        });

    public Task OnWorkflowRejectedAsync(WorkflowEntityType entityType, Guid entityId, long rejectedByUserId, string reason, int? tenantId = null) =>
        OnWorkflowRejectedAsync(new WorkflowEffectContext
        {
            TenantId = tenantId, EntityType = entityType, EntityId = entityId, ActorUserId = rejectedByUserId,
            Comment = reason, Decision = WorkflowDecision.Empty,
        });

    private IWorkflowEntityHandler Handler(WorkflowEntityType entityType)
    {
        var handler = _registry.GetEntityHandler(entityType);
        if (handler == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.ExtensionNotFound,
                $"No workflow handler is registered for {entityType}; its workflow cannot write back to the record.");
        return handler;
    }
}
