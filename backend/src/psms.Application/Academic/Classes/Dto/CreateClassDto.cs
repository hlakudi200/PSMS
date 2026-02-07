using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Classes.Dto;

/// <summary>
/// Input DTO for creating a class.
/// </summary>
public class CreateClassDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string ClassName { get; set; }

    [Required]
    public Guid GradeId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Range(1, 500)]
    public int MaxCapacity { get; set; }

    /// <summary>Optional class teacher (form teacher).</summary>
    public Guid? ClassTeacherId { get; set; }
}
