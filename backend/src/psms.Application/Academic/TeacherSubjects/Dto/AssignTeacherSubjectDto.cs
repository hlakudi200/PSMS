using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.TeacherSubjects.Dto;

/// <summary>
/// Input DTO for assigning a teacher to a subject for a specific grade.
/// </summary>
public class AssignTeacherSubjectDto
{
    [Required]
    public Guid TeacherId { get; set; }

    [Required]
    public Guid SubjectId { get; set; }

    [Required]
    public Guid GradeId { get; set; }

    public bool IsPrimary { get; set; }
}
