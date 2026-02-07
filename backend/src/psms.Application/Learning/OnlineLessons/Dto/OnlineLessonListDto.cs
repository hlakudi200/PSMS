using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// Lightweight DTO for online lesson lists.
/// </summary>
public class OnlineLessonListDto : EntityDto<Guid>
{
    public Guid ClassSubjectId { get; set; }
    public string Title { get; set; }
    public OnlinePlatform Platform { get; set; }
    public DateTime ScheduledStartTime { get; set; }
    public DateTime ScheduledEndTime { get; set; }
    public int DurationMinutes { get; set; }
    public OnlineLessonStatus Status { get; set; }
    public bool HasRecording { get; set; }

    // Flattened from ClassSubject
    public string ClassName { get; set; }
    public string SubjectName { get; set; }
}
