using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Communication.Notifications.Dto;

public class NotificationListDto : EntityDto<Guid>
{
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public string ActionUrl { get; set; }

    /// <summary>
    /// #201. What the notification is about, so a client can open it without a
    /// web path. A report card notification has no ActionUrl on purpose (there
    /// is no parent web route for one); it carries "Report" and the report's id.
    /// </summary>
    public string EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
    public DateTime CreationTime { get; set; }
}
