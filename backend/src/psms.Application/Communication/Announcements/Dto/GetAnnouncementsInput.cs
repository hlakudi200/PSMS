using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;

namespace psms.Communication.Announcements.Dto;

public class GetAnnouncementsInput : PagedAndSortedResultRequestDto
{
    public AnnouncementType? Type { get; set; }
    public AnnouncementPriority? Priority { get; set; }
    public AnnouncementAudience? TargetAudience { get; set; }
    public bool? IsPublished { get; set; }
    public bool? IsPinned { get; set; }
    public string Search { get; set; }
    public string Keyword { get; set; }
}
