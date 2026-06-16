using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Admissions.ApplicationDocuments.Dto;
using psms.Domain.Shared.Storage;
using System;
using System.Threading.Tasks;

namespace psms.Admissions.ApplicationDocuments;

/// <summary>
/// Service for managing application documents.
/// Implements ADM-008 to ADM-010.
/// </summary>
public interface IApplicationDocumentAppService : IApplicationService
{
    /// <summary>
    /// Gets a document by ID.
    /// </summary>
    Task<ApplicationDocumentDto> GetAsync(Guid id);

    /// <summary>
    /// Gets all documents for an application.
    /// </summary>
    Task<ListResultDto<ApplicationDocumentDto>> GetAllByApplicationAsync(Guid applicationId);

    /// <summary>
    /// Gets required documents status for an application.
    /// </summary>
    Task<RequiredDocumentsStatusDto> GetRequiredDocumentsStatusAsync(Guid applicationId);

    /// <summary>
    /// Step 1 of the direct upload: mints a one-time signed URL for the client
    /// to PUT the file straight to the private "documents" bucket.
    /// </summary>
    Task<FileUploadTicket> RequestUploadUrlAsync(RequestDocumentUploadUrlDto input);

    /// <summary>
    /// Records a document whose bytes were already uploaded directly to storage.
    /// </summary>
    Task<ApplicationDocumentDto> UploadAsync(UploadDocumentDto input);

    /// <summary>
    /// Deletes a document (only if not verified).
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Verifies a document.
    /// </summary>
    Task<ApplicationDocumentDto> VerifyAsync(Guid id, VerifyDocumentDto input);

    /// <summary>
    /// Rejects a document with a reason.
    /// </summary>
    Task<ApplicationDocumentDto> RejectAsync(Guid id, string reason);

    /// <summary>
    /// Gets the download URL for a document.
    /// </summary>
    Task<string> GetDownloadUrlAsync(Guid id);
}
