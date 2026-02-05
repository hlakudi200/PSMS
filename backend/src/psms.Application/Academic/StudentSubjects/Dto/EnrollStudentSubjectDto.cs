using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.StudentSubjects.Dto;

/// <summary>
/// Input DTO for enrolling a student in a subject.
/// </summary>
public class EnrollStudentSubjectDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid SubjectId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }
}
