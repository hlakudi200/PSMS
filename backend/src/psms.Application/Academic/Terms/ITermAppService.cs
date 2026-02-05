using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.Terms.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.Terms;

/// <summary>
/// Service for managing terms. Implements AR-003, AR-004.
/// </summary>
public interface ITermAppService : IApplicationService
{
    Task<TermDto> GetAsync(Guid id);
    Task<ListResultDto<TermListDto>> GetByAcademicYearAsync(Guid academicYearId);
    Task<TermDto> CreateAsync(CreateTermDto input);
    Task<TermDto> UpdateAsync(Guid id, UpdateTermDto input);
    Task DeleteAsync(Guid id);

    /// <summary>Gets the current term.</summary>
    Task<TermDto> GetCurrentAsync();

    /// <summary>Sets the specified term as current (AR-004).</summary>
    Task<TermDto> SetAsCurrentAsync(Guid id);
}
