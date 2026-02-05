using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Admissions.Waitlists.Dto;

/// <summary>
/// DTO for waitlist entry information.
/// Aligned with Waitlist entity.
/// </summary>
public class WaitlistDto : FullAuditedEntityDto<Guid>
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }
    public string ApplicantName { get; set; }
    public Guid GradeId { get; set; }
    public string GradeName { get; set; }

    // Position (ADM-021)
    public int Position { get; set; }
    public DateTime AddedDate { get; set; }

    // Status
    public WaitlistStatus Status { get; set; }
    public string StatusDisplayName => Status.ToString();

    // Offer Information (ADM-022, ADM-023)
    public DateTime? NotifiedDate { get; set; }
    public DateTime? OfferExpiryDate { get; set; }
    public bool IsOfferExpired => OfferExpiryDate.HasValue && OfferExpiryDate.Value < DateTime.UtcNow;
    public int? DaysUntilOfferExpiry => OfferExpiryDate.HasValue && !IsOfferExpired
        ? (int)(OfferExpiryDate.Value - DateTime.UtcNow).TotalDays
        : null;

    // Notes
    public string Notes { get; set; }

    // Flags
    public bool CanAcceptOffer => Status == WaitlistStatus.Offered && !IsOfferExpired;
    public bool CanDeclineOffer => Status == WaitlistStatus.Offered && !IsOfferExpired;
    public bool CanWithdraw => Status == WaitlistStatus.Active || Status == WaitlistStatus.Offered;
}
