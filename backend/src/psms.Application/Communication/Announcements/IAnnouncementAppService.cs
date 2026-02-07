using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Communication.Announcements.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Communication.Announcements;

public interface IAnnouncementAppService : IApplicationService
{
    Task<AnnouncementDto> GetAsync(Guid id);
    Task<PagedResultDto<AnnouncementListDto>> GetAllAsync(GetAnnouncementsInput input);
    Task<AnnouncementDto> CreateAsync(CreateAnnouncementDto input);
    Task<AnnouncementDto> UpdateAsync(Guid id, UpdateAnnouncementDto input);
    Task DeleteAsync(Guid id);
    Task<AnnouncementDto> PublishAsync(Guid id);
    Task<AnnouncementDto> UnpublishAsync(Guid id);
    Task<AnnouncementDto> PinAsync(Guid id);
    Task<AnnouncementDto> UnpinAsync(Guid id);
}
