using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Financial.FeeStructures.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Financial.FeeStructures;

/// <summary>
/// Application service interface for managing fee structures.
/// </summary>
public interface IFeeStructureAppService : IApplicationService
{
    Task<FeeStructureDto> GetAsync(Guid id);
    Task<PagedResultDto<FeeStructureListDto>> GetAllAsync(GetFeeStructuresInput input);
    Task<ListResultDto<FeeStructureListDto>> GetByGradeAndYearAsync(Guid gradeId, Guid academicYearId);
    Task<FeeStructureDto> CreateAsync(CreateFeeStructureDto input);
    Task<FeeStructureDto> UpdateAsync(Guid id, UpdateFeeStructureDto input);
    Task DeleteAsync(Guid id);
    Task<FeeStructureDto> ActivateAsync(Guid id);
    Task<FeeStructureDto> DeactivateAsync(Guid id);
}
