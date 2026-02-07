using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Communication.Announcements.Dto;

public class UpdateAnnouncementDto
{
    [StringLength(200)]
    public string Title { get; set; }

    [StringLength(10000)]
    public string Content { get; set; }

    public AnnouncementType? Type { get; set; }

    public AnnouncementPriority? Priority { get; set; }

    public AnnouncementAudience? TargetAudience { get; set; }

    public Guid? TargetGradeId { get; set; }

    public Guid? TargetClassId { get; set; }

    [StringLength(500)]
    public string AttachmentUrl { get; set; }

    public DateTime? PublishDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool? ClearExpiryDate { get; set; }

    public bool? SendEmailNotification { get; set; }

    public bool? SendPushNotification { get; set; }
}
