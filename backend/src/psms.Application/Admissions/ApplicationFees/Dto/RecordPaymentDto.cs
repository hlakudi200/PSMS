using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.ApplicationFees.Dto;

/// <summary>
/// DTO for recording a manual payment (EFT, Cash, etc.).
/// Aligned with ApplicationFee entity.
/// </summary>
public class RecordPaymentDto
{
    [Required]
    public SouthAfricanPaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Reference number from the payment (e.g., EFT reference).
    /// </summary>
    [Required]
    [StringLength(100)]
    public string PaymentReference { get; set; }

    /// <summary>
    /// Receipt number to issue.
    /// </summary>
    [StringLength(50)]
    public string ReceiptNumber { get; set; }
}
