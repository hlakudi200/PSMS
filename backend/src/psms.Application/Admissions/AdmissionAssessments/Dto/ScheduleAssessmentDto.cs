using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.AdmissionAssessments.Dto;

/// <summary>
/// DTO for scheduling an admission assessment.
/// Aligned with AdmissionAssessment entity.
/// Validates business rule ADM-015.
/// </summary>
public class ScheduleAssessmentDto
{
    [Required]
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Type of assessment.
    /// </summary>
    [Required]
    public AssessmentType Type { get; set; }

    /// <summary>
    /// Date of the assessment.
    /// Must be at least 7 days in advance (ADM-015).
    /// </summary>
    [Required]
    public DateTime ScheduledDate { get; set; }

    /// <summary>
    /// Grade to assess for.
    /// </summary>
    [Required]
    public Guid AssessedGradeId { get; set; }

    /// <summary>
    /// Subjects to be assessed (JSON array).
    /// </summary>
    [StringLength(500)]
    public string Subjects { get; set; }

    /// <summary>
    /// User ID of the assessor.
    /// Must have Admissions.Assessments.Conduct permission.
    /// </summary>
    [Required]
    public long AssessorUserId { get; set; }

    /// <summary>
    /// Maximum score for the assessment.
    /// </summary>
    [Required]
    [Range(1, 1000)]
    public decimal MaxScore { get; set; } = 100;
}
