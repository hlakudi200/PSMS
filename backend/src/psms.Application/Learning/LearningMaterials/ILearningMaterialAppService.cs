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

    /// <summary>
    /// Upload a learning material file together with its metadata. Accepts
    /// multipart form data, validates against LM-001 (type whitelist + size
    /// cap), then creates the LearningMaterial entity with a path reference.
    /// File-storage integration is a TODO; this mirrors the existing pattern
    /// in ApplicationDocument/Upload.
    /// </summary>
    Task<LearningMaterialDto> UploadAsync(UploadLearningMaterialDto input);

    Task<LearningMaterialDto> UpdateAsync(Guid id, UpdateLearningMaterialDto input);
    Task DeleteAsync(Guid id);
    Task<LearningMaterialDto> PublishAsync(Guid id);
    Task<LearningMaterialDto> UnpublishAsync(Guid id);
    Task IncrementViewCountAsync(Guid id);
}
