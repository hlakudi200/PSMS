using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.AdmissionSettings.Dto;

/// <summary>
/// DTO for creating admission settings.
/// Aligned with AdmissionSettings entity.
/// </summary>
public class CreateAdmissionSettingsDto
{
    [Required]
    public Guid AcademicYearId { get; set; }

    /// <summary>
    /// Grade ID. If null, creates default settings for all grades.
    /// </summary>
    public Guid? GradeId { get; set; }

    // Fees
    [Required]
    [Range(0, 100000)]
    public decimal ApplicationFeeAmount { get; set; }

    // Capacity
    [Range(1, 1000)]
    public int? MaxCapacity { get; set; }

    // Application Period
    public DateTime? ApplicationOpenDate { get; set; }
    public DateTime? ApplicationCloseDate { get; set; }
    public bool IsAcceptingApplications { get; set; }

    // Interview Settings
    public bool IsInterviewRequired { get; set; }

    // Assessment Settings
    public bool IsAssessmentRequired { get; set; }

    // Offer Settings
    [Range(7, 30)]
    public int OfferExpiryDays { get; set; } = 14;

    // Age Requirements
    [Range(4, 19)]
    public int? MinimumAge { get; set; }

    [Range(4, 19)]
    public int? MaximumAge { get; set; }

    // Required Documents (JSON array of DocumentCategory values)
    [StringLength(1000)]
    public string RequiredDocuments { get; set; }

    // Notes
    [StringLength(2000)]
    public string Notes { get; set; }
}
