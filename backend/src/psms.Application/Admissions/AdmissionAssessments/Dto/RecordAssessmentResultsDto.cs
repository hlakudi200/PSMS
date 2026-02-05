using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.AdmissionAssessments.Dto;

/// <summary>
/// DTO for recording assessment results.
/// Aligned with AdmissionAssessment entity.
/// Validates business rule ADM-016.
/// </summary>
public class RecordAssessmentResultsDto
{
    /// <summary>
    /// Total score achieved.
    /// </summary>
    [Required]
    [Range(0, 1000)]
    public decimal TotalScore { get; set; }

    /// <summary>
    /// Maximum score for the assessment.
    /// </summary>
    [Required]
    [Range(1, 1000)]
    public decimal MaxScore { get; set; }

    /// <summary>
    /// Pass percentage threshold.
    /// </summary>
    [Required]
    [Range(0, 100)]
    public decimal PassPercentage { get; set; } = 50;

    /// <summary>
    /// Feedback on the assessment (required, max 2000 chars per ADM-016).
    /// </summary>
    [StringLength(2000)]
    public string Feedback { get; set; }
}
