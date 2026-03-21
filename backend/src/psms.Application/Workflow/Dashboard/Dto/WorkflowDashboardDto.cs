using psms.Domain.Workflow.Enums;
using System;
using System.Collections.Generic;

namespace psms.Workflow.Dashboard.Dto;

public class WorkflowDashboardDto
{
    /// <summary>
    /// Total active workflows (NotStarted + InProgress)
    /// </summary>
    public int TotalActive { get; set; }

    /// <summary>
    /// Total completed workflows
    /// </summary>
    public int TotalCompleted { get; set; }

    /// <summary>
    /// Total rejected workflows
    /// </summary>
    public int TotalRejected { get; set; }

    /// <summary>
    /// Total overdue workflows (past SLA)
    /// </summary>
    public int TotalOverdue { get; set; }

    /// <summary>
    /// Average completion time in hours (for completed workflows in last 90 days)
    /// </summary>
    public double? AverageCompletionHours { get; set; }

    /// <summary>
    /// Breakdown by entity type
    /// </summary>
    public List<WorkflowEntityTypeSummaryDto> ByEntityType { get; set; } = new();

    /// <summary>
    /// Breakdown by current step role (who has pending work)
    /// </summary>
    public List<WorkflowRoleSummaryDto> ByRole { get; set; } = new();

    /// <summary>
    /// Workflows pending action by the current user
    /// </summary>
    public int MyPendingCount { get; set; }
}

public class WorkflowEntityTypeSummaryDto
{
    public WorkflowEntityType EntityType { get; set; }
    public int ActiveCount { get; set; }
    public int CompletedCount { get; set; }
    public int RejectedCount { get; set; }
    public int OverdueCount { get; set; }
}

public class WorkflowRoleSummaryDto
{
    public string RoleName { get; set; }
    public int PendingCount { get; set; }
    public int OverdueCount { get; set; }
}
