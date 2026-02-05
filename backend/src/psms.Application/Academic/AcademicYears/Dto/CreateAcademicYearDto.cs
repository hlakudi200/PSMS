using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.AcademicYears.Dto;

/// <summary>
/// Input DTO for creating a new academic year.
/// </summary>
public class CreateAcademicYearDto
{
    [Required]
    [Range(2020, 2100)]
    public int Year { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// If true, automatically creates the 4 SA terms with default dates.
    /// </summary>
    public bool CreateDefaultTerms { get; set; } = true;
}
