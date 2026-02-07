using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Assessment.Marks.Dto;

/// <summary>
/// Full DTO for a student mark.
/// </summary>
public class MarkDto : FullAuditedEntityDto<Guid>
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
    public DateTime? MarkedDate { get; set; }
    public long? MarkedByTeacherUserId { get; set; }
    public bool IsModerated { get; set; }
    public decimal? ModerationAdjustment { get; set; }

    // Flattened from Assessment
    public string AssessmentName { get; set; }
    public decimal AssessmentMaxMarks { get; set; }

    // Flattened from Student
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
}
