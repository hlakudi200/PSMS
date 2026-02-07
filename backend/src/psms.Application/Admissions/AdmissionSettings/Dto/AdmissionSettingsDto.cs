using Abp.Application.Services.Dto;
using System;

namespace psms.Admissions.AdmissionSettings.Dto;

/// <summary>
/// DTO for admission settings information.
/// Aligned with AdmissionSettings entity.
/// </summary>
public class AdmissionSettingsDto : FullAuditedEntityDto<Guid>
{
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }
    public Guid? GradeId { get; set; }
    public string GradeName { get; set; }
    public bool IsDefaultSettings => !GradeId.HasValue;

    // Application Period
    public DateTime? ApplicationOpenDate { get; set; }
    public DateTime? ApplicationCloseDate { get; set; }
    public bool IsAcceptingApplications { get; set; }
    public bool IsApplicationPeriodOpen => IsAcceptingApplications
        && (!ApplicationOpenDate.HasValue || DateTime.UtcNow >= ApplicationOpenDate.Value)
        && (!ApplicationCloseDate.HasValue || DateTime.UtcNow <= ApplicationCloseDate.Value);

    // Capacity (ADM-028)
    public int? MaxCapacity { get; set; }
    public int CurrentEnrolledCount { get; set; }
    public int AvailableSpots => MaxCapacity.HasValue ? Math.Max(0, MaxCapacity.Value - CurrentEnrolledCount) : int.MaxValue;
    public bool IsCapacityFull => MaxCapacity.HasValue && CurrentEnrolledCount >= MaxCapacity.Value;

    // Fees (ADM-006)
    public decimal ApplicationFeeAmount { get; set; }
    public string ApplicationFeeDisplay => $"R {ApplicationFeeAmount:N2}";

    // Interview Settings (ADM-011)
    public bool IsInterviewRequired { get; set; }

    // Assessment Settings (ADM-014)
    public bool IsAssessmentRequired { get; set; }

    // Offer Settings (ADM-018)
    public int OfferExpiryDays { get; set; } = 14;

    // Age Requirements (ADM-004)
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }

    // Required Documents (JSON)
    public string RequiredDocuments { get; set; }

    // Notes
    public string Notes { get; set; }
}
