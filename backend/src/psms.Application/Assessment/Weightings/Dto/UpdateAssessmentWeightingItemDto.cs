using psms.Domain.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Weightings.Dto;

/// <summary>One band's new split.</summary>
public class UpdateAssessmentWeightingItemDto
{
    [Required]
    public AssessmentWeightingBand Band { get; set; }

    [Range(0, 100)]
    public int SbaPercentage { get; set; }

    [Range(0, 100)]
    public int ExamPercentage { get; set; }
}
