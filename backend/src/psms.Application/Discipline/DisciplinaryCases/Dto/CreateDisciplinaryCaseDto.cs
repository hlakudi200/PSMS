using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Discipline.DisciplinaryCases.Dto;

/// <summary>
/// Input DTO for creating a disciplinary case.
/// </summary>
public class CreateDisciplinaryCaseDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public DateTime IncidentDate { get; set; }

    [Required]
    [StringLength(2000)]
    public string IncidentDescription { get; set; }

    [Required]
    public int IncidentCategory { get; set; }

    [Required]
    public int Severity { get; set; }

    [StringLength(200)]
    public string Location { get; set; }

    [StringLength(500)]
    public string WitnessNames { get; set; }
}
