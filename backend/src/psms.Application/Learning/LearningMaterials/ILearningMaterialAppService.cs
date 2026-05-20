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

    /// <summary>
    /// Lists the version history for a single learning material, newest
    /// first. Used by the teacher portal's version-history drawer.
    /// </summary>
    Task<ListResultDto<LearningMaterialVersionDto>> GetVersionsAsync(Guid learningMaterialId);

    /// <summary>
    /// Upload a new version of an existing material. Creates a new
    /// LearningMaterialVersion row (next sequential VersionNumber), then
    /// replaces the file pointer on the parent material so the "current"
    /// file is always the latest version. Retains up to 10 versions
    /// per LM-003 — older versions beyond that are pruned.
    /// </summary>
    Task<LearningMaterialDto> UploadNewVersionAsync(UploadNewVersionDto input);

    /// <summary>
    /// Restore the parent material's current file pointer to an older
    /// version by COPYING that version's data into a new
    /// LearningMaterialVersion row — history is never overwritten in
    /// place.
    /// </summary>
    Task<LearningMaterialDto> RestoreVersionAsync(Guid learningMaterialId, Guid versionId);
}
