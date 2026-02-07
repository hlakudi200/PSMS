using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Communication.Documents.Dto;

public class CreateDocumentDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [Required]
    [StringLength(256)]
    public string FileName { get; set; }

    [Required]
    [StringLength(500)]
    public string FileUrl { get; set; }

    [Required]
    public long FileSizeBytes { get; set; }

    [StringLength(100)]
    public string ContentType { get; set; }

    [StringLength(50)]
    public string Category { get; set; }

    [Required]
    public SharedDocumentType DocumentType { get; set; }

    [Required]
    public DocumentAudience TargetAudience { get; set; }

    public Guid? AcademicYearId { get; set; }
}
