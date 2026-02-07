using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Financial.PaymentAllocations.Dto;

/// <summary>
/// Input DTO for allocating a payment to multiple student fees.
/// </summary>
public class BulkAllocatePaymentDto
{
    [Required]
    public Guid PaymentId { get; set; }

    [Required]
    [MinLength(1)]
    public List<AllocationEntryDto> Allocations { get; set; }
}

/// <summary>
/// Individual allocation entry within a bulk allocation.
/// </summary>
public class AllocationEntryDto
{
    [Required]
    public Guid StudentFeeId { get; set; }

    [Required]
    [Range(0.01, 9999999.99)]
    public decimal Amount { get; set; }
}
