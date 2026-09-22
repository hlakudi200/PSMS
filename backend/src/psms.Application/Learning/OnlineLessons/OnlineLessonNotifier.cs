using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using psms.Communication.Dispatch;
using psms.Domain.Academic.Entities;
using psms.Domain.Learning.Entities;
using psms.Domain.Shared.Enums;
using psms.Learning.OnlineLessons.Jobs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Learning.OnlineLessons;

/// <summary>
/// US-TCH-004 student notifications for online lessons: a notice when a lesson
/// is rescheduled, and reminders 24 h and 15 min before it starts. Everything
/// goes out through the COMM-01 dispatcher (in-app + push), so the recipient's
/// preferences and consents apply as they do for any other notification.
/// </summary>
public class OnlineLessonNotifier : ITransientDependency
{
    // Minutes before the start at which a reminder goes out.
    public static readonly int[] ReminderMinutesBefore = { 24 * 60, 15 };

    // Lesson times are shown in South Africa Standard Time (UTC+02:00, no
    // DST), the same zone the OL-001 school-hours rule uses.
    private static readonly TimeSpan SastOffset = TimeSpan.FromHours(2);
    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-ZA");

    // A reminder job only fires for the start time it was queued against.
    // Allow for the database storing microseconds, not .NET ticks.
    private static readonly TimeSpan StartMatchTolerance = TimeSpan.FromSeconds(1);

    private static readonly IReadOnlyList<NotificationChannel> Channels =
        new[] { NotificationChannel.InApp, NotificationChannel.Push };

    private readonly IRepository<OnlineLesson, Guid> _onlineLessonRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly INotificationDispatcher _dispatcher;
    private readonly IBackgroundJobManager _backgroundJobManager;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public OnlineLessonNotifier(
        IRepository<OnlineLesson, Guid> onlineLessonRepository,
        IRepository<Student, Guid> studentRepository,
        INotificationDispatcher dispatcher,
        IBackgroundJobManager backgroundJobManager)
    {
        _onlineLessonRepository = onlineLessonRepository;
        _studentRepository = studentRepository;
        _dispatcher = dispatcher;
        _backgroundJobManager = backgroundJobManager;
    }

    /// <summary>
    /// Queues the 24 h and 15 min reminders for the lesson's current start
    /// time. Reminders whose moment has already passed are skipped. Older
    /// reminders queued for a previous start time are left to expire on
    /// their own (see <see cref="SendReminderAsync"/>).
    /// </summary>
    public async Task ScheduleRemindersAsync(OnlineLesson lesson)
    {
        var now = DateTime.UtcNow;
        foreach (var minutesBefore in ReminderMinutesBefore)
        {
            var delay = lesson.ScheduledStartTime.AddMinutes(-minutesBefore) - now;
            if (delay <= TimeSpan.Zero)
                continue;

            await _backgroundJobManager.EnqueueAsync<OnlineLessonReminderJob, OnlineLessonReminderJobArgs>(
                new OnlineLessonReminderJobArgs
                {
                    TenantId = lesson.TenantId,
                    LessonId = lesson.Id,
                    ExpectedStartUtc = lesson.ScheduledStartTime,
                    MinutesBefore = minutesBefore,
                },
                BackgroundJobPriority.Normal,
                delay);
        }
    }

    /// <summary>
    /// Tells the enrolled students a lesson has moved. Queued, not sent inline,
    /// so the teacher's request doesn't wait on push delivery.
    /// </summary>
    public async Task NotifyRescheduledAsync(OnlineLesson lesson, ClassSubject classSubject, DateTime previousStartUtc)
    {
        var recipients = await GetEnrolledStudentUserIdsAsync(lesson.TenantId, classSubject.ClassId);
        if (recipients.Count == 0)
            return;

        await _dispatcher.EnqueueAsync(new NotificationRequest
        {
            TenantId = lesson.TenantId,
            RecipientUserIds = recipients,
            Type = NotificationType.Academic,
            Priority = NotificationPriority.High,
            Title = "Lesson rescheduled",
            Message = $"\"{lesson.Title}\" has moved from {FormatSast(previousStartUtc)} to {FormatSast(lesson.ScheduledStartTime)}.",
            ActionUrl = ActionUrlFor(lesson),
            EntityType = nameof(OnlineLesson),
            EntityId = lesson.Id,
            IdempotencyKey = $"lesson-rescheduled:{lesson.Id}:{lesson.ScheduledStartTime.Ticks}",
            RequestedChannels = Channels,
        });
    }

