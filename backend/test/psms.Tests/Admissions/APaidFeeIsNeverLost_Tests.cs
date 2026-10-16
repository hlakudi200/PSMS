using psms.Admissions.ApplicationFees.Yoco;
using Shouldly;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// A payment the gateway took is not lost because a webhook went missing.
/// <para>
/// Found on a live sandbox payment: Yoco took R200, redirected the payer back
/// to the success URL, and the fee sat Pending for as long as anyone cared to
/// watch. The webhook endpoint was up and correctly refusing unsigned requests
/// — nothing was calling it. A webhook is best-effort by nature, and the one
/// thing that must not depend on it is a parent's money being seen.
/// </para>
/// <para>
/// So when the payer comes back, the server asks the gateway directly. These
/// are about what it does with the answer — in particular, what it refuses to
/// read as paid, because marking an unpaid application paid is the one mistake
/// nothing downstream can undo.
/// </para>
/// </summary>
public class APaidFeeIsNeverLost_Tests
{
    [Fact]
    public void A_payment_id_is_proof_the_money_was_taken()
    {
        // The gateway only has one of these once a payment exists.
        new YocoCheckoutState { Found = true, PaymentId = "p_123" }.IsPaid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("completed")]
    [InlineData("Completed")]
    [InlineData("COMPLETED")]
    [InlineData("succeeded")]
    [InlineData("successful")]
    public void The_words_the_gateway_uses_for_paid_are_read_as_paid(string status)
    {
        new YocoCheckoutState { Found = true, Status = status }.IsPaid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("created")]
    [InlineData("started")]
    [InlineData("failed")]
    [InlineData("expired")]
    [InlineData("cancelled")]
    [InlineData(null)]
    [InlineData("")]
    public void Anything_else_is_not_paid(string status)
    {
        new YocoCheckoutState { Found = true, Status = status }.IsPaid.ShouldBeFalse();
    }

    [Fact]
    public void A_word_this_code_does_not_know_is_not_read_as_paid()
    {
        /* The gateway's vocabulary is theirs to change. Guessing in the
           generous direction would give away a place at the school; guessing
           the other way leaves the webhook, the school and the payer asking
           again, all of which still work. */
        new YocoCheckoutState { Found = true, Status = "something_new" }.IsPaid.ShouldBeFalse();
    }

    [Fact]
    public void An_answer_that_never_came_is_not_paid()
    {
        new YocoCheckoutState { Error = "Could not reach the payment service." }.IsPaid.ShouldBeFalse();
        new YocoCheckoutState { NotConfigured = true }.IsPaid.ShouldBeFalse();
        new YocoCheckoutState().IsPaid.ShouldBeFalse();
    }

    [Fact]
    public void A_payment_id_without_an_answer_is_still_not_paid()
    {
        // Found is what says the gateway actually answered. Without it there is
        // nothing to believe, whatever else happens to be set.
        new YocoCheckoutState { Found = false, PaymentId = "p_123", Status = "completed" }
            .IsPaid.ShouldBeFalse();
    }
}
