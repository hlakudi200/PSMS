using System.ComponentModel.DataAnnotations;

namespace psms.Financial.PaymentAllocations.Dto;

/// <summary>
/// Input DTO for updating a payment allocation amount.
/// </summary>
public class UpdatePaymentAllocationDto
{
    [Required]
    [Range(0.01, 9999999.99)]
    public decimal Amount { get; set; }
}
