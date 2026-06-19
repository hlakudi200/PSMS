using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;

namespace psms.Communication.Dispatch;

/// <summary>
/// COMM-01: a channel-agnostic request to notify one or more recipients about
/// something. App code raises this via <see cref="INotificationDispatcher"/> and
/// never talks to a channel/provider directly. TenantId is carried explicitly so
/// a request can be dispatched outside an ABP session (e.g. background jobs in
/// COMM-02).
/// </summary>
public class NotificationRequest
{
    public int? TenantId { get; set; }

    public IReadOnlyList<long> RecipientUserIds { get; set; } = new List<long>();

    public NotificationType Type { get; set; }

    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    public string Title { get; set; }

    public string Message { get; set; }

    public string ActionUrl { get; set; }

    public string EntityType { get; set; }

    public Guid? EntityId { get; set; }

    /// <summary>
    /// COMM-02: optional dedup key. When set, the dispatcher skips a channel for a
    /// recipient if a non-failed delivery-log row already exists for the same
    /// (key, channel, recipient) — so a retry (or a duplicate trigger) never
    /// double-sends. Null = no dedup (e.g. ad-hoc manual sends).
    /// </summary>
    public string IdempotencyKey { get; set; }

    /// <summary>Convenience for the common single-recipient case.</summary>
    public static NotificationRequest ForUser(
        int? tenantId, long userId, NotificationType type, string title, string message)
        => new NotificationRequest
        {
            TenantId = tenantId,
            RecipientUserIds = new List<long> { userId },
            Type = type,
            Title = title,
            Message = message
        };
}
