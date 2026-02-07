using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.StudentClasses.Dto;

public class CreateStudentClassDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid ClassId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public DateTime EnrollmentDate { get; set; }
}
