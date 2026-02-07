using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Academic.Grades.Dto;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Academic.Grades;

/// <summary>
/// Service for managing grades. Implements AR-005.
/// </summary>
public interface IGradeAppService : IApplicationService
{
    Task<GradeDto> GetAsync(Guid id);
    Task<PagedResultDto<GradeListDto>> GetAllAsync(PagedAndSortedResultRequestDto input);
    Task<GradeDto> CreateAsync(CreateGradeDto input);
    Task<GradeDto> UpdateAsync(Guid id, UpdateGradeDto input);
    Task DeleteAsync(Guid id);

    /// <summary>Gets only active grades (for dropdowns in Admissions, etc.)</summary>
    Task<ListResultDto<GradeListDto>> GetActiveGradesAsync();

    /// <summary>Filters grades by SA school phase.</summary>
    Task<ListResultDto<GradeListDto>> GetByPhaseAsync(SouthAfricanSchoolPhase phase);

    Task ActivateAsync(Guid id);
    Task DeactivateAsync(Guid id);
}
