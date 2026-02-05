using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.Subjects.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.Subjects;

/// <summary>
/// Service for managing academic subjects.
/// </summary>
public interface ISubjectAppService : IApplicationService
{
    Task<SubjectDto> GetAsync(Guid id);
    Task<PagedResultDto<SubjectListDto>> GetAllAsync(PagedAndSortedResultRequestDto input);
    Task<SubjectDto> CreateAsync(CreateSubjectDto input);
    Task<SubjectDto> UpdateAsync(Guid id, UpdateSubjectDto input);
    Task DeleteAsync(Guid id);

    /// <summary>Gets only active subjects (for dropdowns).</summary>
    Task<ListResultDto<SubjectListDto>> GetActiveSubjectsAsync();

    /// <summary>Gets only core (required) subjects.</summary>
    Task<ListResultDto<SubjectListDto>> GetCoreSubjectsAsync();

    Task ActivateAsync(Guid id);
    Task DeactivateAsync(Guid id);
}
