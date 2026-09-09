using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Shared;

/// <summary>
/// WF-37: every 30 minutes, notify the assignees of every in-progress step that
/// has passed its SLA. Runs across all tenants (the tenant filter is disabled for
/// the scan and re-applied per tenant for the sends). Each (instance, step) is
/// notified once — the dispatcher's idempotency key skips repeats.
/// </summary>
public class WorkflowSlaBreachWorker : AsyncPeriodicBackgroundWorkerBase, ISingletonDependency
{
    private const int PeriodMilliseconds = 30 * 60 * 1000;

    private readonly IRepository<WorkflowInstance, Guid> _instances;
    private readonly WorkflowNotifier _notifier;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public WorkflowSlaBreachWorker(
        AbpAsyncTimer timer,
        IRepository<WorkflowInstance, Guid> instances,
        WorkflowNotifier notifier,
        IUnitOfWorkManager unitOfWorkManager)
        : base(timer)
    {
        _instances = instances;
        _notifier = notifier;
        _unitOfWorkManager = unitOfWorkManager;
        Timer.Period = PeriodMilliseconds;
        Timer.RunOnStart = false;
    }

    protected override async Task DoWorkAsync()
    {
        using (var uow = _unitOfWorkManager.Begin())
        {
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var now = DateTime.UtcNow;
                var overdue = await _instances.GetAll()
                    .Include(i => i.CurrentStep)
                    .Include(i => i.WorkflowDefinition)
                    .Where(i => i.Status == WorkflowStatus.InProgress
                        && i.CurrentStepDueDate.HasValue
                        && i.CurrentStepDueDate.Value < now
                        && i.CurrentStep != null)
                    .ToListAsync();

                foreach (var group in overdue.GroupBy(i => i.TenantId))
                {
                    using (_unitOfWorkManager.Current.SetTenantId(group.Key))
                    {
                        foreach (var instance in group)
                            await _notifier.SlaBreachedAsync(instance, instance.CurrentStep, instance.WorkflowDefinition?.Name ?? "Workflow");
                    }
                }
            }

            await uow.CompleteAsync();
        }
    }
}
