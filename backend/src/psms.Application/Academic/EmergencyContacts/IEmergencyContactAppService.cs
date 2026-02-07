using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.EmergencyContacts.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.EmergencyContacts;

public interface IEmergencyContactAppService : IApplicationService
{
    Task<EmergencyContactDto> GetAsync(Guid id);
    Task<ListResultDto<EmergencyContactListDto>> GetByStudentAsync(Guid studentId);
    Task<EmergencyContactDto> CreateAsync(CreateEmergencyContactDto input);
    Task<EmergencyContactDto> UpdateAsync(Guid id, UpdateEmergencyContactDto input);
    Task DeleteAsync(Guid id);
}
