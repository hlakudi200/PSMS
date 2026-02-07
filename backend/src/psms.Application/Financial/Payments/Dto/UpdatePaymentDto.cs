using psms.Domain.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace psms.Financial.Payments.Dto;

/// <summary>
/// Input DTO for updating a payment. All fields nullable (null-skip).
/// Only pending payments can be updated.
/// </summary>
public class UpdatePaymentDto
{
    [StringLength(100)]
    public string PaymentReference { get; set; }

    [StringLength(2000)]
    public string Notes { get; set; }

    public SouthAfricanPaymentMethod? PaymentMethod { get; set; }
}
