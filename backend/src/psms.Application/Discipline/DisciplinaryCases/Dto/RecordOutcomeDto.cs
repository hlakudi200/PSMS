using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Discipline.DisciplinaryCases.Dto;

/// <summary>
/// Input DTO for recording the outcome of a disciplinary case.
/// </summary>
public class RecordOutcomeDto
{
    [Required]
    public int Outcome { get; set; }

    [Required]
    [StringLength(1000)]
    public string OutcomeDescription { get; set; }

    public DateTime? SanctionStartDate { get; set; }

    public DateTime? SanctionEndDate { get; set; }
}
