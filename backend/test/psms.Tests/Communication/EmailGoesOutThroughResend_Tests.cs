using Microsoft.Extensions.Configuration;
using psms.Communication.Channels;
using psms.Communication.Channels.Email;
using Shouldly;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Communication;

/// <summary>
/// Email stops being a no-op.
/// <para>
/// Every external channel shipped with a gateway that logged and returned
/// "not configured" (COMM-09). Admissions has nothing to tell a family with —
/// an offer with a fourteen-day clock on it reaches nobody — so email is the
/// first one wired, to Resend.
/// </para>
/// <para>
/// What these pin is the behaviour a deployment depends on: that an
/// environment with no key behaves exactly as the scaffold did rather than
/// throwing, that the two configuration mistakes which actually happen say so
/// in words an administrator can act on, and that nothing puts a whole email
/// address in the log.
/// </para>
/// </summary>
public class EmailGoesOutThroughResend_Tests
{
    private static ResendEmailGateway With(params (string Key, string Value)[] settings)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>(
                System.Linq.Enumerable.ToDictionary(settings, x => x.Key, x => x.Value)))
            .Build();

        return new ResendEmailGateway(configuration);
    }

    [Fact]
    public void An_environment_with_no_key_is_simply_not_configured()
    {
        With().IsConfigured.ShouldBeFalse();
    }

    [Fact]
    public void A_key_without_a_from_address_is_not_configured_either()
    {
        // Resend refuses a send with no sender, and finding that out one failed
        // delivery at a time is worse than saying so up front.
        With(("Resend:ApiKey", "re_test_key")).IsConfigured.ShouldBeFalse();
    }

    [Fact]
    public void A_key_and_a_sender_is_configured()
    {
        With(("Resend:ApiKey", "re_test_key"),
             ("Resend:From", "St. Peters <admissions@example.test>"))
            .IsConfigured.ShouldBeTrue();
    }

    [Fact]
    public async Task Sending_without_a_key_reports_not_configured_rather_than_throwing()
    {
        /* The same build runs where email is wired and where it is not. A
           deployment without a key must behave exactly as the scaffold did:
           the dispatcher records NotConfigured, which it tells apart from a
           send that was attempted and failed. */
        var result = await With().SendAsync("parent@example.test", "Subject", "<p>Body</p>");

        result.NotConfigured.ShouldBeTrue();
        result.Success.ShouldBeFalse();
        result.Error.ShouldContain("not configured");
    }

    [Fact]
    public async Task An_empty_recipient_is_refused_before_anything_is_sent()
    {
        var gateway = With(("Resend:ApiKey", "re_test_key"), ("Resend:From", "a@example.test"));

        var result = await gateway.SendAsync("  ", "Subject", "<p>Body</p>");

        result.Success.ShouldBeFalse();
        result.NotConfigured.ShouldBeFalse();
        result.Error.ShouldContain("No email address");
    }

    [Fact]
    public async Task An_empty_body_is_refused_before_anything_is_sent()
    {
        var gateway = With(("Resend:ApiKey", "re_test_key"), ("Resend:From", "a@example.test"));

        var result = await gateway.SendAsync("parent@example.test", "Subject", "   ");

        result.Success.ShouldBeFalse();
        result.Error.ShouldContain("No email content");
    }

    [Theory]
    [InlineData("zodwa.parent@example.test", "zo***@example.test")]
    [InlineData("a@example.test", "a***@example.test")]
    // A short local part gives up less, not more: showing two of two characters
    // would print the whole thing.
    [InlineData("ab@example.test", "a***@example.test")]
    [InlineData("not-an-address", "***")]
    [InlineData(null, "(none)")]
    public void A_log_line_carries_enough_to_trace_and_not_enough_to_harvest(string email, string expected)
    {
        // Application logs are read by more people than the mailbox is, and a
        // parent's address is theirs.
        ResendEmailGateway.Mask(email).ShouldBe(expected);
    }

    [Fact]
    public void The_scaffold_default_still_exists_for_the_channels_that_need_it()
    {
        // WhatsApp, SMS and Push are still no-ops; this is the shape they share,
        // and the shape an unconfigured Resend has to keep.
        var scaffold = new NotConfiguredEmailGateway();

        var result = scaffold.SendAsync("parent@example.test", "s", "<p>b</p>").Result;

        result.NotConfigured.ShouldBeTrue();
        GatewaySendResult.NotConfiguredResult("Email").NotConfigured.ShouldBeTrue();
    }
}
