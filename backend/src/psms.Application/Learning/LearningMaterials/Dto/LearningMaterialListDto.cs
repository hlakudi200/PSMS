using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Learning.LearningMaterials.Dto;

/// <summary>
/// Lightweight DTO for learning material lists.
/// </summary>
public class LearningMaterialListDto : EntityDto<Guid>
{
    public Guid ClassSubjectId { get; set; }
    public string Title { get; set; }
    public LearningMaterialType MaterialType { get; set; }
    public bool IsPublished { get; set; }
    public int DisplayOrder { get; set; }
    public int ViewCount { get; set; }
    public string FileName { get; set; }
    /// <summary>
    /// File size in bytes; null for ExternalLink-only materials. Used by
    /// the teacher portal's storage-quota approximation per LM-007.
    /// </summary>
    public long? FileSizeBytes { get; set; }
    /// <summary>
    /// Public URL of the current file version; null for ExternalLink-only
    /// materials. Lets list views offer a "View" action without a second
    /// round trip to GetAsync.
    /// </summary>
    public string FileUrl { get; set; }
    public string ExternalLink { get; set; }
}
