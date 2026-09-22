using Abp.Dependency;
using Abp.Domain.Repositories;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using psms.Communication.Dispatch;
using psms.Domain.Academic.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Assessment.Reports;

/// <summary>
/// RC-10. Tells a learner's parents that their report card is available.
/// <para>
/// RE-003 requires "notification sent to parents on publish", US-ADM-009
/// "parents receive notification when reports are published", and US-PAR-004
/// "receive notification when new report published". Publishing set a date and
/// a status and did nothing else; the <c>report.published</c> template key was
/// referenced in a doc comment and used nowhere.
/// </para>
/// <para>
/// Everything goes out through the COMM-01 dispatcher, so channel preferences
/// and POPIA consent apply as they do to any other notification (COMM-05). The
/// message carries no marks — a notification is not the report card, and the
/// card itself stays behind the permission-checked, signed download (RC-04).
/// </para>
/// </summary>
public class ReportPublishedNotifier : ITransientDependency
{
    /// <summary>
    /// COMM-06 template key. Referenced by NotificationTemplate's documentation
    /// since the communication work landed, and until now sent by nothing.
    /// </summary>
    public const string TemplateKey = "report.published";

    private static readonly IReadOnlyList<NotificationChannel> Channels =
        new[] { NotificationChannel.InApp, NotificationChannel.Push, NotificationChannel.Email };

    private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("en-ZA");

    private readonly IRepository<StudentParent, Guid> _studentParentRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly INotificationDispatcher _dispatcher;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public ReportPublishedNotifier(
        IRepository<StudentParent, Guid> studentParentRepository,
        IRepository<Student, Guid> studentRepository,
        INotificationDispatcher dispatcher)
    {
        _studentParentRepository = studentParentRepository;
        _studentRepository = studentRepository;
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// Queues the notification for every parent linked to the learner, and for
    /// the learner themselves where they have a user account.
    /// <para>
    /// Queued rather than sent inline: an external channel must not hold up the
    /// publish, and a failure to notify must not roll back a report card that
    /// was legitimately published. A learner with no linked parent is not an
    /// error — it is a school that has not captured the link yet — but it is
    /// worth a log line, because it means nobody was told.
    /// </para>
    /// </summary>
    public async Task NotifyAsync(Report report, string studentName, string reportTypeLabel)
    {
        if (report == null)
            return;

        var recipients = await ResolveRecipientsAsync(report.TenantId, report.StudentId);

        if (recipients.Count == 0)
        {
            Logger.Warn(
                $"Report {report.Id} was published but there is nobody to notify — "
                + $"learner {report.StudentId} has no linked parent with a user account.");
            return;
        }

        var name = string.IsNullOrWhiteSpace(studentName) ? "your child" : studentName;
        var label = string.IsNullOrWhiteSpace(reportTypeLabel) ? "report card" : reportTypeLabel;

        var request = new NotificationRequest
        {
            TenantId = report.TenantId,
            RecipientUserIds = recipients,
            Type = NotificationType.Academic,
            Priority = NotificationPriority.Normal,
            Title = $"{label} available for {name}",
            Message =
                $"{name}'s {label.ToLower(DisplayCulture)} has been published and is ready to view.",
            EntityType = nameof(Report),
            EntityId = report.Id,
            ActionUrl = $"/parent/reports/{report.Id}",
            TemplateKey = TemplateKey,
            Variables = new Dictionary<string, string>
            {
                ["studentName"] = name,
                ["reportType"] = label,
                ["publishedDate"] = (report.PublishedDate ?? DateTime.UtcNow)
                    .ToString("dd MMMM yyyy", DisplayCulture),
            },
            RequestedChannels = Channels,

            // COMM-02: a report card is published once. If a retry or a double
            // click reaches here again, the dispatcher skips a channel it has
            // already delivered on rather than telling a parent twice.
            IdempotencyKey = $"report.published:{report.Id}",
        };

        await _dispatcher.EnqueueAsync(request);
    }

    /// <summary>
    /// As <see cref="NotifyAsync"/>, but never throws: a report card that has
    /// been published stays published even if nothing could be sent about it.
    /// </summary>
    public async Task TryNotifyAsync(Report report, string studentName, string reportTypeLabel)
    {
        try
        {
            await NotifyAsync(report, studentName, reportTypeLabel);
        }
        catch (Exception ex)
        {
            Logger.Error($"Could not queue the publish notification for report {report?.Id}.", ex);
        }
    }

    /// <summary>
    /// The user accounts to tell: the learner's parents, and the learner where
    /// they have one of their own.
    /// </summary>
    private async Task<List<long>> ResolveRecipientsAsync(int? tenantId, Guid studentId)
    {
        var parentUserIds = await _studentParentRepository
            .GetAll()
            .Where(sp => sp.StudentId == studentId)
            .Where(sp => sp.Parent.TenantId == tenantId)
            .Where(sp => sp.Parent.UserId != 0)
            .Select(sp => sp.Parent.UserId)
            .Distinct()
            .ToListAsync();

        var studentUserId = await _studentRepository
            .GetAll()
            .Where(s => s.Id == studentId && s.TenantId == tenantId)
            .Select(s => s.UserId)
            .FirstOrDefaultAsync();

        var recipients = new HashSet<long>(parentUserIds);

        if (studentUserId.HasValue && studentUserId.Value != 0)
            recipients.Add(studentUserId.Value);

        return recipients.ToList();
    }
}
