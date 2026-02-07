using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.ClassSubjects.Dto;

/// <summary>
/// Input DTO for creating a class-subject assignment.
/// </summary>
public class CreateClassSubjectDto
{
    [Required]
    public Guid ClassId { get; set; }

    [Required]
    public Guid SubjectId { get; set; }

    public Guid? TeacherId { get; set; }

    [Range(0, 40)]
    public int PeriodsPerWeek { get; set; }
}
