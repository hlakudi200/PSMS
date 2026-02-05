using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.AdmissionInterviews.Dto;

/// <summary>
/// DTO for completing an interview and recording the outcome.
/// Aligned with AdmissionInterview entity.
/// Validates business rule ADM-013.
/// </summary>
public class CompleteInterviewDto
{
    /// <summary>
    /// Rating on a scale of 1-5.
    /// Required when completing an interview.
    /// </summary>
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    /// <summary>
    /// Recommendation for admission (Yes/No).
    /// Required when completing an interview.
    /// </summary>
    [Required]
    public bool Recommended { get; set; }

    /// <summary>
    /// Interview notes (up to 2000 characters per ADM-013).
    /// </summary>
    [StringLength(2000)]
    public string Notes { get; set; }
}
