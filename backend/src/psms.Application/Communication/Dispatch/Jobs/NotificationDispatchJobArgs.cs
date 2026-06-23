using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;

namespace psms.Communication.Dispatch.Jobs;

/// <summary>
/// COMM-11: serializable mirror of NotificationRequest for the background dispatch
/// job (the job runs without a session, so TenantId travels in the args).
/// </summary>
[Serializable]
public class NotificationDispatchJobArgs
{
    public int? TenantId { get; set; }
    public List<long> RecipientUserIds { get; set; } = new List<long>();
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string ActionUrl { get; set; }
    public string EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public string IdempotencyKey { get; set; }
    public string TemplateKey { get; set; }
    public string Language { get; set; }
    public Dictionary<string, string> Variables { get; set; }
    public List<NotificationChannel> RequestedChannels { get; set; }
}
