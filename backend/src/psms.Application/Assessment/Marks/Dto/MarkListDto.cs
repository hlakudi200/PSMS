using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Assessment.Marks.Dto;

/// <summary>
/// Lightweight DTO for mark lists.
/// </summary>
public class MarkListDto : EntityDto<Guid>
{
    public Guid AssessmentId { get; set; }
    public Guid StudentId { get; set; }
    public decimal? RawMark { get; set; }
    public decimal? Percentage { get; set; }
    public CapsAchievementLevel? AchievementLevel { get; set; }
    public MarkStatus Status { get; set; }
    public bool WasAbsent { get; set; }
    public bool IsReassessment { get; set; }
    public string TeacherComment { get; set; }
    public string Feedback { get; set; }
    public bool IsModerated { get; set; }
    public decimal? ModerationAdjustment { get; set; }

    // Flattened
    public string AssessmentName { get; set; }
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
}
