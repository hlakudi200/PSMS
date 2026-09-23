using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// RC-17. Input for recording how a learner conducted themselves and applied
/// themselves — two of the fields RE-002 lists for a South African report card.
/// </summary>
public class RecordConductDto
{
    [Required]
    public Guid ReportId { get; set; }

    public ConductDiligenceRating? ConductRating { get; set; }

    public ConductDiligenceRating? DiligenceRating { get; set; }

    /// <summary>
    /// What the ratings are based on. A rating with no account of it tells a
    /// parent very little.
    /// </summary>
    [StringLength(psms.Domain.Assessment.Entities.Report.MaxBehaviourCommentLength)]
    public string BehaviourComments { get; set; }
}
