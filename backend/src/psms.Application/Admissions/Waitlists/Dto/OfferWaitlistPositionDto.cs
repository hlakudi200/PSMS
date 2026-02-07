using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.Waitlists.Dto;

/// <summary>
/// DTO for offering a waitlist position.
/// Implements ADM-022.
/// </summary>
public class OfferWaitlistPositionDto
{
    /// <summary>
    /// Number of days until the offer expires.
    /// Default is 7 days per ADM-022 (shorter than regular acceptance).
    /// </summary>
    [Range(3, 14)]
    public int OfferExpiryDays { get; set; } = 7;

    /// <summary>
    /// Custom message to include in the offer notification.
    /// </summary>
    [StringLength(500)]
    public string CustomMessage { get; set; }
}
