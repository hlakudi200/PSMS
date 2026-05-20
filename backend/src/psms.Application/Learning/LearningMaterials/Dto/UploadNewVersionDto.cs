using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Learning.LearningMaterials.Dto;

/// <summary>
/// Input DTO for "Upload new version" of an existing learning material.
/// </summary>
public class UploadNewVersionDto
{
    [Required]
    public Guid LearningMaterialId { get; set; }

    /// <summary>
    /// Required teacher-supplied note describing what changed in this
    /// version (LM-003). Backed by the same character bounds as the
    /// matching column on the LearningMaterialVersion entity.
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string ChangeDescription { get; set; }

    /// <summary>
    /// New file. File-type / size constraints follow the parent material's
    /// type and are validated by the same private ValidateFile helper used
    /// by UploadAsync.
    /// </summary>
    [Required]
    public IFormFile File { get; set; }
}
