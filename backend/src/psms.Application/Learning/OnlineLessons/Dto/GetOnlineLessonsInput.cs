using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// Input DTO for querying online lessons with optional filters.
/// </summary>
public class GetOnlineLessonsInput : PagedAndSortedResultRequestDto
{
    public Guid? ClassSubjectId { get; set; }
    public OnlineLessonStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public long? HostTeacherUserId { get; set; }
}
