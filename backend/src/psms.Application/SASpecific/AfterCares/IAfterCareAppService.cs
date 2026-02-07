using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.SASpecific.AfterCares.Dto;
using System;
using System.Threading.Tasks;

namespace psms.SASpecific.AfterCares;

/// <summary>
/// Application service interface for managing after-care programs.
/// </summary>
public interface IAfterCareAppService : IApplicationService
{
    Task<AfterCareDto> GetAsync(Guid id);
    Task<PagedResultDto<AfterCareListDto>> GetAllAsync(GetAfterCaresInput input);
    Task<ListResultDto<AfterCareListDto>> GetByAcademicYearAsync(Guid academicYearId);
    Task<AfterCareDto> CreateAsync(CreateAfterCareDto input);
    Task<AfterCareDto> UpdateAsync(Guid id, UpdateAfterCareDto input);
    Task DeleteAsync(Guid id);
    Task<AfterCareDto> ActivateAsync(Guid id);
    Task<AfterCareDto> DeactivateAsync(Guid id);
}
