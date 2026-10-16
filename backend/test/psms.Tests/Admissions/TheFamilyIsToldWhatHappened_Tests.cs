using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// A family finds out what happened to their application.
/// <para>
/// Nothing in admissions notified anybody — grep <c>Notification</c> under the
/// module and it returned nothing at all. A family found out by signing in and
/// looking, including that they had been offered a place, with a fourteen-day
/// clock already running on it. An offer nobody is told about is not an offer,
/// and a deadline nobody is told about is a trap.
/// </para>
/// </summary>
public class TheFamilyIsToldWhatHappened_Tests
{
    private static string SourceFile(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;

        directory.ShouldNotBeNull("could not find the repository src folder");
        return Path.Combine(new[] { directory.FullName, "src" }.Concat(parts).ToArray());
    }

    private static string Notifier() =>
        File.ReadAllText(SourceFile("psms.Application", "Admissions", "Offers", "AdmissionsNotifier.cs"));

    private static string Expirer() =>
        File.ReadAllText(SourceFile("psms.Application", "Admissions", "Offers", "AdmissionOfferExpirer.cs"));

    private static string Handlers() =>
        File.ReadAllText(SourceFile("psms.Application", "Workflow", "Engine", "Handlers", "WorkflowEntityHandlers.cs"));

    [Fact]
    public void Every_outcome_of_a_decision_is_told_to_the_family()
    {
        var source = Notifier();

        source.ShouldContain("ApplicationStatus.Approved");
        source.ShouldContain("ApplicationStatus.Waitlisted");
        source.ShouldContain("ApplicationStatus.Rejected");
    }

    [Fact]
    public void The_offer_notice_carries_the_deadline_the_family_is_working_to()
    {
        /* The thing that makes it urgent. An offer notice without the date is
           the trap this ticket is about, one step removed. */
        var source = Notifier();

        source.ShouldContain("Let them know by");
        source.ShouldContain("ExpiryDate");
    }

    [Fact]
    public void The_deadline_is_said_in_the_schools_own_time()
    {
        /* Stored UTC, and SAST is two hours ahead — so a deadline at local
           midnight renders as the previous day if nobody converts it. Exactly
           the fault #399 is open about, and not one to repeat in a message
           telling a parent when they must answer by. */
        var source = Notifier();

        source.ShouldContain("ConvertTimeFromUtc");
        source.ShouldContain("Africa/Johannesburg");
        // Linux names it differently from Windows and the deployment is Linux.
        source.ShouldContain("South Africa Standard Time");
    }

    [Fact]
    public void A_family_is_warned_before_the_offer_goes_not_only_after()
    {
        var expirer = Expirer();

        expirer.ShouldContain("WarnAboutOffersRunningOutAsync");
        expirer.ShouldContain("OfferRunningOutAsync");
        expirer.ShouldContain("OfferLapsedAsync");
        // Once a week out, once two days out.
        expirer.ShouldContain("RemindAt = { 7, 2 }");
    }

    [Fact]
    public void A_reminder_cannot_repeat_on_the_next_sweep()
    {
        /* The sweep runs hourly. Without a key carrying the day count, a family
           with seven days left would be told so twenty-four times. */
        var source = Notifier();

        source.ShouldContain("IdempotencyKey");
        source.ShouldContain("offer-running-out:{daysLeft}");
    }

    [Fact]
    public void A_failed_send_never_undoes_the_decision()
    {
        /* The principal's approval must stand even if the email provider is
           having a bad afternoon. A notification is never worth a rollback. */
        var source = Notifier();

        source.ShouldContain("catch (Exception");
        source.ShouldContain("Logger.Error");
    }

    [Fact]
    public void Telling_the_family_does_not_block_the_write_back()
    {
        /* The write-back is a plain synchronous method that changes the entity
           and nothing else. Blocking on the dispatcher from inside it would
           make an approval wait on an email provider. */
        var handlers = Handlers();

        handlers.ShouldContain("protected virtual Task AfterCompletedAsync");
        handlers.ShouldNotContain("GetAwaiter().GetResult()");
    }

    [Fact]
    public void The_hook_runs_after_the_entity_has_been_written()
    {
        /* Telling a family they have a place before the place is saved is the
           one ordering that must not happen. */
        var handlers = Handlers();
        var update = handlers.IndexOf("await Repository.UpdateAsync(entity);", StringComparison.Ordinal);
        var hook = handlers.IndexOf("await after(entity, ctx);", StringComparison.Ordinal);

        update.ShouldBeGreaterThan(-1);
        hook.ShouldBeGreaterThan(update);
    }

    [Fact]
    public void Telling_the_family_is_queued_not_awaited_in_the_transaction()
    {
        /* Found on the deployed system. This is called from inside the workflow
           transaction that writes the decision back, and sending is a network
           round trip to somebody else's server. Dispatching here made a
           principal's Approve wait on an SMTP connection — and when that
           connection hung, the request never returned and the decision rolled
           back: the application sat at UnderReview with the workflow still on
           the Principal Decision step, as though nobody had pressed anything.
           EnqueueAsync exists for "slow external channels". */
        var source = Notifier();

        source.ShouldContain("_dispatcher.EnqueueAsync");
        source.ShouldNotContain("_dispatcher.DispatchAsync");
    }

    [Fact]
    public void The_smtp_send_is_bounded_by_something_that_actually_works()
    {
        /* SmtpClient.Timeout bounds the synchronous Send and is ignored by
           SendMailAsync. A host that silently drops outbound SMTP — many block
           it to deter spam — leaves the connect hanging with no timeout at all.
           Cancellation is the only thing that bounds the async path. */
        var smtp = System.IO.File.ReadAllText(SourceFile(
            "psms.Application", "Communication", "Channels", "Email", "SmtpEmailGateway.cs"));

        smtp.ShouldContain("CancellationTokenSource");
        smtp.ShouldContain("SendMailAsync(message, cancellation.Token)");
        smtp.ShouldContain("catch (OperationCanceledException)");
    }

    [Fact]
    public void It_goes_in_app_and_by_email()
    {
        /* In-app so it is waiting when they next sign in, email because the
           address they registered with is the only one the school is sure of. */
        var source = Notifier();

        source.ShouldContain("NotificationChannel.InApp");
        source.ShouldContain("NotificationChannel.Email");
    }

    [Fact]
    public void The_notifier_is_reachable_from_the_places_that_decide()
    {
        var notifier = typeof(psms.Admissions.Offers.AdmissionsNotifier);

        notifier.GetMethod("DecisionMadeAsync").ShouldNotBeNull();
        notifier.GetMethod("OfferRunningOutAsync").ShouldNotBeNull();
        notifier.GetMethod("OfferLapsedAsync").ShouldNotBeNull();

        // Taken by the handler that writes the decision back, and by the sweep.
        typeof(psms.Workflow.Engine.Handlers.ApplicationWorkflowHandler)
            .GetConstructors().Single()
            .GetParameters().Select(p => p.ParameterType)
            .ShouldContain(notifier);

        typeof(psms.Admissions.Offers.AdmissionOfferExpirer)
            .GetConstructors().Single()
            .GetParameters().Select(p => p.ParameterType)
            .ShouldContain(notifier);
    }
}
