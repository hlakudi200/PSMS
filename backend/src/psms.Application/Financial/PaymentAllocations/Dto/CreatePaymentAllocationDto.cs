using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Financial.PaymentAllocations.Dto;

/// <summary>
/// Input DTO for creating a payment allocation.
/// </summary>
public class CreatePaymentAllocationDto
{
    [Required]
    public Guid PaymentId { get; set; }

    [Required]
    public Guid StudentFeeId { get; set; }

    [Required]
    [Range(0.01, 9999999.99)]
    public decimal Amount { get; set; }
}
