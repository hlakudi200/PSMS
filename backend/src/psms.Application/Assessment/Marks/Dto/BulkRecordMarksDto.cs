using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Marks.Dto;

/// <summary>
/// Input DTO for bulk recording marks for an assessment.
/// </summary>
public class BulkRecordMarksDto
{
    [Required]
    public Guid AssessmentId { get; set; }

    [Required]
    public List<StudentMarkDto> StudentMarks { get; set; }
}

/// <summary>
/// Individual student mark entry within a bulk operation.
/// </summary>
public class StudentMarkDto
{
    [Required]
    public Guid StudentId { get; set; }

    public decimal? RawMark { get; set; }
    public bool WasAbsent { get; set; }

    [StringLength(1000)]
    public string TeacherComment { get; set; }
}
