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
    /// Storage object key returned by RequestUploadUrl (the file the client
    /// already PUT to storage). The server validates it belongs to this
    /// tenant/class-subject, reads the real size/type from storage, and
    /// derives the public URL — none of these are trusted from the client.
    /// Required when MaterialType is not ExternalLink.
    /// </summary>
    [StringLength(500)]
    public string ObjectKey { get; set; }

    /// <summary>Original file name (used for the display name + extension check).</summary>
    [StringLength(260)]
    public string FileName { get; set; }

    /// <summary>
    /// Required when MaterialType is ExternalLink, otherwise optional.
    /// </summary>
    [StringLength(500)]
    public string ExternalLink { get; set; }

    public int DisplayOrder { get; set; }
}
