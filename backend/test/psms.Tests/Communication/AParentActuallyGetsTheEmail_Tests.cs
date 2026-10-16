using Microsoft.Extensions.Configuration;
using psms.Communication.Channels.Email;
using psms.Communication.Dispatch;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace psms.Tests.Communication;

/// <summary>
/// An email to a parent is approved, and goes out over the API.
/// <para>
/// Two things were stopping every one of them, and the delivery log on the
/// deployed system had both:
/// </para>
/// <code>
/// Email  Failed      user=203  The mail server did not answer within 20 seconds…
/// Email  Suppressed  user=197  No consent on file for this channel.
/// </code>
/// <para>
/// SMTP was preferred over the HTTP API, on a host that blocks outbound SMTP —
/// so a provider that could not work was chosen while a working one sat beside
/// it. And consent was required for a message about the recipient's own
/// application, which nobody had ever been asked for.
/// </para>
/// </summary>
public class AParentActuallyGetsTheEmail_Tests
{
    private static IConfiguration Config(params (string Key, string Value)[] settings) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>(
                settings.ToDictionary(x => x.Key, x => x.Value)))
            .Build();

    private static ConfiguredEmailGateway Routed(params (string Key, string Value)[] settings)
    {
        var configuration = Config(settings);
        return new ConfiguredEmailGateway(
            new SmtpEmailGateway(configuration), new ResendEmailGateway(configuration));
    }

    private static readonly (string, string)[] BothConfigured =
    {
        ("Smtp:Host", "smtp.gmail.com"),
        ("Smtp:UserName", "school@example.test"),
        ("Smtp:Password", "app-password"),
        ("Resend:ApiKey", "re_test_key"),
        ("Resend:From", "School <admissions@example.test>"),
    };

    [Fact]
    public void With_both_configured_the_api_is_used()
    {
        /* The whole point. This host blocks outbound SMTP, as most managed
           platforms do, so preferring it meant choosing a provider that could
           not work while a working one sat beside it. */
        Routed(BothConfigured).SelectedProvider.ShouldBe("Resend");
    }

    [Fact]
    public void Smtp_is_still_there_for_a_school_that_only_has_that()
    {
        Routed(
            ("Smtp:Host", "smtp.gmail.com"),
            ("Smtp:UserName", "school@example.test"),
            ("Smtp:Password", "app-password"))
            .SelectedProvider.ShouldBe("Smtp");
    }

    [Fact]
    public void With_neither_the_api_is_named_first()
    {
        // What somebody should reach for, named in the order to try it.
        Routed().SelectedProvider.ShouldBe("none");

        var gateway = File.ReadAllText(SourceFile(
            "psms.Application", "Communication", "Channels", "Email", "ConfiguredEmailGateway.cs"));
        var message = gateway.Substring(gateway.IndexOf("Email is not configured.", StringComparison.Ordinal));

        message.IndexOf("Resend__ApiKey", StringComparison.Ordinal)
            .ShouldBeLessThan(message.IndexOf("Smtp__Host", StringComparison.Ordinal));
    }

    [Fact]
    public void A_message_about_your_own_application_is_marked_as_such()
    {
        var notifier = File.ReadAllText(SourceFile(
            "psms.Application", "Admissions", "Offers", "AdmissionsNotifier.cs"));

        notifier.ShouldContain("IsTransactional = true");
    }

    [Fact]
    public void Never_having_been_asked_no_longer_blocks_a_transactional_message()
    {
        /* Nobody had ever been asked, so no applicant had a consent row, so
           the email telling a family they had been offered a place was
           suppressed before it reached a provider — while the offer ran out. */
        var dispatcher = Dispatcher();

        dispatcher.ShouldContain("!request.IsTransactional && !gate.GrantedChannels.Contains(channel)");
    }

    [Fact]
    public void But_an_actual_refusal_still_does()
    {
        /* Silence is not a no. A no is. Only one of the two may ever be
           overridden, and it is not this one. */
        var dispatcher = Dispatcher();

        dispatcher.ShouldContain("gate.RefusedChannels.Contains(channel)");
        dispatcher.ShouldContain("The recipient has turned this channel off.");

        // The refusal is checked before the transactional exemption, so the
        // exemption can never reach somebody who said no.
        var refusal = dispatcher.IndexOf("gate.RefusedChannels.Contains(channel)", StringComparison.Ordinal);
        var exemption = dispatcher.IndexOf("!request.IsTransactional", StringComparison.Ordinal);
        refusal.ShouldBeLessThan(exemption);
    }

    [Fact]
    public void And_so_does_turning_the_category_off()
    {
        /* The per-category preference is a deliberate act too, and is checked
           before either of the consent branches. */
        var dispatcher = Dispatcher();

        var disabled = dispatcher.IndexOf("gate.DisabledChannels.Contains(channel)", StringComparison.Ordinal);
        var refusal = dispatcher.IndexOf("gate.RefusedChannels.Contains(channel)", StringComparison.Ordinal);

        disabled.ShouldBeGreaterThan(-1);
        disabled.ShouldBeLessThan(refusal);
    }

    [Fact]
    public void A_refusal_is_told_apart_from_a_blank()
    {
        /* The gate used to load only the granted rows, so a row saying no and
           no row at all were the same thing to it — and the exemption would
           have walked straight past somebody's refusal. */
        var dispatcher = Dispatcher();

        dispatcher.ShouldContain("consents.Where(c => c.IsGranted)");
        dispatcher.ShouldContain("consents.Where(c => !c.IsGranted)");
    }

    [Fact]
    public void Nothing_here_invents_a_consent_row()
    {
        /* The alternative fix was to write consent rows for every family who
           had already applied. That is manufacturing a record of something
           that never happened, and it would outlive any argument for it. */
        var dispatcher = Dispatcher();

        dispatcher.ShouldNotContain("new NotificationConsent");
        dispatcher.ShouldNotContain("_consentRepository.InsertAsync");
    }

    private static string Dispatcher() => File.ReadAllText(SourceFile(
        "psms.Application", "Communication", "Dispatch", "NotificationDispatcher.cs"));

    private static string SourceFile(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;

        directory.ShouldNotBeNull("could not find the repository src folder");
        return Path.Combine(new[] { directory.FullName, "src" }.Concat(parts).ToArray());
    }
}
