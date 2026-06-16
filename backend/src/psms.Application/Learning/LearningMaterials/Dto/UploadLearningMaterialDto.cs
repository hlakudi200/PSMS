using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Learning.LearningMaterials.Dto;

/// <summary>
/// DTO that records a learning material after its file has been uploaded
/// directly to storage (the client first calls RequestUploadUrl, PUTs the
/// bytes to the returned signed URL, then posts this metadata). The file
/// bytes never pass through the server. File validation follows LM-001
/// (type whitelist + size cap) using the supplied metadata.
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
    /// Public URL of the already-uploaded file (from the upload ticket).
    /// Required when MaterialType is not ExternalLink.
    /// </summary>
    [StringLength(1000)]
    public string FileUrl { get; set; }

    /// <summary>Original file name (used for the type/extension check).</summary>
    [StringLength(260)]
    public string FileName { get; set; }

    /// <summary>Client-reported size in bytes (re-checked against LM-001).</summary>
    public long FileSizeBytes { get; set; }

    [StringLength(150)]
    public string ContentType { get; set; }

    /// <summary>
    /// Required when MaterialType is ExternalLink, otherwise optional.
    /// </summary>
    [StringLength(500)]
    public string ExternalLink { get; set; }

    public int DisplayOrder { get; set; }
}
