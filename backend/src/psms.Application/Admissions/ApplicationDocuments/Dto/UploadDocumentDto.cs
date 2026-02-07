using Microsoft.AspNetCore.Http;
using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.ApplicationDocuments.Dto;

/// <summary>
/// DTO for uploading a document to an application.
/// Validates business rule ADM-009.
/// </summary>
public class UploadDocumentDto
{
    [Required]
    public Guid ApplicationId { get; set; }

    [Required]
    public DocumentCategory Category { get; set; }

    /// <summary>
    /// The file to upload.
    /// Allowed formats: PDF, JPG, JPEG, PNG (ADM-009).
    /// Maximum size: 10MB (ADM-009).
    /// </summary>
    [Required]
    public IFormFile File { get; set; }

    /// <summary>
    /// Optional description of the document.
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; }
}
