using System;

namespace psms.Admissions.AdmissionSettings.Dto;

/// <summary>
/// DTO for capacity status information (ADM-028).
/// </summary>
public class CapacityStatusDto
{
    public Guid GradeId { get; set; }
    public string GradeName { get; set; }
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }

    /// <summary>
    /// Total capacity for the grade.
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Currently enrolled students.
    /// </summary>
    public int CurrentEnrollment { get; set; }

    /// <summary>
    /// Approved applications pending enrollment.
    /// </summary>
    public int ApprovedPendingEnrollment { get; set; }

    /// <summary>
    /// Applications currently under consideration.
    /// </summary>
    public int UnderConsideration { get; set; }

    /// <summary>
    /// Applications on the waitlist.
    /// </summary>
    public int WaitlistCount { get; set; }

    /// <summary>
    /// Available spots (Capacity - CurrentEnrollment - ApprovedPendingEnrollment).
    /// </summary>
    public int AvailableSpots => Capacity - CurrentEnrollment - ApprovedPendingEnrollment;

    /// <summary>
    /// Capacity utilization percentage.
    /// </summary>
    public decimal UtilizationPercentage => Capacity > 0
        ? (decimal)CurrentEnrollment / Capacity * 100
        : 0;

    /// <summary>
    /// Projected utilization if all approved applications enroll.
    /// </summary>
    public decimal ProjectedUtilization => Capacity > 0
        ? (decimal)(CurrentEnrollment + ApprovedPendingEnrollment) / Capacity * 100
        : 0;

    /// <summary>
    /// Indicates if the grade is at full capacity.
    /// </summary>
    public bool IsCapacityFull => AvailableSpots <= 0;

    /// <summary>
    /// Indicates if applications should be automatically waitlisted.
    /// </summary>
    public bool ShouldAutoWaitlist => IsCapacityFull;
}
