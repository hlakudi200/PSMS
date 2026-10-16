using psms.Authorization.Accounts.Dto;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// A family is asked whether the school may email them, and the answer is
/// written down.
/// <para>
/// Every channel but in-app is consent-gated (COMM-05), and nothing in the
/// system ever created a consent row for an applicant. So the email telling a
/// family they had been offered a place was suppressed before it reached the
/// provider — the delivery log read "No consent on file for this channel",
/// which nobody reads — while the in-app notice landed for a family who were
/// not signed in and had no idea it existed. The offer then ran out of time.
/// </para>
/// <para>
/// Observed on the deployed system, on the very notification #415 was built to
/// send:
/// <code>
/// Email  Suppressed  user=197   error: No consent on file for this channel.
/// InApp  Delivered   user=197
/// </code>
/// </para>
/// </summary>
public class TheFamilyIsAskedBeforeBeingEmailed_Tests
{
    private static string SourceFile(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;

        directory.ShouldNotBeNull("could not find the repository src folder");
        return Path.Combine(new[] { directory.FullName, "src" }.Concat(parts).ToArray());
    }

    private static string Accounts() => File.ReadAllText(
        SourceFile("psms.Application", "Authorization", "Accounts", "AccountAppService.cs"));

    [Fact]
    public void Registration_takes_an_answer()
    {
        typeof(RegisterApplicantInput)
            .GetProperty(nameof(RegisterApplicantInput.MayEmailMe))
            .ShouldNotBeNull();
    }

    [Fact]
    public void It_is_nullable_so_an_older_client_is_not_silently_opted_out()
    {
        /* A bool that arrives absent binds as false, which would read as a
           refusal nobody made — and would reinstate exactly the silence this
           fixes. Nullable, defaulted where it is read. */
        typeof(RegisterApplicantInput)
            .GetProperty(nameof(RegisterApplicantInput.MayEmailMe))
            .PropertyType.ShouldBe(typeof(bool?));

        Accounts().ShouldContain("input.MayEmailMe ?? true");
    }

    [Fact]
    public void The_answer_is_recorded_either_way()
    {
        /* A refusal is as much an answer as a grant, and one written down is
           one the school cannot talk itself out of later. */
        var source = Accounts();

        source.ShouldContain("RecordEmailConsentAsync");
        source.ShouldContain("consent.Grant(ConsentSource)");
        source.ShouldContain("consent.Revoke(ConsentSource)");
    }

    [Fact]
    public void It_is_recorded_against_the_channel_that_was_suppressed()
    {
        Accounts().ShouldContain("NotificationChannel.Email");
    }

    [Fact]
    public void Where_the_consent_came_from_is_kept()
    {
        // Consent with no provenance is consent nobody can defend.
        Accounts().ShouldContain("\"applicant-registration\"");
    }

    [Fact]
    public void Granting_and_revoking_move_the_dates_the_gate_reads()
    {
        var consent = new NotificationConsent(Guid.NewGuid(), 1, 42, NotificationChannel.Email);

        consent.Grant("applicant-registration");
        consent.IsGranted.ShouldBeTrue();
        consent.GrantedDate.ShouldNotBeNull();

        consent.Revoke("applicant-registration");
        consent.IsGranted.ShouldBeFalse();
        consent.RevokedDate.ShouldNotBeNull();
    }

    [Fact]
    public void In_app_is_never_gated_on_this()
    {
        /* In-app is the inbox of record and is exempt — which is why the family
           above got something and the school thought all was well. Nothing here
           may change that, or a refusal to be emailed would silence the lot. */
        var dispatcher = File.ReadAllText(SourceFile(
            "psms.Application", "Communication", "Dispatch", "NotificationDispatcher.cs"));

        dispatcher.ShouldContain("channel != NotificationChannel.InApp");
    }
}
