using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.Parents.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.Parents;

/// <summary>
/// Service for managing parents/guardians.
/// </summary>
public interface IParentAppService : IApplicationService
{
    Task<ParentDto> GetAsync(Guid id);
    Task<PagedResultDto<ParentListDto>> GetAllAsync(PagedAndSortedResultRequestDto input);
    Task<ParentDto> CreateAsync(CreateParentDto input);
    Task<ParentDto> UpdateAsync(Guid id, UpdateParentDto input);
    Task DeleteAsync(Guid id);

    /// <summary>Search parents by name or email.</summary>
    Task<ListResultDto<ParentListDto>> SearchAsync(string searchTerm);
}
