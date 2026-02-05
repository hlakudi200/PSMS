using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.TeacherSubjects.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.TeacherSubjects;

/// <summary>
/// Application service interface for managing teacher-subject assignments.
/// </summary>
public interface ITeacherSubjectAppService : IApplicationService
{
    Task<ListResultDto<TeacherSubjectDto>> GetByTeacherAsync(Guid teacherId);
    Task<ListResultDto<TeacherSubjectDto>> GetBySubjectAsync(Guid subjectId);
    Task<ListResultDto<TeacherSubjectDto>> GetByGradeAsync(Guid gradeId);
    Task<TeacherSubjectDto> AssignAsync(AssignTeacherSubjectDto input);
    Task UnassignAsync(Guid id);
}
