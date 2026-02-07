using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.SASpecific.StudentExtramurals.Dto;
using System;
using System.Threading.Tasks;

namespace psms.SASpecific.StudentExtramurals;

/// <summary>
/// Application service interface for managing student extramural enrollments.
/// </summary>
public interface IStudentExtramuralAppService : IApplicationService
{
    Task<StudentExtramuralDto> GetAsync(Guid id);
    Task<PagedResultDto<StudentExtramuralListDto>> GetAllAsync(GetStudentExtramuralsInput input);
    Task<ListResultDto<StudentExtramuralListDto>> GetByActivityAsync(Guid activityId);
    Task<ListResultDto<StudentExtramuralListDto>> GetByStudentAsync(Guid studentId);
    Task<StudentExtramuralDto> CreateAsync(CreateStudentExtramuralDto input);
    Task<StudentExtramuralDto> UpdateAsync(Guid id, UpdateStudentExtramuralDto input);
    Task DeleteAsync(Guid id);
    Task<StudentExtramuralDto> SuspendAsync(Guid id);
    Task<StudentExtramuralDto> ReactivateAsync(Guid id);
    Task<StudentExtramuralDto> TerminateAsync(Guid id);
    Task<StudentExtramuralDto> SignConsentFormAsync(Guid id);
}
