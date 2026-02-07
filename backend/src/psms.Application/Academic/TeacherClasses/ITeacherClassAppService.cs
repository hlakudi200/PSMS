using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.TeacherClasses.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.TeacherClasses;

/// <summary>
/// Application service interface for managing teacher-class assignments.
/// </summary>
public interface ITeacherClassAppService : IApplicationService
{
    Task<ListResultDto<TeacherClassDto>> GetByTeacherAsync(Guid teacherId);
    Task<ListResultDto<TeacherClassDto>> GetByClassAsync(Guid classId);
    Task<TeacherClassDto> AssignAsync(AssignTeacherClassDto input);
    Task UnassignAsync(Guid id);
}
