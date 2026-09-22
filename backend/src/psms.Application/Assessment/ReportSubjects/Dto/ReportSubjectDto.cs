using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Assessment.ReportSubjects.Dto;

/// <summary>
/// DTO for a subject-level entry in a student report.
/// </summary>
public class ReportSubjectDto : EntityDto<Guid>
{
    public Guid ReportId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid? TeacherId { get; set; }
    public decimal? TermMark { get; set; }
    public decimal? ExamMark { get; set; }
    public decimal? FinalMark { get; set; }
    public CapsAchievementLevel? AchievementLevel { get; set; }
    public string TeacherComment { get; set; }
    public int? SubjectPosition { get; set; }
    public decimal? ClassAverage { get; set; }
    public decimal? HighestInClass { get; set; }
    public decimal? LowestInClass { get; set; }

    /// <summary>
    /// RC-15. The mark is the school-based component only — the examination for
    /// this subject is the external National Senior Certificate paper.
    /// </summary>
    public bool AwaitsExternalExamination { get; set; }
    public decimal TermWeight { get; set; }
    public decimal ExamWeight { get; set; }

    // Flattened
    public string SubjectName { get; set; }
    public string SubjectCode { get; set; }
    public string TeacherName { get; set; }
}
