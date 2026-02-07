using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// Input DTO for creating an online lesson.
/// </summary>
public class CreateOnlineLessonDto
{
    [Required]
    public Guid ClassSubjectId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    [Required]
    public OnlinePlatform Platform { get; set; }

    [Required]
    [StringLength(500)]
    public string MeetingLink { get; set; }

    [StringLength(100)]
    public string MeetingId { get; set; }

    [StringLength(50)]
    public string MeetingPassword { get; set; }

    [Required]
    public DateTime ScheduledStartTime { get; set; }

    [Required]
    public DateTime ScheduledEndTime { get; set; }

    public bool IsRecurring { get; set; }

    public string RecurrencePattern { get; set; }
}
