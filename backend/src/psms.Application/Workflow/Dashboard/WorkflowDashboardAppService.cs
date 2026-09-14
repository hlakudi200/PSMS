using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Authorization.Users;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Dashboard.Dto;
using psms.Workflow.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Dashboard;

[AbpAuthorize(PermissionNames.Workflow_Instances_View)]
public class WorkflowDashboardAppService : ApplicationService, IWorkflowDashboardAppService
{
    private readonly IRepository<WorkflowInstance, Guid> _instanceRepository;
    private readonly IRepository<WorkflowTransition, Guid> _transitionRepository;
    private readonly IRepository<WorkflowDelegation, Guid> _delegationRepository;
    private readonly UserManager _userManager;
    private readonly WorkflowActorResolver _actors;

    public WorkflowDashboardAppService(
        IRepository<WorkflowInstance, Guid> instanceRepository,
        IRepository<WorkflowTransition, Guid> transitionRepository,
        IRepository<WorkflowDelegation, Guid> delegationRepository,
        UserManager userManager,
        WorkflowActorResolver actors)
    {
        _actors = actors;
        _instanceRepository = instanceRepository;
        _transitionRepository = transitionRepository;
        _delegationRepository = delegationRepository;
        _userManager = userManager;
    }

    [AbpAuthorize(PermissionNames.Workflow_Instances_View)]
    public async Task<WorkflowDashboardDto> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var ninetyDaysAgo = now.AddDays(-90);

        var allInstances = await _instanceRepository
            .GetAll()
            .Include(i => i.CurrentStep)
            .Where(i => i.TenantId == AbpSession.TenantId)
            .ToListAsync();

        var dashboard = new WorkflowDashboardDto();

        // --- Top-level counts ---
        dashboard.TotalActive = allInstances.Count(i =>
            i.Status == WorkflowStatus.NotStarted || i.Status == WorkflowStatus.InProgress);

        dashboard.TotalCompleted = allInstances.Count(i => i.Status == WorkflowStatus.Completed);

        dashboard.TotalRejected = allInstances.Count(i => i.Status == WorkflowStatus.Rejected);

        dashboard.TotalOverdue = allInstances.Count(i =>
            i.Status == WorkflowStatus.InProgress
            && i.CurrentStepDueDate.HasValue
            && i.CurrentStepDueDate.Value < now);

        // --- Average completion hours (last 90 days) ---
        var recentCompleted = allInstances
            .Where(i => i.Status == WorkflowStatus.Completed
                && i.CompletedDate.HasValue
                && i.StartedDate.HasValue
                && i.CompletedDate.Value >= ninetyDaysAgo)
            .ToList();

        if (recentCompleted.Any())
        {
            dashboard.AverageCompletionHours = Math.Round(
                recentCompleted.Average(i => (i.CompletedDate.Value - i.StartedDate.Value).TotalHours), 2);
        }

        // --- Breakdown by entity type ---
        dashboard.ByEntityType = allInstances
            .GroupBy(i => i.EntityType)
            .Select(g => new WorkflowEntityTypeSummaryDto
            {
                EntityType = g.Key,
                ActiveCount = g.Count(i =>
                    i.Status == WorkflowStatus.NotStarted || i.Status == WorkflowStatus.InProgress),
                CompletedCount = g.Count(i => i.Status == WorkflowStatus.Completed),
                RejectedCount = g.Count(i => i.Status == WorkflowStatus.Rejected),
                OverdueCount = g.Count(i =>
                    i.Status == WorkflowStatus.InProgress
                    && i.CurrentStepDueDate.HasValue
                    && i.CurrentStepDueDate.Value < now)
            })
            .OrderBy(s => s.EntityType)
            .ToList();

        // --- Breakdown by role ---
        var inProgressWithStep = allInstances
            .Where(i => i.Status == WorkflowStatus.InProgress && i.CurrentStep != null)
            .ToList();

        dashboard.ByRole = inProgressWithStep
            .GroupBy(i => i.CurrentStep.AssignedRole)
            .Select(g => new WorkflowRoleSummaryDto
            {
                RoleName = g.Key,
                PendingCount = g.Count(),
                OverdueCount = g.Count(i =>
                    i.CurrentStepDueDate.HasValue && i.CurrentStepDueDate.Value < now)
            })
            .OrderByDescending(r => r.PendingCount)
            .ToList();

        // --- My pending count ---
        dashboard.MyPendingCount = await GetMyPendingCountAsync(inProgressWithStep, now);

        return dashboard;
    }

    [AbpAuthorize(PermissionNames.Workflow_Instances_View)]
    public async Task<List<WorkflowActivityDto>> GetRecentActivityAsync(int count = 20)
    {
        if (count <= 0) count = 20;
        if (count > 100) count = 100;

        var transitions = await _transitionRepository
            .GetAll()
            .Include(t => t.FromStep)
            .Include(t => t.ToStep)
            .Include(t => t.WorkflowInstance)
                .ThenInclude(i => i.WorkflowDefinition)
            .Where(t => t.TenantId == AbpSession.TenantId)
            .OrderByDescending(t => t.TransitionDate)
            .Take(count)
            .ToListAsync();

        return transitions.Select(t => new WorkflowActivityDto
        {
            WorkflowInstanceId = t.WorkflowInstanceId,
            EntityType = t.WorkflowInstance.EntityType,
            EntityId = t.WorkflowInstance.EntityId,
            WorkflowDefinitionName = t.WorkflowInstance.WorkflowDefinition?.Name,
            Action = t.Action,
            ActorUserName = t.ActorUserName,
            FromStepName = t.FromStep?.Name,
            ToStepName = t.ToStep?.Name,
            Comment = t.Comment,
            TransitionDate = t.TransitionDate
        }).ToList();
    }

    /// <summary>WF-34: same rule as the act check and My Approvals (WorkflowActorResolver).</summary>
    private async Task<int> GetMyPendingCountAsync(List<WorkflowInstance> inProgressInstances, DateTime now)
    {
        var actor = await _actors.GetCurrentAsync();
        if (actor == null) return 0;
        return inProgressInstances.Count(i => WorkflowActorResolver.CanAct(actor, i.CurrentStep, i.EntityType));
    }
}
