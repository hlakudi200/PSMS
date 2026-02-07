using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.AcademicYears.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.AcademicYears;

/// <summary>
/// Service for managing academic years. Implements AR-001, AR-002, AR-003.
/// </summary>
public interface IAcademicYearAppService : IApplicationService
{
    Task<AcademicYearDto> GetAsync(Guid id);
    Task<PagedResultDto<AcademicYearListDto>> GetAllAsync(PagedAndSortedResultRequestDto input);
    Task<AcademicYearDto> CreateAsync(CreateAcademicYearDto input);
    Task<AcademicYearDto> UpdateAsync(Guid id, UpdateAcademicYearDto input);
    Task DeleteAsync(Guid id);

    /// <summary>Gets the current academic year for this tenant.</summary>
    Task<AcademicYearDto> GetCurrentAsync();

    /// <summary>Sets the specified academic year as current (AR-001).</summary>
    Task<AcademicYearDto> SetAsCurrentAsync(Guid id);
}
