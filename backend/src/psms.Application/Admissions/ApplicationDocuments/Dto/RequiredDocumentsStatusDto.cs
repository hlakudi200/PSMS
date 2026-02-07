using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;

namespace psms.Admissions.ApplicationDocuments.Dto;

/// <summary>
/// DTO showing required documents status for an application.
/// Based on business rule ADM-008.
/// </summary>
public class RequiredDocumentsStatusDto
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }
    public string GradeName { get; set; }
    public bool IsSACitizen { get; set; }

    // Summary
    public int TotalRequired { get; set; }
    public int TotalUploaded { get; set; }
    public int TotalVerified { get; set; }
    public bool AllRequiredUploaded => TotalUploaded >= TotalRequired;
    public bool AllRequiredVerified => TotalVerified >= TotalRequired;

    // Document Details
    public List<RequiredDocumentItemDto> Documents { get; set; } = new();
}

/// <summary>
/// Status of a specific required document category.
/// </summary>
public class RequiredDocumentItemDto
{
    public DocumentCategory Category { get; set; }
    public string CategoryDisplayName => Category.ToString();
    public string Description { get; set; }
    public bool IsRequired { get; set; }
    public bool IsUploaded { get; set; }
    public bool IsVerified { get; set; }
    public Guid? DocumentId { get; set; }
    public string FileName { get; set; }
    public DateTime? UploadedDate { get; set; }
}
