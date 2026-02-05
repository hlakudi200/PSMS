using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.AcademicYears.Dto;

/// <summary>
/// Input DTO for updating an academic year. All fields nullable (partial update).
/// </summary>
public class UpdateAcademicYearDto
{
    [StringLength(50, MinimumLength = 2)]
    public string YearName { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
