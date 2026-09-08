using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using System;
using System.Threading.Tasks;

namespace psms.Workflow.Shared;

/// <summary>
/// WF-36: one approval path. While a record has an active workflow instance, its
/// own Approve / Reject endpoints refuse, so a decision can only be taken from
/// the workflow step (where guards, decision fields and history apply). When no
/// definition is active for the tenant there is no instance and the direct
/// endpoints keep working as a manual fallback.
/// </summary>
public class WorkflowInstanceGuard : ITransientDependency
{
    private readonly IRepository<WorkflowInstance, Guid> _instances;
    private readonly IAbpSession _session;

    public WorkflowInstanceGuard(IRepository<WorkflowInstance, Guid> instances, IAbpSession session)
    {
        _instances = instances;
        _session = session;
    }

    public async Task EnsureNoActiveInstanceAsync(WorkflowEntityType entityType, Guid entityId)
    {
        var tenantId = _session.TenantId;
        var active = await _instances.GetAll().AnyAsync(i =>
            i.TenantId == tenantId
            && i.EntityType == entityType
            && i.EntityId == entityId
            && (i.Status == WorkflowStatus.NotStarted || i.Status == WorkflowStatus.InProgress));

        if (active)
            throw new UserFriendlyException(WorkflowExceptionCodes.EntityHasActiveWorkflow,
                "This record is in an approval workflow. Take the decision from My Approvals, or cancel the workflow first.");
    }
}
