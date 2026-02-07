using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Communication.Notifications.Dto;

public class CreateNotificationDto
{
    [Required]
    public long UserId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [Required]
    [StringLength(1000)]
    public string Message { get; set; }

    [Required]
    public NotificationType Type { get; set; }

    public NotificationPriority Priority { get; set; }

    [StringLength(500)]
    public string ActionUrl { get; set; }

    [StringLength(100)]
    public string EntityType { get; set; }

    public Guid? EntityId { get; set; }
}
