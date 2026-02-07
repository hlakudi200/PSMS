using System;
using System.ComponentModel.DataAnnotations;

namespace psms.SASpecific.StudentAfterCares.Dto;

/// <summary>
/// Input DTO for creating a student after-care enrollment.
/// </summary>
public class CreateStudentAfterCareDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid AfterCareId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public string DaysEnrolled { get; set; }

    [StringLength(500)]
    public string DietaryRequirements { get; set; }

    [StringLength(1000)]
    public string MedicalNotes { get; set; }

    [StringLength(2000)]
    public string AuthorizedPickupPersons { get; set; }

    public TimeSpan? UsualPickupTime { get; set; }

    [StringLength(1000)]
    public string Notes { get; set; }
}
