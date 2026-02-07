using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Learning.LearningMaterials.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Learning.LearningMaterials;

/// <summary>
/// Application service interface for managing learning materials.
/// </summary>
public interface ILearningMaterialAppService : IApplicationService
{
    Task<LearningMaterialDto> GetAsync(Guid id);
    Task<PagedResultDto<LearningMaterialListDto>> GetAllAsync(GetLearningMaterialsInput input);
    Task<ListResultDto<LearningMaterialListDto>> GetByClassSubjectAsync(Guid classSubjectId);
    Task<LearningMaterialDto> CreateAsync(CreateLearningMaterialDto input);
    Task<LearningMaterialDto> UpdateAsync(Guid id, UpdateLearningMaterialDto input);
    Task DeleteAsync(Guid id);
    Task<LearningMaterialDto> PublishAsync(Guid id);
    Task<LearningMaterialDto> UnpublishAsync(Guid id);
    Task IncrementViewCountAsync(Guid id);
}
