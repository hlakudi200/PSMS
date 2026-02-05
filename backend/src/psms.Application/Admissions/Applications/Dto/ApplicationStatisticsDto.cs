using System;
using System.Collections.Generic;

namespace psms.Admissions.Applications.Dto;

/// <summary>
/// DTO for admission statistics and dashboard data.
/// </summary>
public class ApplicationStatisticsDto
{
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }

    // Overall Counts
    public int TotalApplications { get; set; }
    public int DraftApplications { get; set; }
    public int SubmittedApplications { get; set; }
    public int UnderReviewApplications { get; set; }
    public int UnderConsiderationApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int WaitlistedApplications { get; set; }
    public int EnrolledApplications { get; set; }
    public int WithdrawnApplications { get; set; }
    public int ExpiredApplications { get; set; }

    // Fee Statistics
    public int PendingPayments { get; set; }
    public int CompletedPayments { get; set; }
    public decimal TotalFeesCollected { get; set; }

    // Conversion Rates
    public decimal SubmissionRate => TotalApplications > 0
        ? (decimal)(TotalApplications - DraftApplications) / TotalApplications * 100
        : 0;

    public decimal ApprovalRate => SubmittedApplications > 0
        ? (decimal)ApprovedApplications / SubmittedApplications * 100
        : 0;

    public decimal EnrollmentRate => ApprovedApplications > 0
        ? (decimal)EnrolledApplications / ApprovedApplications * 100
        : 0;

    // Per Grade Statistics
    public List<GradeStatisticsDto> ByGrade { get; set; } = new();

    // Trend Data (last 30 days)
    public List<DailyApplicationCountDto> DailyTrend { get; set; } = new();
}

/// <summary>
/// Statistics per grade.
/// </summary>
public class GradeStatisticsDto
{
    public Guid GradeId { get; set; }
    public string GradeName { get; set; }
    public int Capacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public int PendingApplications { get; set; }
    public int ApprovedNotEnrolled { get; set; }
    public int WaitlistCount { get; set; }
    public int AvailableSpots => Capacity - CurrentEnrollment - ApprovedNotEnrolled;
    public decimal CapacityUtilization => Capacity > 0
        ? (decimal)CurrentEnrollment / Capacity * 100
        : 0;
}

/// <summary>
/// Daily application count for trend charts.
/// </summary>
public class DailyApplicationCountDto
{
    public DateTime Date { get; set; }
    public int Submitted { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Enrolled { get; set; }
}
