using Abp.Application.Services.Dto;
using System;

namespace psms.Financial.PaymentAllocations.Dto;

/// <summary>
/// Lightweight DTO for payment allocation lists.
/// </summary>
public class PaymentAllocationListDto : EntityDto<Guid>
{
    public Guid PaymentId { get; set; }
    public Guid StudentFeeId { get; set; }
    public decimal Amount { get; set; }
    public DateTime AllocatedDate { get; set; }

    // Flattened
    public string ReceiptNumber { get; set; }
    public string FeeStructureName { get; set; }
    public string StudentName { get; set; }
}
