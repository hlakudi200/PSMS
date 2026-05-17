using Microsoft.AspNetCore.Http;
using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Learning.LearningMaterials.Dto;

/// <summary>
/// DTO for uploading a learning material file together with its metadata.
/// File validation follows business rule LM-001:
///  - documents (PDF/DOC/DOCX/PPT/PPTX/XLS/XLSX/TXT/ZIP) and images: ≤ 50 MB
///  - audio: ≤ 50 MB
///  - video (MP4/MOV/AVI/WEBM): ≤ 500 MB
/// </summary>
public class UploadLearningMaterialDto
{
    [Required]
    public Guid ClassSubjectId { get; set; }

    public Guid? TermId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string Description { get; set; }

    [Required]
    public LearningMaterialType MaterialType { get; set; }

    /// <summary>
    /// The uploaded file. Required when MaterialType is not ExternalLink.
    /// Validated against LM-001 (type whitelist + size cap).
    /// </summary>
    public IFormFile File { get; set; }

    /// <summary>
    /// Required when MaterialType is ExternalLink, otherwise optional
    /// (allows attaching a supporting link alongside an uploaded file).
    /// </summary>
    [StringLength(500)]
    public string ExternalLink { get; set; }

    public int DisplayOrder { get; set; }
}
