using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Admissions.ApplicationDocuments.Dto;

/// <summary>
/// DTO for application document information.
/// Aligned with ApplicationDocument entity.
/// </summary>
public class ApplicationDocumentDto : CreationAuditedEntityDto<Guid>
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }

    // Document Information
    public DocumentCategory Category { get; set; }
    public string CategoryDisplayName => Category.ToString();
    public string DocumentName { get; set; }
    public string FileName { get; set; }
    public string FileUrl { get; set; }
    public long FileSizeBytes { get; set; }
    public string FileSizeDisplay => FormatFileSize(FileSizeBytes);
    public string ContentType { get; set; }

    // Dates
    public DateTime UploadedDate { get; set; }

    // Verification Status
    public bool IsRequired { get; set; }
    public bool IsVerified { get; set; }
    public long? VerifiedByUserId { get; set; }
    public string VerifiedByUserName { get; set; }
    public DateTime? VerifiedDate { get; set; }

    // Flags
    public bool CanDelete => !IsVerified;

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
