using psms.Domain.Shared.Enums;
using System;

namespace psms.Admissions.ApplicationFees.Dto;

/// <summary>
/// What an applicant needs in order to deal with the application fee: how much,
/// what state it is in, and whether there is anything online to click.
/// </summary>
public class ApplicationFeeCheckoutDto
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }

    /// <summary>
    /// Whether this school charges for this grade and year at all. False means
    /// there is nothing to pay and nothing to show — the application went
    /// straight to review when it was submitted.
    /// </summary>
    public bool FeeRequired { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "ZAR";

    public PaymentStatus Status { get; set; }
    public string PaymentReference { get; set; }
    public string ReceiptNumber { get; set; }
    public DateTime? PaymentDate { get; set; }

    /// <summary>The application is sitting in PaymentPending, waiting for this.</summary>
    public bool AwaitingPayment { get; set; }

    public PaymentGatewayMode GatewayMode { get; set; }

    /// <summary>
    /// True while this deployment is using the stand-in gateway. The screen
    /// must say so, unmistakably: a simulated payment a parent could take for a
    /// real one is worse than no payment step at all.
    /// </summary>
    public bool IsSimulated { get; set; }
}
