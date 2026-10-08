using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.Shared;
using psms.Academic.Students.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.Students;

/// <summary>
/// Application service interface for managing students.
/// </summary>
public interface IStudentAppService : IApplicationService
{
    Task<StudentDto> GetAsync(Guid id);
    Task<PagedResultDto<StudentListDto>> GetAllAsync(GetAcademicEntityInput input);
    Task<StudentDto> CreateAsync(CreateStudentDto input);
    Task<StudentDto> UpdateAsync(Guid id, UpdateStudentDto input);
    Task DeleteAsync(Guid id);

    /// <summary>Gets only active students (for dropdowns).</summary>
    Task<ListResultDto<StudentListDto>> GetActiveStudentsAsync();

    /// <summary>Gets students by grade.</summary>
    Task<ListResultDto<StudentListDto>> GetStudentsByGradeAsync(Guid gradeId);

    /// <summary>Gets students by class.</summary>
    Task<ListResultDto<StudentListDto>> GetStudentsByClassAsync(Guid classId);

    /// <summary>Searches students by name or admission number.</summary>
    Task<ListResultDto<StudentListDto>> SearchAsync(string searchTerm);

    /// <summary>Assigns a student to a different class (within same grade).</summary>
    Task<StudentDto> AssignClassAsync(Guid id, Guid classId);

    /// <summary>
    /// Writes the current year's enrolment for every learner who has a class
    /// but no record of being in it. Idempotent, and records this year only.
    /// </summary>
    Task<int> BackfillCurrentYearEnrolmentsAsync();

    Task ActivateAsync(Guid id);
    Task DeactivateAsync(Guid id);
}
