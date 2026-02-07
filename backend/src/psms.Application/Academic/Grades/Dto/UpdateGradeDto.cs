using psms.Domain.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Grades.Dto;

/// <summary>
/// Input DTO for updating an existing grade. All fields nullable (partial update).
/// </summary>
public class UpdateGradeDto
{
    [StringLength(100, MinimumLength = 2)]
    public string GradeName { get; set; }

    public SouthAfricanSchoolPhase? SchoolPhase { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    public bool? IsActive { get; set; }
}
