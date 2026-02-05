using psms.Domain.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Grades.Dto;

/// <summary>
/// Input DTO for creating a new grade.
/// </summary>
public class CreateGradeDto
{
    [Required]
    public SouthAfricanGradeLevel GradeLevel { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string GradeName { get; set; }

    [Required]
    public SouthAfricanSchoolPhase SchoolPhase { get; set; }

    [StringLength(500)]
    public string Description { get; set; }
}
