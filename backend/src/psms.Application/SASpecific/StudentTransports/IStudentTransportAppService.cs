using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.SASpecific.StudentTransports.Dto;
using System;
using System.Threading.Tasks;

namespace psms.SASpecific.StudentTransports;

/// <summary>
/// Application service interface for managing student transport enrollments.
/// </summary>
public interface IStudentTransportAppService : IApplicationService
{
    Task<StudentTransportDto> GetAsync(Guid id);
    Task<PagedResultDto<StudentTransportListDto>> GetAllAsync(GetStudentTransportsInput input);
    Task<ListResultDto<StudentTransportListDto>> GetByTransportAsync(Guid transportId);
    Task<ListResultDto<StudentTransportListDto>> GetByStudentAsync(Guid studentId);
    Task<StudentTransportDto> CreateAsync(CreateStudentTransportDto input);
    Task<StudentTransportDto> UpdateAsync(Guid id, UpdateStudentTransportDto input);
    Task DeleteAsync(Guid id);
    Task<StudentTransportDto> SuspendAsync(Guid id);
    Task<StudentTransportDto> ReactivateAsync(Guid id);
    Task<StudentTransportDto> TerminateAsync(Guid id);
}
