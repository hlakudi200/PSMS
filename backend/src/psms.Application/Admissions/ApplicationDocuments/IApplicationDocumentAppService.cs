using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Admissions.ApplicationDocuments.Dto;
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
    /// Uploads a document to an application.
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
