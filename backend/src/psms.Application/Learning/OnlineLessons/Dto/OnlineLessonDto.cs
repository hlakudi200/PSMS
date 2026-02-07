using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// Full DTO for an online lesson.
/// </summary>
public class OnlineLessonDto : FullAuditedEntityDto<Guid>
{
    public Guid ClassSubjectId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public OnlinePlatform Platform { get; set; }
    public string MeetingLink { get; set; }
    public string MeetingId { get; set; }
    public string MeetingPassword { get; set; }
    public DateTime ScheduledStartTime { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public OnlineLessonStatus Status { get; set; }
    public long HostTeacherUserId { get; set; }
    public int? AttendeeCount { get; set; }
    public string RecordingUrl { get; set; }
    public bool HasRecording { get; set; }
    public bool IsRecurring { get; set; }
    public string RecurrencePattern { get; set; }

    // Flattened from ClassSubject
    public string ClassName { get; set; }
    public string SubjectName { get; set; }
}
