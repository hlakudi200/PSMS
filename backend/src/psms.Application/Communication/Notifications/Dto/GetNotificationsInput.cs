using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;

namespace psms.Communication.Notifications.Dto;

public class GetNotificationsInput : PagedAndSortedResultRequestDto
{
    public NotificationType? Type { get; set; }
    public NotificationPriority? Priority { get; set; }
    public bool? IsRead { get; set; }
    public string Search { get; set; }
    public string Keyword { get; set; }
}
