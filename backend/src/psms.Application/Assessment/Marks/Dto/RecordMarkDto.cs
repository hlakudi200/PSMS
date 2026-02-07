using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Marks.Dto;

/// <summary>
/// Input DTO for recording a single mark.
/// </summary>
public class RecordMarkDto
{
    [Required]
    public Guid AssessmentId { get; set; }

    [Required]
    public Guid StudentId { get; set; }

    public decimal? RawMark { get; set; }
    public bool WasAbsent { get; set; }
    public bool IsReassessment { get; set; }

    [StringLength(1000)]
    public string TeacherComment { get; set; }

    [StringLength(2000)]
    public string Feedback { get; set; }
}
