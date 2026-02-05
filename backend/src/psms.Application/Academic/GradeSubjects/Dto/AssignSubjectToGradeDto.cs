using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.GradeSubjects.Dto;

/// <summary>
/// Input DTO for assigning a subject to a grade.
/// </summary>
public class AssignSubjectToGradeDto
{
    [Required]
    public Guid GradeId { get; set; }

    [Required]
    public Guid SubjectId { get; set; }

    /// <summary>Whether this subject is required for the grade</summary>
    public bool IsRequired { get; set; }
}
