using Microsoft.Extensions.Configuration;
using psms.Admissions.ApplicationFees;
using psms.Admissions.ApplicationFees.Yoco;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// The application fee can actually be taken, and a webhook claiming it was
/// has to prove itself.
/// <para>
/// The only path that recorded a fee was <c>SimulatePayment</c> — a button
/// that marks it paid and moves no money. A school could not collect, and the
/// screens said so in as many words.
/// </para>
/// <para>
/// Most of these are about the signature, because that is the one place a
/// mistake costs somebody money: a webhook endpoint that settles without
/// checking it is a way to buy a place at the school for nothing, from
/// anywhere on the internet, by posting a JSON body at it.
/// </para>
/// </summary>
public class TheApplicationFeeCanBePaid_Tests
{
    private const string Secret = "whsec_MfKQ9r8GKYqrTwjUPD8ILPZIo2LaLaSw";

    private static YocoPaymentGateway With(params (string Key, string Value)[] settings) =>
        new YocoPaymentGateway(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>(
                settings.ToDictionary(x => x.Key, x => x.Value)))
            .Build());

    private static YocoPaymentGateway Verifying() =>
        With(("Yoco:SecretKey", "sk_test_abc"), ("Yoco:WebhookSecret", Secret));

    /// <summary>Signs exactly the way the verifier expects, so the test can lie convincingly.</summary>
    private static string Sign(string id, string timestamp, string body, string secret = Secret)
    {
        var key = Convert.FromBase64String(secret.Substring("whsec_".Length));
        using var hmac = new HMACSHA256(key);
        return "v1," + Convert.ToBase64String(
            hmac.ComputeHash(Encoding.UTF8.GetBytes($"{id}.{timestamp}.{body}")));
    }

    private static string Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    [Fact]
    public void A_properly_signed_webhook_is_believed()
    {
        var id = "msg_1";
        var at = Now();
        var body = "{\"type\":\"payment.succeeded\"}";

        Verifying().IsSignedByYoco(id, at, Sign(id, at, body), body).ShouldBeTrue();
    }

    [Fact]
    public void A_body_changed_after_signing_is_not()
    {
        /* The attack this exists to stop: take a real delivery, change the
           amount or the reference, post it back. */
        var id = "msg_1";
        var at = Now();
        var signature = Sign(id, at, "{\"type\":\"payment.succeeded\",\"amount\":100}");

        Verifying()
            .IsSignedByYoco(id, at, signature, "{\"type\":\"payment.succeeded\",\"amount\":999999}")
            .ShouldBeFalse();
    }

    [Fact]
    public void A_signature_from_a_different_secret_is_not()
    {
        var id = "msg_1";
        var at = Now();
        var body = "{}";
        var theirs = Sign(id, at, body, "whsec_AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        Verifying().IsSignedByYoco(id, at, theirs, body).ShouldBeFalse();
    }

    [Fact]
    public void A_replayed_delivery_from_last_week_is_not()
    {
        /* Without a timestamp window a captured delivery stays valid for ever
           and can be posted back as often as somebody likes. */
        var id = "msg_1";
        var old = DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds().ToString();
        var body = "{}";

        Verifying().IsSignedByYoco(id, old, Sign(id, old, body), body).ShouldBeFalse();
    }

    [Fact]
    public void A_webhook_with_no_signature_at_all_is_not()
    {
        var gateway = Verifying();

        gateway.IsSignedByYoco("msg_1", Now(), null, "{}").ShouldBeFalse();
        gateway.IsSignedByYoco("msg_1", Now(), "", "{}").ShouldBeFalse();
        gateway.IsSignedByYoco(null, Now(), "v1,x", "{}").ShouldBeFalse();
    }

    [Fact]
    public void With_no_webhook_secret_nothing_is_believed()
    {
        /* Failing closed. The alternative — accepting anything when the secret
           is missing — turns a configuration slip into free places. */
        var id = "msg_1";
        var at = Now();
        var body = "{}";

        With(("Yoco:SecretKey", "sk_test_abc"))
            .IsSignedByYoco(id, at, Sign(id, at, body), body)
            .ShouldBeFalse();
    }

    [Fact]
    public void One_of_several_signatures_matching_is_enough()
    {
        // How a secret is rotated without dropping deliveries mid-flight.
        var id = "msg_1";
        var at = Now();
        var body = "{}";
        var header = "v1,bm90LXRoZS1yaWdodC1vbmU= " + Sign(id, at, body);

        Verifying().IsSignedByYoco(id, at, header, body).ShouldBeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task With_no_key_there_is_no_checkout_and_it_says_which_setting()
    {
        var result = await With().CreateCheckoutAsync(
            200m, "ref", Guid.NewGuid(), "s", "c", "f");

        result.Success.ShouldBeFalse();
        result.NotConfigured.ShouldBeTrue();
        result.Error.ShouldContain("Yoco__SecretKey");
    }

    [Fact]
    public void A_test_key_is_called_a_test_key()
    {
        /* A school live on a test key takes no money and would not find out
           from any screen. */
        With(("Yoco:SecretKey", "sk_test_abc")).IsTestKey.ShouldBeTrue();
        With(("Yoco:SecretKey", "sk_live_abc")).IsTestKey.ShouldBeFalse();
    }

    [Fact]
    public void A_key_on_its_own_switches_the_mode()
    {
        /* Staying Simulated while a real key sits in the configuration would
           take no money and tell nobody — the worst of the three states. */
        var service = File.ReadAllText(SourceFile(
            "psms.Application", "Admissions", "ApplicationFees", "ApplicationFeeAppService.cs"));

        service.ShouldContain("_yoco.IsConfigured ? PaymentGatewayMode.Yoco : PaymentGatewayMode.Simulated");
    }

    [Fact]
    public void The_fee_is_settled_by_the_webhook_and_not_by_the_redirect()
    {
        /* A payer reaching the success page has not necessarily paid, and one
           who paid and closed the tab has. */
        var service = File.ReadAllText(SourceFile(
            "psms.Application", "Admissions", "ApplicationFees", "ApplicationFeeAppService.cs"));
        var start = service.Substring(service.IndexOf("StartPaymentAsync", StringComparison.Ordinal));
        var body = start.Substring(0, start.IndexOf("SimulatePaymentAsync", StringComparison.Ordinal));

        body.ShouldNotContain("MarkPaymentReceived");
        body.ShouldNotContain("SettleAsync");
    }

    [Fact]
    public void Settling_twice_does_nothing_the_second_time()
    {
        /* Yoco retries until it gets a 2xx, so a retry after a slow ack is
           ordinary. Settling again would push an application that has since
           been reviewed back to the start of its workflow. */
        var controller = File.ReadAllText(SourceFile(
            "psms.Web.Host", "Controllers", "YocoWebhookController.cs"));

        controller.ShouldContain("fee.Status == PaymentStatus.Completed");
        controller.ShouldContain("already settled");
    }

    [Fact]
    public void An_unverified_webhook_never_reaches_the_settling()
    {
        var controller = File.ReadAllText(SourceFile(
            "psms.Web.Host", "Controllers", "YocoWebhookController.cs"));

        var check = controller.IndexOf("IsSignedByYoco", StringComparison.Ordinal);
        var settle = controller.IndexOf("await SettleAsync(", StringComparison.Ordinal);

        check.ShouldBeGreaterThan(-1);
        settle.ShouldBeGreaterThan(check);
        controller.ShouldContain("return Unauthorized();");
    }

    [Fact]
    public void Money_is_sent_in_cents_and_rounded_the_way_money_rounds()
    {
        var gateway = File.ReadAllText(SourceFile(
            "psms.Application", "Admissions", "ApplicationFees", "Yoco", "YocoPaymentGateway.cs"));

        gateway.ShouldContain("amount * 100m");
        gateway.ShouldContain("MidpointRounding.AwayFromZero");
    }

    [Fact]
    public void The_probe_is_above_the_amount_Yoco_will_accept()
    {
        /* Found by running it: a R1 probe comes back "You cannot process a
           payment for less than R2.00", which reads like a broken integration
           and is not. Yoco's own floor is R2.00. */
        var diagnostics = File.ReadAllText(SourceFile(
            "psms.Application", "Admissions", "ApplicationFees", "Yoco",
            "PaymentChannelDiagnosticsService.cs"));

        diagnostics.ShouldContain("ProbeAmount = 2m");
        diagnostics.ShouldContain("amount: ProbeAmount");
    }

    [Fact]
    public void The_webhook_url_is_where_this_deployment_actually_answers()
    {
        /* App:ServerRootAddress is a development value on the deployed server,
           so reading it printed https://localhost:44311/api/yoco/webhook — an
           address nobody could register, handed over as the thing to register. */
        var diagnostics = File.ReadAllText(SourceFile(
            "psms.Application", "Admissions", "ApplicationFees", "Yoco",
            "PaymentChannelDiagnosticsService.cs"));

        diagnostics.ShouldContain("IHttpContextAccessor");
        diagnostics.ShouldContain("request.Host.HasValue");

        /* And over https. TLS is terminated at the host's proxy, so the request
           arrives as plain HTTP and Request.Scheme alone said http:// — a URL
           Yoco would refuse, offered as the one to register. */
        diagnostics.ShouldContain("X-Forwarded-Proto");
    }

    [Fact]
    public void There_is_a_way_to_ask_whether_payment_works()
    {
        /* Without one, finding out means registering an applicant and filling
           in a whole application — which is what the email channel cost before
           it got the same thing. */
        typeof(PaymentChannelDiagnosticsService)
            .GetMethod(nameof(PaymentChannelDiagnosticsService.CheckAsync))
            .ShouldNotBeNull();

        var status = new PaymentChannelStatus();
        status.Reachable.ShouldBeFalse();
    }

    [Fact]
    public void A_first_payment_does_not_insert_then_update_the_same_row()
    {
        /* Found by pressing Pay on an application that had never had a fee
           row. InsertAsync followed by UpdateAsync on the same entity marks an
           Added entity Modified, so EF issues an UPDATE against a row that
           does not exist and the save throws — the parent got a bare 500. */
        var service = File.ReadAllText(SourceFile(
            "psms.Application", "Admissions", "ApplicationFees", "ApplicationFeeAppService.cs"));

        var start = service.IndexOf("StartPaymentAsync", StringComparison.Ordinal);
        var body = service.Substring(start, service.IndexOf("SimulatePaymentAsync", StringComparison.Ordinal) - start);

        /* The reference goes in when the row is made, not afterwards. Matched
           on the call rather than its exact argument list, so that adding
           something else the row needs at creation — the gateway's checkout
           id, for one — is not a failure. Writing the row twice is. */
        body.ShouldContain("EnsureFeeAsync(application, amount, reference");
        body.ShouldNotContain("_feeRepository.UpdateAsync");
        body.ShouldNotContain("_feeRepository.InsertAsync");
    }

    [Fact]
    public void The_family_has_a_button_that_takes_them_to_the_gateway()
    {
        var review = Frontend("components", "modules", "admissions", "apply", "ReviewStep.tsx");

        review.ShouldContain("startPaymentAsync(");
        review.ShouldContain("PaymentGatewayMode.Yoco");
        review.ShouldContain("window.location.href = started.redirectUrl");
    }

    [Fact]
    public void Coming_back_from_the_gateway_is_not_treated_as_proof_of_payment()
    {
        /* The redirect says where the payer went, not whether the money
           arrived — settlement comes on a webhook that can be seconds behind.
           Saying "paid" before it has is a lie the school pays for. */
        var review = Frontend("components", "modules", "admissions", "apply", "ReviewStep.tsx");

        review.ShouldContain("Checking with the bank");
        review.ShouldContain("next.awaitingPayment");
        // And it gives up rather than spinning for ever.
        review.ShouldContain("attempts < 7");
    }

    [Fact]
    public void A_cancelled_payment_says_nothing_was_charged()
    {
        var review = Frontend("components", "modules", "admissions", "apply", "ReviewStep.tsx");

        review.ShouldContain("Nothing has been charged");
    }

    [Fact]
    public void The_simulated_button_still_says_what_it_is()
    {
        /* It stays, for a school with no gateway and for walking the journey
           on a machine with no keys — but a simulated payment a parent could
           take for a real one would be worse than no payment step at all. */
        var review = Frontend("components", "modules", "admissions", "apply", "ReviewStep.tsx");

        review.ShouldContain("no money will be taken");
        review.ShouldContain("Simulate paying R");
    }

    private static string Frontend(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "frontend")))
            directory = directory.Parent;

        directory.ShouldNotBeNull("could not find the frontend folder");
        return File.ReadAllText(
            Path.Combine(new[] { directory.FullName, "frontend", "src" }.Concat(parts).ToArray()));
    }

    private static string SourceFile(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;

        directory.ShouldNotBeNull("could not find the repository src folder");
        return Path.Combine(new[] { directory.FullName, "src" }.Concat(parts).ToArray());
    }
}