    /// <summary>
    /// Sends one queued reminder, unless the lesson is gone, no longer
    /// scheduled, or has moved since the reminder was queued.
    /// </summary>
    public async Task SendReminderAsync(OnlineLessonReminderJobArgs args)
    {
        var lesson = await _onlineLessonRepository
            .GetAll()
            .Include(ol => ol.ClassSubject).ThenInclude(cs => cs.Subject)
            .FirstOrDefaultAsync(ol => ol.Id == args.LessonId && ol.TenantId == args.TenantId);

        if (lesson == null || lesson.Status != OnlineLessonStatus.Scheduled)
            return;

        var drift = (lesson.ScheduledStartTime - args.ExpectedStartUtc).Duration();
        if (drift > StartMatchTolerance)
            return;

        if (lesson.ClassSubject == null)
        {
            Logger.Warn($"Online lesson {lesson.Id} has no class-subject; reminder skipped.");
            return;
        }

        var recipients = await GetEnrolledStudentUserIdsAsync(lesson.TenantId, lesson.ClassSubject.ClassId);
        if (recipients.Count == 0)
            return;

        var subject = lesson.ClassSubject.Subject?.SubjectName;
        var lead = args.MinutesBefore >= 60
            ? $"{args.MinutesBefore / 60} hours"
            : $"{args.MinutesBefore} minutes";
        var what = string.IsNullOrWhiteSpace(subject) ? $"\"{lesson.Title}\"" : $"{subject}: \"{lesson.Title}\"";

        await _dispatcher.DispatchAsync(new NotificationRequest
        {
            TenantId = lesson.TenantId,
            RecipientUserIds = recipients,
            Type = NotificationType.Academic,
            Priority = args.MinutesBefore <= 15 ? NotificationPriority.High : NotificationPriority.Normal,
            Title = $"Online lesson in {lead}",
            Message = $"{what} starts at {FormatSast(lesson.ScheduledStartTime)}.",
            ActionUrl = ActionUrlFor(lesson),
            EntityType = nameof(OnlineLesson),
            EntityId = lesson.Id,
            IdempotencyKey = $"lesson-reminder:{lesson.Id}:{args.MinutesBefore}:{args.ExpectedStartUtc.Ticks}",
            RequestedChannels = Channels,
        });
    }

    private async Task<List<long>> GetEnrolledStudentUserIdsAsync(int? tenantId, Guid classId)
    {
        // Explicit TenantId predicate: the reminder job runs with the tenant
        // filter disabled.
        return await _studentRepository
            .GetAll()
            .Where(s => s.TenantId == tenantId && s.CurrentClassId == classId && s.UserId != null)
            .Select(s => s.UserId.Value)
            .Distinct()
            .ToListAsync();
    }

    // In-app classes open in the PSMS live classroom. External meetings have
    // no PSMS page for students yet (MOB-S04), so the notification carries
    // no link rather than a raw third-party URL.
    private static string ActionUrlFor(OnlineLesson lesson)
        => lesson.Platform == OnlinePlatform.InApp ? $"/live-class/{lesson.Id}" : null;

    private static string FormatSast(DateTime utc)
        => (DateTime.SpecifyKind(utc, DateTimeKind.Utc) + SastOffset)
            .ToString("ddd d MMM, HH:mm", DisplayCulture);
}
