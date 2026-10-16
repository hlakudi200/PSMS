using System;

namespace psms.Admissions.ApplicationFees.Dto;

/// <summary>
/// Where to send a payer, and what they are about to be charged.
/// <para>
/// Nothing is recorded by the call that returns this. The fee is marked paid
/// when the gateway says so on a signed webhook — reaching the success page is
/// not proof of payment, and closing the tab after paying is not proof of its
/// absence.
/// </para>
/// </summary>
public class StartPaymentDto
{
    public Guid ApplicationId { get; set; }

    /// <summary>The gateway's hosted page. No card details touch this application.</summary>
    public string RedirectUrl { get; set; }

    public decimal Amount { get; set; }

    /// <summary>What the webhook will be matched on.</summary>
    public string PaymentReference { get; set; }
}
