using System.ComponentModel.DataAnnotations;

namespace psms.Discipline.DisciplinaryCases.Dto;

/// <summary>
/// Input DTO for updating a disciplinary case (null-skip pattern).
/// </summary>
public class UpdateDisciplinaryCaseDto
{
    [StringLength(2000)]
    public string IncidentDescription { get; set; }

    public int? IncidentCategory { get; set; }

    public int? Severity { get; set; }

    [StringLength(200)]
    public string Location { get; set; }

    [StringLength(500)]
    public string WitnessNames { get; set; }

    [StringLength(2000)]
    public string InvestigationNotes { get; set; }

    [StringLength(2000)]
    public string HearingNotes { get; set; }
}
