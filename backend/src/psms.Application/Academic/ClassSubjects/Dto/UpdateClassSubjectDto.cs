using System;

namespace psms.Academic.ClassSubjects.Dto;

/// <summary>
/// Input DTO for updating a class-subject assignment. All fields nullable for partial updates.
/// </summary>
public class UpdateClassSubjectDto
{
    public Guid? TeacherId { get; set; }
    public int? PeriodsPerWeek { get; set; }
    public bool? IsActive { get; set; }
}
