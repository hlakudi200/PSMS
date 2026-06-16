using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.ApplicationDocuments.Dto;

/// <summary>
/// Step 1 of the direct document upload (SF-03): the client asks the server
/// for a one-time signed upload URL into the private "documents" bucket. The
/// server validates the application accepts uploads + the file extension
/// before minting the ticket; the bytes are then PUT straight to storage by
/// the browser.
/// </summary>
public class RequestDocumentUploadUrlDto
{
    [Required]
    public Guid ApplicationId { get; set; }

    [Required]
    public DocumentCategory Category { get; set; }

    /// <summary>
    /// Original file name — used for the extension whitelist check and to
    /// preserve the extension on the generated object key.
    /// </summary>
    [Required]
    [StringLength(256)]
    public string FileName { get; set; }
}
