using psms.Domain.Shared.Enums;
using System;

namespace psms.Learning.LearningMaterials.Dto;

/// <summary>
/// Input DTO for updating a learning material. All fields nullable for partial updates.
/// </summary>
public class UpdateLearningMaterialDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public LearningMaterialType? MaterialType { get; set; }
    public Guid? TermId { get; set; }
    public string FileName { get; set; }
    public string FileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string ContentType { get; set; }
    public string ExternalLink { get; set; }
    public int? DisplayOrder { get; set; }
}
