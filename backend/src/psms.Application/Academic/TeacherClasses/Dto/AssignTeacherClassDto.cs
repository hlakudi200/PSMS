using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.TeacherClasses.Dto;

/// <summary>
/// Input DTO for assigning a teacher to a class for a specific subject.
/// </summary>
public class AssignTeacherClassDto
{
    [Required]
    public Guid TeacherId { get; set; }

    [Required]
    public Guid ClassId { get; set; }

    [Required]
    public Guid SubjectId { get; set; }

    public bool IsClassTeacher { get; set; }
}
