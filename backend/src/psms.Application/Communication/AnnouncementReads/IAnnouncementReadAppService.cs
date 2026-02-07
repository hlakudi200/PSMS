using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Communication.AnnouncementReads.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Communication.AnnouncementReads;

public interface IAnnouncementReadAppService : IApplicationService
{
    Task MarkAsReadAsync(Guid announcementId);
    Task<ListResultDto<AnnouncementReadDto>> GetByAnnouncementAsync(Guid announcementId);
    Task<int> GetUnreadCountAsync();
}
