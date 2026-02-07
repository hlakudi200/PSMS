using Abp.Application.Services;
using psms.Academic.MedicalInfos.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.MedicalInfos;

public interface IMedicalInfoAppService : IApplicationService
{
    Task<MedicalInfoDto> GetByStudentAsync(Guid studentId);
    Task<MedicalInfoDto> CreateOrUpdateAsync(CreateUpdateMedicalInfoDto input);
    Task DeleteAsync(Guid studentId);
}
