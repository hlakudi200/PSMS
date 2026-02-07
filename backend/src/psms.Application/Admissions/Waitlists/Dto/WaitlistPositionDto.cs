using System;

namespace psms.Admissions.Waitlists.Dto;

/// <summary>
/// DTO for waitlist position information (for applicants).
/// Implements ADM-021.
/// </summary>
public class WaitlistPositionDto
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }
    public Guid GradeId { get; set; }
    public string GradeName { get; set; }

    /// <summary>
    /// Current position in the waitlist (1 = first in line).
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Date added to waitlist.
    /// </summary>
    public DateTime AddedDate { get; set; }

    /// <summary>
    /// Total number of people on the waitlist.
    /// </summary>
    public int TotalInWaitlist { get; set; }
}
