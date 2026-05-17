using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.Shared;
using psms.Academic.Teachers.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.Teachers;

/// <summary>
/// Service for managing teachers.
/// </summary>
public interface ITeacherAppService : IApplicationService
{
    Task<TeacherDto> GetAsync(Guid id);
    Task<PagedResultDto<TeacherListDto>> GetAllAsync(GetAcademicEntityInput input);
    Task<TeacherDto> CreateAsync(CreateTeacherDto input);
    Task<TeacherDto> UpdateAsync(Guid id, UpdateTeacherDto input);
    Task DeleteAsync(Guid id);

    /// <summary>Gets only active teachers (for dropdowns).</summary>
    Task<ListResultDto<TeacherListDto>> GetActiveTeachersAsync();

    /// <summary>
    /// Returns the Teacher record linked to the current ABP session user,
    /// or null if no teacher profile is linked. Returns null rather than
    /// throwing so callers can render a "no teacher profile" hint without
    /// going through the global error modal.
    /// </summary>
    Task<TeacherDto> GetByCurrentUserAsync();

    Task ActivateAsync(Guid id);
    Task DeactivateAsync(Guid id);
}
