using System;
using System.Collections.Generic;

namespace psms.Academic.PrincipalDashboard.Dto;

/// <summary>
/// One-call summary for the Principal / Vice Principal dashboard: headline
/// counts, the "needs your attention" queue and per-grade performance, all
/// aggregated server-side so the client no longer pages through every report
/// and class to compute them.
/// </summary>
public class PrincipalDashboardSummaryDto
{
    // --- Headline counts ---
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalClasses { get; set; }

    // --- Needs attention (items awaiting a principal-level decision) ---
    public int PendingAdmissions { get; set; }
    public int OpenDisciplinaryCases { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int PendingFieldTrips { get; set; }
    public int PendingExpenses { get; set; }
    public int PendingFeeWaivers { get; set; }
    public int PendingTransfers { get; set; }

    // --- Finance ---
    /// <summary>Sum of (AmountDue - AmountPaid) over fees that are Pending, PartiallyPaid or Overdue.</summary>
    public decimal OutstandingFeesTotal { get; set; }
    /// <summary>Number of student fees currently flagged Overdue.</summary>
    public int OverdueFeesCount { get; set; }

    // --- Academic performance ---
    /// <summary>The pass mark used for <see cref="GradePerformanceDto.PassRate"/>.</summary>
    public decimal PassMarkPercentage { get; set; }
    public List<GradePerformanceDto> GradePerformance { get; set; } = new();
}

public class GradePerformanceDto
{
    public Guid GradeId { get; set; }
    public string GradeName { get; set; }
    public int GradeLevel { get; set; }
    public int StudentCount { get; set; }
    public int ReportCount { get; set; }
    /// <summary>Mean OverallPercentage across reports in the grade; null when there are no scored reports.</summary>
    public decimal? AveragePercentage { get; set; }
    /// <summary>Percentage of scored reports at or above the pass mark; null when there are no scored reports.</summary>
    public decimal? PassRate { get; set; }
}
