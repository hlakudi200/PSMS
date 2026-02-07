using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Financial.Payments.Dto;

/// <summary>
/// Lightweight DTO for payment lists.
/// </summary>
public class PaymentListDto : EntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid ParentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public SouthAfricanPaymentMethod PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentReference { get; set; }
    public string ReceiptNumber { get; set; }
    public PaymentStatus Status { get; set; }

    // Computed
    public string FormattedAmount { get; set; }
    public decimal TotalAllocated { get; set; }
    public decimal UnallocatedAmount { get; set; }
    public int AllocationCount { get; set; }

    // Flattened
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
    public string ParentName { get; set; }
}
