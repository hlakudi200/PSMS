using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Financial.Payments.Dto;

/// <summary>
/// Full DTO for a payment.
/// </summary>
public class PaymentDto : FullAuditedEntityDto<Guid>
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
    public string Notes { get; set; }
    public string ConcurrencyStamp { get; set; }

    // Computed
    public string FormattedAmount { get; set; }
    public decimal TotalAllocated { get; set; }
    public decimal UnallocatedAmount { get; set; }
    public int AllocationCount { get; set; }

    // Flattened from Student
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }

    // Flattened from Parent
    public string ParentName { get; set; }
}
