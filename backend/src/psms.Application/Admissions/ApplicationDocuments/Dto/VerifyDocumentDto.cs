using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.ApplicationDocuments.Dto;

/// <summary>
/// DTO for verifying a document.
/// Implements business rule ADM-010.
/// </summary>
public class VerifyDocumentDto
{
    /// <summary>
    /// Optional notes about the verification.
    /// </summary>
    [StringLength(500)]
    public string Notes { get; set; }
}
