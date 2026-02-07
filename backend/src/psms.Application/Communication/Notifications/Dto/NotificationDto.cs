using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Communication.Notifications.Dto;

public class NotificationDto : CreationAuditedEntityDto<Guid>
{
    public long UserId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public string ActionUrl { get; set; }
    public string EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
    public bool EmailSent { get; set; }
    public DateTime? EmailSentDate { get; set; }
    public bool PushSent { get; set; }
    public DateTime? PushSentDate { get; set; }
}
