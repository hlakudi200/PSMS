using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.StudentSubjects.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.StudentSubjects;

/// <summary>
/// Application service interface for managing student-subject enrollments.
/// </summary>
public interface IStudentSubjectAppService : IApplicationService
{
    Task<ListResultDto<StudentSubjectDto>> GetByStudentAsync(Guid studentId);
    Task<ListResultDto<StudentSubjectDto>> GetBySubjectAsync(Guid subjectId);
    Task<ListResultDto<StudentSubjectDto>> GetByStudentAndYearAsync(Guid studentId, Guid academicYearId);
    Task<StudentSubjectDto> EnrollAsync(EnrollStudentSubjectDto input);
    Task UnenrollAsync(Guid id);
}
