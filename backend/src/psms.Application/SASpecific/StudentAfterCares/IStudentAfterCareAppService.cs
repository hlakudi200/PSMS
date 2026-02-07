using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.SASpecific.StudentAfterCares.Dto;
using System;
using System.Threading.Tasks;

namespace psms.SASpecific.StudentAfterCares;

/// <summary>
/// Application service interface for managing student after-care enrollments.
/// </summary>
public interface IStudentAfterCareAppService : IApplicationService
{
    Task<StudentAfterCareDto> GetAsync(Guid id);
    Task<PagedResultDto<StudentAfterCareListDto>> GetAllAsync(GetStudentAfterCaresInput input);
    Task<ListResultDto<StudentAfterCareListDto>> GetByAfterCareAsync(Guid afterCareId);
    Task<ListResultDto<StudentAfterCareListDto>> GetByStudentAsync(Guid studentId);
    Task<StudentAfterCareDto> CreateAsync(CreateStudentAfterCareDto input);
    Task<StudentAfterCareDto> UpdateAsync(Guid id, UpdateStudentAfterCareDto input);
    Task DeleteAsync(Guid id);
    Task<StudentAfterCareDto> SuspendAsync(Guid id);
    Task<StudentAfterCareDto> ReactivateAsync(Guid id);
    Task<StudentAfterCareDto> TerminateAsync(Guid id);
}
