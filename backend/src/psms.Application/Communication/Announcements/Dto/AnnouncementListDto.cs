using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Communication.Announcements.Dto;

public class AnnouncementListDto : EntityDto<Guid>
{
    public string Title { get; set; }
    public AnnouncementType Type { get; set; }
    public AnnouncementPriority Priority { get; set; }
    public AnnouncementAudience TargetAudience { get; set; }
    public DateTime PublishDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsPublished { get; set; }
    public bool IsPinned { get; set; }
    public long CreatedByUserId { get; set; }
    public int ReadCount { get; set; }
}
