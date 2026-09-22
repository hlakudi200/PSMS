using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
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

    /// <summary>
    /// External meeting URL. Required for external platforms (Zoom/Teams/etc.)
    /// and validated server-side; omitted for <see cref="OnlinePlatform.InApp"/>
    /// live classes, where the classroom is hosted inside PSMS and the join
    /// route is derived from the lesson id (see CreateAsync / LC-01).
    /// </summary>
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

    /// <summary>
    /// Published learning materials on the same class-subject to attach as
    /// pre-lesson reading (US-TCH-004). Null or empty attaches nothing.
    /// </summary>
    public List<Guid> MaterialIds { get; set; }
}
