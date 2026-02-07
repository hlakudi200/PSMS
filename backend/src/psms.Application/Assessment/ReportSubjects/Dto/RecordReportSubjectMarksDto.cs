using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.ReportSubjects.Dto;

/// <summary>
/// Input DTO for recording marks on a report subject entry.
/// </summary>
public class RecordReportSubjectMarksDto
{
    [Required]
    public Guid ReportSubjectId { get; set; }

    [Range(0, 100)]
    public decimal? TermMark { get; set; }

    [Range(0, 100)]
    public decimal? ExamMark { get; set; }

    [Range(0, 100)]
    public decimal TermWeight { get; set; } = 40;

    [Range(0, 100)]
    public decimal ExamWeight { get; set; } = 60;

    [StringLength(1000)]
    public string TeacherComment { get; set; }
}
