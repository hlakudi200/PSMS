using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Communication.Notifications.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Communication.Notifications;

public interface INotificationAppService : IApplicationService
{
    Task<NotificationDto> GetAsync(Guid id);
    Task<PagedResultDto<NotificationListDto>> GetAllAsync(GetNotificationsInput input);
    Task<int> GetUnreadCountAsync();
    Task MarkAsReadAsync(Guid id);
    Task MarkAllAsReadAsync();
    Task<NotificationDto> CreateAsync(CreateNotificationDto input);
}
