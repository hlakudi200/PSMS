using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Learning.LearningMaterials.Dto;

/// <summary>
/// Request for a signed upload URL when adding a new learning material.
/// The server validates ownership + file type, then mints a one-time URL
/// the client uploads the bytes to directly.
/// </summary>
public class RequestMaterialUploadUrlDto
{
    [Required]
    public Guid ClassSubjectId { get; set; }

    [Required]
    [StringLength(260)]
    public string FileName { get; set; }

    [Required]
    public LearningMaterialType MaterialType { get; set; }
}

/// <summary>
/// Request for a signed upload URL when adding a new version of an
/// existing material (file type follows the parent material's type).
/// </summary>
public class RequestVersionUploadUrlDto
{
    [Required]
    public Guid LearningMaterialId { get; set; }

    [Required]
    [StringLength(260)]
    public string FileName { get; set; }
}
