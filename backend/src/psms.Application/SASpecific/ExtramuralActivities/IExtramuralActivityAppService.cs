using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.SASpecific.ExtramuralActivities.Dto;
using System;
using System.Threading.Tasks;

namespace psms.SASpecific.ExtramuralActivities;

/// <summary>
/// Application service interface for managing extramural activities.
/// </summary>
public interface IExtramuralActivityAppService : IApplicationService
{
    Task<ExtramuralActivityDto> GetAsync(Guid id);
    Task<PagedResultDto<ExtramuralActivityListDto>> GetAllAsync(GetExtramuralActivitiesInput input);
    Task<ListResultDto<ExtramuralActivityListDto>> GetByAcademicYearAsync(Guid academicYearId);
    Task<ExtramuralActivityDto> CreateAsync(CreateExtramuralActivityDto input);
    Task<ExtramuralActivityDto> UpdateAsync(Guid id, UpdateExtramuralActivityDto input);
    Task DeleteAsync(Guid id);
    Task<ExtramuralActivityDto> ActivateAsync(Guid id);
    Task<ExtramuralActivityDto> DeactivateAsync(Guid id);
    Task<ExtramuralActivityDto> OpenRegistrationAsync(Guid id);
    Task<ExtramuralActivityDto> CloseRegistrationAsync(Guid id);
}
