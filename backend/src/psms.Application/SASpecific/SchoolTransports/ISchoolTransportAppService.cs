using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.SASpecific.SchoolTransports.Dto;
using System;
using System.Threading.Tasks;

namespace psms.SASpecific.SchoolTransports;

/// <summary>
/// Application service interface for managing school transport routes.
/// </summary>
public interface ISchoolTransportAppService : IApplicationService
{
    Task<SchoolTransportDto> GetAsync(Guid id);
    Task<PagedResultDto<SchoolTransportListDto>> GetAllAsync(GetSchoolTransportsInput input);
    Task<SchoolTransportDto> CreateAsync(CreateSchoolTransportDto input);
    Task<SchoolTransportDto> UpdateAsync(Guid id, UpdateSchoolTransportDto input);
    Task DeleteAsync(Guid id);
    Task<SchoolTransportDto> ActivateAsync(Guid id);
    Task<SchoolTransportDto> DeactivateAsync(Guid id);
}
