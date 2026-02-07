using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.ClassSubjects.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.ClassSubjects;

/// <summary>
/// Application service interface for managing class-subject assignments.
/// </summary>
public interface IClassSubjectAppService : IApplicationService
{
    Task<ClassSubjectDto> GetAsync(Guid id);
    Task<PagedResultDto<ClassSubjectListDto>> GetAllAsync(GetClassSubjectsInput input);
    Task<ListResultDto<ClassSubjectListDto>> GetByClassAsync(Guid classId);
    Task<ListResultDto<ClassSubjectListDto>> GetByTeacherAsync(Guid teacherId);
    Task<ClassSubjectDto> CreateAsync(CreateClassSubjectDto input);
    Task<ClassSubjectDto> UpdateAsync(Guid id, UpdateClassSubjectDto input);
    Task DeleteAsync(Guid id);
    Task<ClassSubjectDto> AssignTeacherAsync(Guid id, Guid teacherId);
    Task<ClassSubjectDto> RemoveTeacherAsync(Guid id);
}
