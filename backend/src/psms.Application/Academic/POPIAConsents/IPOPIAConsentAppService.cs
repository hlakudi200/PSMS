using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.POPIAConsents.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.POPIAConsents;

public interface IPOPIAConsentAppService : IApplicationService
{
    Task<POPIAConsentDto> GetByStudentAsync(Guid studentId);
    Task<PagedResultDto<POPIAConsentDto>> GetAllAsync(GetPOPIAConsentsInput input);
    Task<POPIAConsentDto> CreateAsync(CreatePOPIAConsentDto input);
    Task<POPIAConsentDto> UpdateAsync(Guid id, UpdatePOPIAConsentDto input);

    /// <summary>Revokes consent via soft delete.</summary>
    Task RevokeAsync(Guid id);
}
