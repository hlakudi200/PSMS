using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Learning.LearningMaterials.Dto;

/// <summary>
/// Input DTO for creating a learning material.
/// </summary>
public class CreateLearningMaterialDto
{
    [Required]
    public Guid ClassSubjectId { get; set; }

    public Guid? TermId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    [Required]
    public LearningMaterialType MaterialType { get; set; }

    [StringLength(256)]
    public string FileName { get; set; }

    [StringLength(500)]
    public string FileUrl { get; set; }

    public long? FileSizeBytes { get; set; }

    [StringLength(100)]
    public string ContentType { get; set; }

    [StringLength(500)]
    public string ExternalLink { get; set; }

    public int DisplayOrder { get; set; }
}
