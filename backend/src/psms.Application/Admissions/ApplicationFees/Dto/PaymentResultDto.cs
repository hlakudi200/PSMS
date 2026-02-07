using psms.Domain.Shared.Enums;
using System;

namespace psms.Admissions.ApplicationFees.Dto;

/// <summary>
/// DTO for payment result (both online and manual).
/// </summary>
public class PaymentResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; }

    // Payment Details
    public Guid ApplicationId { get; set; }
    public Guid? FeeId { get; set; }
    public string PaymentReference { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZAR";

    // Receipt
    public string ReceiptNumber { get; set; }

    // Error Details
    public string ErrorCode { get; set; }
    public string ErrorDetails { get; set; }
}
