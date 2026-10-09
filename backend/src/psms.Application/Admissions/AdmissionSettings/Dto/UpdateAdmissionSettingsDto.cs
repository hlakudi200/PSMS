using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.AdmissionSettings.Dto;

/// <summary>
/// DTO for updating admission settings.
/// Aligned with AdmissionSettings entity.
/// </summary>
public class UpdateAdmissionSettingsDto
{
    // Fees
    [Range(0, 100000)]
    public decimal? ApplicationFeeAmount { get; set; }

    /// <summary>
    /// Whether a fee has to be paid before the school will look at an
    /// application. The amount is kept when this is turned off, so a school
    /// does not have to retype what it costs when they turn it back on.
    /// </summary>
    public bool? IsApplicationFeeRequired { get; set; }

    // Capacity
    [Range(1, 1000)]
    public int? MaxCapacity { get; set; }

    // Application Period
    public DateTime? ApplicationOpenDate { get; set; }
    public DateTime? ApplicationCloseDate { get; set; }
    public bool? IsAcceptingApplications { get; set; }

    // Interview Settings
    public bool? IsInterviewRequired { get; set; }

    // Assessment Settings
    public bool? IsAssessmentRequired { get; set; }

    // Offer Settings
    [Range(7, 30)]
    public int? OfferExpiryDays { get; set; }

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
