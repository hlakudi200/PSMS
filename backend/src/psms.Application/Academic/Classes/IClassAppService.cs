using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.Classes.Dto;
using psms.Academic.Shared;
using System;
using System.Threading.Tasks;

namespace psms.Academic.Classes;

/// <summary>
/// Application service interface for managing classes.
/// </summary>
public interface IClassAppService : IApplicationService
{
    Task<ClassDto> GetAsync(Guid id);
    Task<PagedResultDto<ClassListDto>> GetAllAsync(GetAcademicEntityInput input);
    Task<ClassDto> CreateAsync(CreateClassDto input);
    Task<ClassDto> UpdateAsync(Guid id, UpdateClassDto input);
    Task DeleteAsync(Guid id);

    /// <summary>Gets only active classes (for dropdowns).</summary>
    Task<ListResultDto<ClassListDto>> GetActiveClassesAsync();

    /// <summary>Gets classes for a specific grade.</summary>
    Task<ListResultDto<ClassListDto>> GetClassesByGradeAsync(Guid gradeId);

    /// <summary>Gets classes for a specific academic year.</summary>
    Task<ListResultDto<ClassListDto>> GetClassesByAcademicYearAsync(Guid academicYearId);

    /// <summary>Assigns a class teacher (form teacher) to a class.</summary>
    Task<ClassDto> AssignClassTeacherAsync(Guid id, Guid teacherId);

    /// <summary>Removes the class teacher from a class.</summary>
    Task<ClassDto> RemoveClassTeacherAsync(Guid id);

    Task ActivateAsync(Guid id);
    Task DeactivateAsync(Guid id);
}
