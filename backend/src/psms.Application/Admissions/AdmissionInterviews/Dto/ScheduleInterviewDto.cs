using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.AdmissionInterviews.Dto;

/// <summary>
/// DTO for scheduling an interview.
/// Aligned with AdmissionInterview entity.
/// Validates business rule ADM-012.
/// </summary>
public class ScheduleInterviewDto
{
    [Required]
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Date of the interview.
    /// Must be at least 48 hours in advance (ADM-012).
    /// </summary>
    [Required]
    public DateTime ScheduledDate { get; set; }

    /// <summary>
    /// Time of the interview.
    /// </summary>
    [Required]
    public TimeSpan ScheduledTime { get; set; }

    /// <summary>
    /// User ID of the interviewer.
    /// Must have Admissions.Interviews.Conduct permission.
    /// </summary>
    [Required]
    public long InterviewerUserId { get; set; }

    /// <summary>
    /// Ignored. The name stored against the interview is the school's own
    /// record of the chosen user — this used to be taken from the request
    /// alongside the id, with nothing checking that the two described the same
    /// person. Kept on the contract so existing callers do not break.
    /// </summary>
    [StringLength(100)]
    public string InterviewerName { get; set; }

    /// <summary>
    /// Location for in-person interviews.
    /// </summary>
    [StringLength(200)]
    public string Location { get; set; }

    /// <summary>
    /// Meeting link for online interviews.
    /// </summary>
    [StringLength(500)]
    [Url]
    public string MeetingLink { get; set; }

    /// <summary>
    /// Additional notes for the interview.
    /// </summary>
    [StringLength(2000)]
    public string Notes { get; set; }
}
