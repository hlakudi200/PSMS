using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Learning.LearningMaterials.Dto;

/// <summary>
/// Full DTO for a learning material.
/// </summary>
public class LearningMaterialDto : FullAuditedEntityDto<Guid>
{
    public Guid ClassSubjectId { get; set; }
    public Guid? TermId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public LearningMaterialType MaterialType { get; set; }
    public string FileName { get; set; }
    public string FileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string ContentType { get; set; }
    public string ExternalLink { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedDate { get; set; }
    public long UploadedByUserId { get; set; }
    public int ViewCount { get; set; }

    // Flattened from ClassSubject
    public string ClassName { get; set; }
    public string SubjectName { get; set; }

    // Flattened from Term
    public string TermName { get; set; }
}
