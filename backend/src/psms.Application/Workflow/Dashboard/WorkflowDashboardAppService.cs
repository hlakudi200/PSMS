using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Authorization.Users;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Dashboard.Dto;
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

    public WorkflowDashboardAppService(
        IRepository<WorkflowInstance, Guid> instanceRepository,
        IRepository<WorkflowTransition, Guid> transitionRepository,
        IRepository<WorkflowDelegation, Guid> delegationRepository,
        UserManager userManager)
    {
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

    private async Task<int> GetMyPendingCountAsync(List<WorkflowInstance> inProgressInstances, DateTime now)
    {
        var userId = AbpSession.UserId;
        if (!userId.HasValue) return 0;

        // Get current user's roles
        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        var userRoles = user != null
            ? await _userManager.GetRolesAsync(user)
            : (IList<string>)new List<string>();

        // Directly assigned to user OR user has the required role (when no specific user is assigned)
        var directPending = inProgressInstances.Count(i =>
            i.CurrentStep != null &&
            (i.CurrentStep.AssignedUserId == userId.Value
             || (!i.CurrentStep.AssignedUserId.HasValue
                 && userRoles.Any(r => r.Equals(i.CurrentStep.AssignedRole, StringComparison.OrdinalIgnoreCase)))));

        // Check delegated items
        var activeDelegations = await _delegationRepository
            .GetAll()
            .Where(d => d.TenantId == AbpSession.TenantId
                && d.DelegateUserId == userId.Value
                && d.IsActive
                && d.StartDate <= now
                && d.EndDate >= now)
            .ToListAsync();

        if (!activeDelegations.Any())
            return directPending;

        // Count instances that match any active delegation but are NOT already counted
        var delegatedCount = inProgressInstances.Count(i =>
            i.CurrentStep != null
            // Not already counted as direct pending
            && i.CurrentStep.AssignedUserId != userId.Value
            && !(
                !i.CurrentStep.AssignedUserId.HasValue
                && userRoles.Any(r => r.Equals(i.CurrentStep.AssignedRole, StringComparison.OrdinalIgnoreCase)))
            // Matches a delegation
            && activeDelegations.Any(d =>
                (!d.EntityType.HasValue || d.EntityType.Value == i.EntityType)
                && (string.IsNullOrEmpty(d.AssignedRole)
                    || d.AssignedRole.Equals(i.CurrentStep.AssignedRole, StringComparison.OrdinalIgnoreCase))));

        return directPending + delegatedCount;
    }
}
