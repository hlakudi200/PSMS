using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Admissions.ApplicationFees.Dto;

/// <summary>
/// DTO for application fee information.
/// Aligned with ApplicationFee entity.
/// </summary>
public class ApplicationFeeDto : CreationAuditedEntityDto<Guid>
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }

    // Fee Information
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZAR";
    public string AmountDisplay => $"R {Amount:N2}";

    // Payment Status
    public PaymentStatus Status { get; set; }
    public string StatusDisplayName => Status.ToString();
    public bool IsPaid => Status == PaymentStatus.Completed;
    public DateTime? PaymentDate { get; set; }
    public SouthAfricanPaymentMethod? PaymentMethod { get; set; }
    public string PaymentMethodDisplayName => PaymentMethod?.ToString();
    public string PaymentReference { get; set; }

    // Receipt
    public string ReceiptNumber { get; set; }

    // Refundable
    public bool IsRefundable { get; set; }
}
