using Abp.Dependency;
using Castle.Core.Logging;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.Communication.Dispatch;
using psms.Domain.Shared.Enums;
using psms.Domain.Workflow.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Shared;

/// <summary>
/// WF-37: the workflow module's notifications — step assigned, decision made,
/// SLA breached — raised through the channel-agnostic dispatcher (in-app today;
/// channels are opt-in per request and can be widened later).
///
/// Notification failures are logged and never fail the approval itself.
/// </summary>
public class WorkflowNotifier : ITransientDependency
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly UserManager _userManager;
    private readonly WorkflowEntitySummaryProvider _summaries;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public WorkflowNotifier(
        INotificationDispatcher dispatcher,
        UserManager userManager,
        WorkflowEntitySummaryProvider summaries)
    {
        _dispatcher = dispatcher;
        _userManager = userManager;
        _summaries = summaries;
    }

    /// <summary>A step became current: tell the pinned user, or everyone holding the step's role.</summary>
    public async Task StepAssignedAsync(WorkflowInstance instance, WorkflowStep step, string definitionName, string idempotencyKey)
    {
        try
        {
            var recipients = await ResolveStepRecipientsAsync(step);
            if (recipients.Count == 0) return;

            var subject = await SubjectAsync(instance);
            var dueText = instance.CurrentStepDueDate.HasValue
                ? $" Due {instance.CurrentStepDueDate.Value:dd MMM yyyy HH:mm} UTC."
                : string.Empty;

            await _dispatcher.DispatchAsync(new NotificationRequest
            {
                TenantId = instance.TenantId,
                RecipientUserIds = recipients,
                Type = NotificationType.System,
                Priority = NotificationPriority.Normal,
                Title = $"Approval needed: {definitionName}",
                Message = $"{subject} is waiting at \"{step.Name}\".{dueText}",
                ActionUrl = ActionUrlFor(step.AssignedRole, instance.Id),
                EntityType = "WorkflowInstance",
                EntityId = instance.Id,
                IdempotencyKey = idempotencyKey,
            });
        }
        catch (Exception ex)
        {
            Logger.Warn($"Workflow step-assigned notification failed for instance {instance.Id}: {ex.Message}", ex);
        }
    }

    /// <summary>The workflow ended (approved, rejected, cancelled, recalled): tell whoever started it.</summary>
    public async Task OutcomeAsync(WorkflowInstance instance, string definitionName, string outcome, string comment, long? actorUserId)
    {
        try
        {
            var initiator = instance.CreatorUserId;
            if (!initiator.HasValue || initiator == actorUserId) return;

            var subject = await SubjectAsync(instance);
            var detail = string.IsNullOrWhiteSpace(comment) ? string.Empty : $" Comment: {comment.Trim()}";

            await _dispatcher.DispatchAsync(new NotificationRequest
            {
                TenantId = instance.TenantId,
                RecipientUserIds = new List<long> { initiator.Value },
                Type = NotificationType.System,
                Priority = outcome == "rejected" ? NotificationPriority.High : NotificationPriority.Normal,
                Title = $"{definitionName} {outcome}",
                Message = $"{subject} was {outcome}.{detail}",
                ActionUrl = null,
                EntityType = "WorkflowInstance",
                EntityId = instance.Id,
                IdempotencyKey = $"wf-outcome-{instance.Id}-{outcome}",
            });
        }
        catch (Exception ex)
        {
            Logger.Warn($"Workflow outcome notification failed for instance {instance.Id}: {ex.Message}", ex);
        }
    }

    /// <summary>The current step is past its SLA: nudge its assignees once per step.</summary>
    public async Task SlaBreachedAsync(WorkflowInstance instance, WorkflowStep step, string definitionName)
    {
        try
        {
            var recipients = await ResolveStepRecipientsAsync(step);
            if (recipients.Count == 0) return;

            var subject = await SubjectAsync(instance);
            await _dispatcher.DispatchAsync(new NotificationRequest
            {
                TenantId = instance.TenantId,
                RecipientUserIds = recipients,
                Type = NotificationType.System,
                Priority = NotificationPriority.High,
                Title = $"Overdue approval: {definitionName}",
                Message = $"{subject} has been waiting at \"{step.Name}\" since {instance.CurrentStepDueDate:dd MMM yyyy HH:mm} UTC.",
                ActionUrl = ActionUrlFor(step.AssignedRole, instance.Id),
                EntityType = "WorkflowInstance",
                EntityId = instance.Id,
                IdempotencyKey = $"wf-sla-{instance.Id}-{step.Id}",
            });
        }
        catch (Exception ex)
        {
            Logger.Warn($"Workflow SLA notification failed for instance {instance.Id}: {ex.Message}", ex);
        }
    }

    private async Task<List<long>> ResolveStepRecipientsAsync(WorkflowStep step)
    {
        if (step.AssignedUserId.HasValue) return new List<long> { step.AssignedUserId.Value };
        if (string.IsNullOrWhiteSpace(step.AssignedRole)) return new List<long>();

        var users = await _userManager.GetUsersInRoleAsync(step.AssignedRole.ToUpperInvariant());
        return users.Where(u => u.IsActive).Select(u => u.Id).ToList();
    }

    private async Task<string> SubjectAsync(WorkflowInstance instance)
    {
        try
        {
            var labels = await _summaries.GetSubjectLabelsAsync(
                new List<KeyValuePair<Domain.Workflow.Enums.WorkflowEntityType, Guid>>
                {
                    new(instance.EntityType, instance.EntityId),
                });
            return labels.TryGetValue(instance.EntityId, out var label) && !string.IsNullOrWhiteSpace(label)
                ? label
                : instance.EntityType.ToString();
        }
        catch
        {
            return instance.EntityType.ToString();
        }
    }

    /// <summary>
    /// Deep link into the portal that mounts the workflow module for the role.
    /// Roles whose portal has no workflow route yet (HOD, Finance, Admissions
    /// Officer — see WF-40) get no link; the bell still shows the notification.
    /// </summary>
    private static string ActionUrlFor(string role, Guid instanceId)
    {
        if (string.IsNullOrWhiteSpace(role)) return null;
        if (role.Equals(StaticRoleNames.Tenants.Admin, StringComparison.OrdinalIgnoreCase))
            return $"/admin/workflow/instances/{instanceId}";
        if (role.Equals(StaticRoleNames.Tenants.Principal, StringComparison.OrdinalIgnoreCase)
            || role.Equals(StaticRoleNames.Tenants.VicePrincipal, StringComparison.OrdinalIgnoreCase))
            return $"/principal/workflow/instances/{instanceId}";
        if (role.Equals(StaticRoleNames.Tenants.Teacher, StringComparison.OrdinalIgnoreCase))
            return $"/teacher/workflow/instances/{instanceId}";
        return null;
    }
}
