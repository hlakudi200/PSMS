using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.AdmissionInterviews.Dto;

/// <summary>
/// DTO for rescheduling an interview.
/// Aligned with AdmissionInterview entity.
/// </summary>
public class RescheduleInterviewDto
{
    /// <summary>
    /// New date for the interview.
    /// Must be at least 48 hours in advance.
    /// </summary>
    [Required]
    public DateTime NewScheduledDate { get; set; }

    /// <summary>
    /// New time.
    /// </summary>
    [Required]
    public TimeSpan NewScheduledTime { get; set; }

    /// <summary>
    /// New location (optional change).
    /// </summary>
    [StringLength(200)]
    public string Location { get; set; }

    /// <summary>
    /// New meeting link (optional change).
    /// </summary>
    [StringLength(500)]
    [Url]
    public string MeetingLink { get; set; }
}
