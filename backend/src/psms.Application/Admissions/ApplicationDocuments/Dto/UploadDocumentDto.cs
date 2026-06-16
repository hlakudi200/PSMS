using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.ApplicationDocuments.Dto;

/// <summary>
/// Posted after the document's bytes have been uploaded directly to the
/// private "documents" bucket via a ticket from RequestUploadUrl (the bytes
/// never pass through this server — see SF-03). The client supplies only the
/// object key; the server validates it, HEADs the real size/type, and keeps
/// the key. Validates ADM-009.
/// </summary>
public class UploadDocumentDto
{
    [Required]
    public Guid ApplicationId { get; set; }

    [Required]
    public DocumentCategory Category { get; set; }

    /// <summary>
    /// Storage object key from RequestUploadUrl. Validated + measured
    /// server-side (PDF/JPG/JPEG/PNG, 10 MB cap — ADM-009).
    /// </summary>
    [Required]
    [StringLength(500)]
    public string ObjectKey { get; set; }

    /// <summary>Original file name (used for the extension check + display).</summary>
    [Required]
    [StringLength(256)]
    public string FileName { get; set; }

    /// <summary>
    /// Optional description of the document.
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; }
}
