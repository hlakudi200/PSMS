using Abp.Dependency;
using Castle.Core.Logging;
using psms.Communication.Dispatch;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Admissions.Offers;

/// <summary>
/// Tells a family what has happened to their application.
/// <para>
/// Nothing in admissions notified anybody. A family found out by signing in and
/// looking — including that they had been offered a place, with a fourteen-day
/// clock already running on it. An offer nobody is told about is not an offer,
/// and a deadline nobody is told about is a trap.
/// </para>
/// <para>
/// Everything goes through the COMM-01 dispatcher, so consent, per-category
/// preferences, routing and the delivery log all apply and nothing here knows
/// what a channel is. Every send carries an idempotency key: these fire from
/// workflow transitions and an hourly sweep, both of which can run twice.
/// </para>
/// </summary>
public class AdmissionsNotifier : ITransientDependency
{
    /// <summary>
    /// In-app so it is waiting when they next sign in, and email because that
    /// is the only address the school is certain to have — it is what they
    /// registered with.
    /// </summary>
    private static readonly IReadOnlyList<NotificationChannel> Channels =
        new[] { NotificationChannel.InApp, NotificationChannel.Email };

    private readonly INotificationDispatcher _dispatcher;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public AdmissionsNotifier(INotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    /// <summary>The school has decided. Said in the words the outcome deserves.</summary>
    public Task DecisionMadeAsync(Application a) => a.Status switch
    {
        ApplicationStatus.Approved => SendAsync(a, "decision:offered", NotificationPriority.High,
            $"{Learner(a)} has been offered a place",
            Expiry(a) is { } by
                ? $"The school has offered {Learner(a)} a place in {Grade(a)}. Let them know by {by} — after that the place may go to somebody else."
                : $"The school has offered {Learner(a)} a place in {Grade(a)}. Open your application to accept or turn it down."),

        ApplicationStatus.Waitlisted => SendAsync(a, "decision:waitlisted", NotificationPriority.Normal,
            $"{Learner(a)} is on the waiting list",
            $"{Grade(a)} is full for now. The school will be in touch if a place comes free."),

        ApplicationStatus.Rejected => SendAsync(a, "decision:rejected", NotificationPriority.Normal,
            $"The school's decision about {Learner(a)}",
            string.IsNullOrWhiteSpace(a.DecisionReason)
                ? $"The school is not able to offer {Learner(a)} a place in {Grade(a)} this year."
                : $"The school is not able to offer {Learner(a)} a place in {Grade(a)} this year. {a.DecisionReason}"),

        // Anything else is not a decision the family needs telling about.
        _ => Task.CompletedTask,
    };

    /// <summary>The offer is running out and nobody has answered.</summary>
    public Task OfferRunningOutAsync(Application a, int daysLeft) => SendAsync(a,
        // Keyed on the day so a reminder at seven days and one at two days are
        // different notifications, and neither repeats on the next sweep.
        $"offer-running-out:{daysLeft}",
        daysLeft <= 2 ? NotificationPriority.High : NotificationPriority.Normal,
        daysLeft <= 0
            ? $"Today is the last day to answer about {Learner(a)}"
            : $"{daysLeft} day{(daysLeft == 1 ? "" : "s")} left to answer about {Learner(a)}",
        Expiry(a) is { } by
            ? $"The place offered to {Learner(a)} in {Grade(a)} is held until {by}. Open your application to accept it or turn it down."
            : $"The place offered to {Learner(a)} in {Grade(a)} is about to run out. Open your application to accept it or turn it down.");

    /// <summary>It ran out. Said plainly, and without pretending it is reversible.</summary>
    public Task OfferLapsedAsync(Application a) => SendAsync(a, "offer-lapsed", NotificationPriority.High,
        $"The offer for {Learner(a)} has run out",
        $"The place the school offered in {Grade(a)} was not taken up in time and has been released. "
        + "Speak to the school if you still want it — they would have to make a fresh offer.");

    private async Task SendAsync(Application a, string what, NotificationPriority priority, string title, string message)
    {
        if (!a.CreatorUserId.HasValue)
        {
            /* Applications are created by the family's own sign-in, so this
               should not happen — but a notification is never worth throwing
               over, least of all inside a workflow transition. */
            Logger.Warn($"No applicant account on {a.ApplicationNumber}; cannot send {what}.");
            return;
        }

        try
        {
            await _dispatcher.DispatchAsync(new NotificationRequest
            {
                TenantId = a.TenantId,
                RecipientUserIds = new[] { a.CreatorUserId.Value },
                Type = NotificationType.General,
                Priority = priority,
                Title = title,
                Message = message,
                ActionUrl = "/apply",
                EntityType = nameof(Application),
                EntityId = a.Id,
                IdempotencyKey = $"admissions:{what}:{a.Id}",
                RequestedChannels = Channels,
            });
        }
        catch (Exception ex)
        {
            /* A decision that was taken must stand even if telling the family
               about it fails. The alternative is a principal's approval rolled
               back by an email provider having a bad afternoon. */
            Logger.Error($"Could not notify about {a.ApplicationNumber} ({what}): {ex.Message}", ex);
        }
    }

    private static string Learner(Application a) =>
        $"{a.ProspectiveStudentFirstName} {a.ProspectiveStudentLastName}".Trim() is { Length: > 0 } name
            ? name
            : "your child";

    private static string Grade(Application a) => a.AppliedGrade?.GradeName ?? "the grade applied for";

    /// <summary>The deadline in the school's own time, not UTC.</summary>
    private static string Expiry(Application a) =>
        a.ExpiryDate.HasValue
            ? TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(a.ExpiryDate.Value, DateTimeKind.Utc), SouthAfrica).ToString("d MMMM yyyy")
            : null;

    private static readonly TimeZoneInfo SouthAfrica = Resolve();

    private static TimeZoneInfo Resolve()
    {
        // Windows and Linux name it differently and the deployment is Linux.
        foreach (var id in new[] { "South Africa Standard Time", "Africa/Johannesburg" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }

        return TimeZoneInfo.CreateCustomTimeZone("SAST", TimeSpan.FromHours(2), "SAST", "SAST");
    }
}
