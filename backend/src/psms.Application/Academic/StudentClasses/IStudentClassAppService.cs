using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.StudentClasses.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.StudentClasses;

public interface IStudentClassAppService : IApplicationService
{
    Task<StudentClassDto> GetAsync(Guid id);
    Task<ListResultDto<StudentClassListDto>> GetByStudentAsync(Guid studentId);
    Task<ListResultDto<StudentClassListDto>> GetByClassAsync(Guid classId);
    Task<StudentClassDto> GetCurrentByStudentAsync(Guid studentId);

    /// <summary>Enrolls a student in a class for an academic year.</summary>
    Task<StudentClassDto> EnrollAsync(CreateStudentClassDto input);
    Task<StudentClassDto> UpdateAsync(Guid id, UpdateStudentClassDto input);

    /// <summary>Ends the student's enrollment in a class.</summary>
    Task<StudentClassDto> EndEnrollmentAsync(Guid id, DateTime endDate);

    /// <summary>Sets a student class record as the current one (unsets others).</summary>
    Task<StudentClassDto> SetAsCurrentAsync(Guid id);

    Task DeleteAsync(Guid id);
}
