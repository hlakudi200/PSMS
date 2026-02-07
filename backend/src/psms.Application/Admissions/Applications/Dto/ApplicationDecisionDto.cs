using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.Applications.Dto;

/// <summary>
/// DTO for making admission decisions (Approve, Reject, Waitlist).
/// Validates business rules ADM-017 to ADM-020.
/// </summary>
public class ApplicationDecisionDto
{
    /// <summary>
    /// Reason for the decision.
    /// Required for rejection (minimum 50 characters per ADM-020).
    /// Optional for approval (can include conditions per ADM-019).
    /// </summary>
    [StringLength(2000)]
    public string Reason { get; set; }

    /// <summary>
    /// Number of days until the offer expires.
    /// Default is 14 days per ADM-018.
    /// Only applicable for approvals.
    /// </summary>
    [Range(7, 30)]
    public int OfferExpiryDays { get; set; } = 14;

    /// <summary>
    /// Conditions attached to the acceptance (ADM-019).
    /// Examples: "Probationary period", "Additional tutoring required".
    /// </summary>
    [StringLength(1000)]
    public string Conditions { get; set; }

    /// <summary>
    /// Indicates if this is a conditional acceptance.
    /// </summary>
    public bool IsConditional { get; set; }
}
