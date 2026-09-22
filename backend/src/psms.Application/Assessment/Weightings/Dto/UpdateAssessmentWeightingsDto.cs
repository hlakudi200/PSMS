using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Weightings.Dto;

/// <summary>
/// Saves the weighting table. Sent whole rather than a band at a time so the
/// screen's Save is one call and one unit of work.
/// </summary>
public class UpdateAssessmentWeightingsDto
{
    [Required]
    public List<UpdateAssessmentWeightingItemDto> Weightings { get; set; } = new List<UpdateAssessmentWeightingItemDto>();
}
