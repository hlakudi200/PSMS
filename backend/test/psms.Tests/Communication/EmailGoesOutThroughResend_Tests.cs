using System.Reflection;
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
    private static IConfiguration Config(params (string Key, string Value)[] settings) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>(
                System.Linq.Enumerable.ToDictionary(settings, x => x.Key, x => x.Value)))
            .Build();

    private static ResendEmailGateway With(params (string Key, string Value)[] settings) =>
        new ResendEmailGateway(Config(settings));

    private static ConfiguredEmailGateway Routed(params (string Key, string Value)[] settings)
    {
        var configuration = Config(settings);
        return new ConfiguredEmailGateway(
            new SmtpEmailGateway(configuration), new ResendEmailGateway(configuration));
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
        // Which setting, because the operator reads the delivery log.
        result.Error.ShouldContain("Resend__ApiKey");
        result.Error.ShouldContain("Resend__From");
    }

    [Fact]
    public async Task A_key_with_no_from_address_says_which_one_is_missing()
    {
        /* The usual way to get this wrong, and the one that cost a round trip:
           the key is set, the from address is not, and "Email gateway is not
           configured" sends you looking at the key you just set. */
        var gateway = With(("Resend:ApiKey", "re_test_key"));

        var result = await gateway.SendAsync("parent@example.test", "s", "<p>b</p>");

        result.NotConfigured.ShouldBeTrue();
        result.Error.ShouldContain("Resend__From");
        result.Error.ShouldNotContain("Resend__ApiKey");
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
    public void An_smtp_account_is_enough_on_its_own()
    {
        /* The point of having both. Resend will not deliver to anybody but the
           account holder until a domain is verified in its dashboard, which
           needs DNS access and a wait; a Gmail app password works in five
           minutes. */
        new SmtpEmailGateway(Config(
            ("Smtp:Host", "smtp.gmail.com"),
            ("Smtp:UserName", "school@example.test"),
            ("Smtp:Password", "app-password")))
            .IsConfigured.ShouldBeTrue();
    }

    [Fact]
    public void An_smtp_host_with_no_password_is_not_enough()
    {
        new SmtpEmailGateway(Config(("Smtp:Host", "smtp.gmail.com")))
            .IsConfigured.ShouldBeFalse();
    }

    [Fact]
    public async Task With_neither_configured_both_options_are_named()
    {
        /* Naming only the provider somebody did not choose is how "Email
           gateway is not configured" sent an operator looking at the key they
           had just set. */
        var result = await Routed().SendAsync("parent@example.test", "s", "<p>b</p>");

        result.NotConfigured.ShouldBeTrue();
        result.Error.ShouldContain("Smtp__Host");
        result.Error.ShouldContain("Resend__ApiKey");
    }

    [Fact]
    public async Task Smtp_wins_when_a_school_has_filled_it_in()
    {
        /* A school that configured SMTP has said what it wants. Reaching for
           Resend instead would send from an address they did not choose. */
        var routed = Routed(
            ("Smtp:Host", "smtp.invalid.test"),
            ("Smtp:UserName", "school@example.test"),
            ("Smtp:Password", "app-password"),
            ("Resend:ApiKey", "re_test_key"),
            ("Resend:From", "a@example.test"));

        var result = await routed.SendAsync("parent@example.test", "s", "<p>b</p>");

        // The host does not resolve, so it fails — but as SMTP, not as Resend,
        // and not as "not configured".
        result.NotConfigured.ShouldBeFalse();
        result.Success.ShouldBeFalse();
        result.Error.ShouldContain("mail server");
    }

    [Fact]
    public void The_diagnostic_says_which_provider_it_tried()
    {
        /* Precedence is invisible from outside. A deployment whose SMTP
           settings cannot work — this host blocks outbound SMTP — keeps
           choosing SMTP until those settings are cleared, and "it still is not
           sending" after adding a Resend key is a confusing place to be
           without being told which one is being tried. */
        Routed().SelectedProvider.ShouldBe("none");

        Routed(("Resend:ApiKey", "re_test_key"), ("Resend:From", "a@example.test"))
            .SelectedProvider.ShouldBe("Resend");

        Routed(("Smtp:Host", "smtp.gmail.com"), ("Smtp:UserName", "a@example.test"), ("Smtp:Password", "p"))
            .SelectedProvider.ShouldBe("Smtp");

        // Both set: SMTP still wins, which is the thing that needs saying out loud.
        Routed(("Smtp:Host", "smtp.gmail.com"), ("Smtp:UserName", "a@example.test"), ("Smtp:Password", "p"),
               ("Resend:ApiKey", "re_test_key"), ("Resend:From", "a@example.test"))
            .SelectedProvider.ShouldBe("Smtp");
    }

    [Fact]
    public void There_is_a_way_to_ask_whether_email_works_at_all()
    {
        /* Finding out otherwise meant registering an applicant, filling in an
           application, pushing it through four workflow steps, making an
           admission decision and reading the delivery log — a ten-minute round
           trip per guess, on live data. Three guesses in: a missing consent
           row, a missing setting, and a host that blocks outbound SMTP. */
        var service = typeof(EmailChannelDiagnosticsService);

        service.GetMethod(nameof(EmailChannelDiagnosticsService.SendTestEmailAsync))
            .ShouldNotBeNull();

        service.GetCustomAttribute<Abp.Authorization.AbpAuthorizeAttribute>()
            .Permissions.ShouldContain(psms.Authorization.PermissionNames.Communication_Notifications_Configure);
    }

    [Fact]
    public void The_test_email_can_only_go_to_whoever_asked_for_it()
    {
        /* A diagnostic that can address a stranger is a spam tool. It takes no
           recipient at all — the address comes from the session. */
        typeof(EmailChannelDiagnosticsService)
            .GetMethod(nameof(EmailChannelDiagnosticsService.SendTestEmailAsync))
            .GetParameters().ShouldBeEmpty();

        var source = System.IO.File.ReadAllText(SourceFile());
        source.ShouldContain("AbpSession.GetUserId()");
        source.ShouldContain("ResendEmailGateway.Mask(user.EmailAddress)");
    }

    private static string SourceFile()
    {
        var directory = new System.IO.DirectoryInfo(System.AppContext.BaseDirectory);
        while (directory != null && !System.IO.Directory.Exists(System.IO.Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;

        return System.IO.Path.Combine(directory.FullName, "src", "psms.Application",
            "Communication", "Channels", "Email", "EmailChannelDiagnosticsService.cs");
    }

    [Fact]
    public async Task The_scaffold_default_still_exists_for_the_channels_that_need_it()
    {
        // WhatsApp, SMS and Push are still no-ops; this is the shape they share,
        // and the shape an unconfigured Resend has to keep.
        var scaffold = new NotConfiguredEmailGateway();

        var result = await scaffold.SendAsync("parent@example.test", "s", "<p>b</p>");

        result.NotConfigured.ShouldBeTrue();
        GatewaySendResult.NotConfiguredResult("Email").NotConfigured.ShouldBeTrue();
    }
}
