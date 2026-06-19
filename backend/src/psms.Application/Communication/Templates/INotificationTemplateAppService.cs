using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Communication.Templates.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Communication.Templates;

/// <summary>COMM-06: admin authoring/management of notification templates.</summary>
public interface INotificationTemplateAppService : IApplicationService
{
    Task<PagedResultDto<NotificationTemplateListDto>> GetAllAsync(GetTemplatesInput input);
    Task<NotificationTemplateDto> GetAsync(Guid id);
    Task<NotificationTemplateDto> CreateAsync(CreateNotificationTemplateDto input);
    Task<NotificationTemplateDto> UpdateAsync(Guid id, UpdateNotificationTemplateDto input);
    Task DeleteAsync(Guid id);
    Task<NotificationTemplateDto> SetApprovalStatusAsync(Guid id, SetTemplateApprovalDto input);
}
