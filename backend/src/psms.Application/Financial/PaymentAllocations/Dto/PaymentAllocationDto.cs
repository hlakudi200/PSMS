using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Financial.PaymentAllocations.Dto;

/// <summary>
/// Full DTO for a payment allocation.
/// </summary>
public class PaymentAllocationDto : EntityDto<Guid>
{
    public Guid PaymentId { get; set; }
    public Guid StudentFeeId { get; set; }
    public decimal Amount { get; set; }
    public DateTime AllocatedDate { get; set; }

    // Flattened from Payment
    public string ReceiptNumber { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal PaymentAmount { get; set; }

    // Flattened from StudentFee chain
    public string FeeStructureName { get; set; }
    public decimal StudentFeeAmountDue { get; set; }
    public decimal StudentFeeOutstandingBalance { get; set; }
    public string StudentName { get; set; }
}
