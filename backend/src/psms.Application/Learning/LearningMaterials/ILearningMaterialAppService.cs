using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Domain.Shared.Storage;
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
    /// Step 1 of a direct upload: validates ownership + file type and returns
    /// a one-time signed URL the client PUTs the bytes to (bytes bypass the
    /// server). Follow with <see cref="UploadAsync"/>.
    /// </summary>
    Task<FileUploadTicket> RequestUploadUrlAsync(RequestMaterialUploadUrlDto input);

    /// <summary>Step 1 of a direct upload for a new version of a material.</summary>
    Task<FileUploadTicket> RequestVersionUploadUrlAsync(RequestVersionUploadUrlDto input);

    /// <summary>
    /// Step 2: records a learning material whose file was already uploaded to
    /// storage (via the ticket from RequestUploadUrl). Validates LM-001 from
    /// the supplied metadata and stores the public URL.
    /// </summary>
    Task<LearningMaterialDto> UploadAsync(UploadLearningMaterialDto input);

    Task<LearningMaterialDto> UpdateAsync(Guid id, UpdateLearningMaterialDto input);
    Task DeleteAsync(Guid id);
    Task<LearningMaterialDto> PublishAsync(Guid id);
    Task<LearningMaterialDto> UnpublishAsync(Guid id);
    Task IncrementViewCountAsync(Guid id);

    /// <summary>
    /// Short-lived signed URL to stream a video material (view-only; video
    /// files are kept in a private bucket).
    /// </summary>
    Task<string> GetVideoUrlAsync(Guid id);

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
