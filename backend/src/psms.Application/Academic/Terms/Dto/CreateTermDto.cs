using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Terms.Dto;

/// <summary>
/// Input DTO for creating a new term.
/// </summary>
public class CreateTermDto
{
    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public SouthAfricanTermNumber TermNumber { get; set; }

    [StringLength(50, MinimumLength = 2)]
    public string TermName { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}
