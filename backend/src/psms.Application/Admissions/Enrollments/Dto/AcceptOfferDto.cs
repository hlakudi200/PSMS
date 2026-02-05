using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.Enrollments.Dto;

/// <summary>
/// DTO for accepting an admission offer.
/// Implements ADM-025.
/// </summary>
public class AcceptOfferDto
{
    /// <summary>
    /// Application ID.
    /// </summary>
    [Required]
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Confirmation that parent accepts the offer.
    /// </summary>
    [Required]
    public bool ConfirmAcceptance { get; set; }

    /// <summary>
    /// Parent's signature (could be digital signature or acknowledgment).
    /// </summary>
    public string ParentSignature { get; set; }
}
