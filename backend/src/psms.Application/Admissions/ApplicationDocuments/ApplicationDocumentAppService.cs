using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.ApplicationDocuments.Dto;
using psms.Admissions.Shared;
using psms.Authorization;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Shared.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Admissions.ApplicationDocuments;

/// <summary>
/// Service for managing application documents.
/// Implements ADM-008 to ADM-010.
/// </summary>
[AbpAuthorize(PermissionNames.Admissions_Documents)]
public class ApplicationDocumentAppService : ApplicationService, IApplicationDocumentAppService
{
    private readonly IRepository<ApplicationDocument, Guid> _documentRepository;
    private readonly IRepository<Application, Guid> _applicationRepository;
    private readonly IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> _settingsRepository;
    private readonly IFileStorageService _fileStorage;

    // Allowed file types (ADM-009)
    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
    private static readonly string[] AllowedContentTypes = {
        "application/pdf",
        "image/jpeg",
        "image/png"
    };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    // PRIVATE Supabase bucket for admission documents (IDs, birth certificates
    // — sensitive). Unlike materials/recordings this bucket is NOT public:
    // downloads go through short-lived signed URLs (GetDownloadUrlAsync).
    private const string DocumentsBucket = "documents";

    // Signed download URLs are valid for 5 minutes — long enough to open/stream
    // the file, short enough that a leaked URL is quickly useless.
    private const int DownloadUrlExpirySeconds = 300;

    public ApplicationDocumentAppService(
        IRepository<ApplicationDocument, Guid> documentRepository,
        IRepository<Application, Guid> applicationRepository,
        IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> settingsRepository,
        IFileStorageService fileStorage)
    {
        _documentRepository = documentRepository;
        _applicationRepository = applicationRepository;
        _settingsRepository = settingsRepository;
        _fileStorage = fileStorage;
    }

    [AbpAuthorize(PermissionNames.Admissions_Documents_View)]
    public async Task<ApplicationDocumentDto> GetAsync(Guid id)
    {
        var document = await _documentRepository
            .GetAll()
            .Include(d => d.Application)
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

        if (document == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.DocumentNotFound, "Document not found.");

        return ObjectMapper.Map<ApplicationDocumentDto>(document);
    }

    [AbpAuthorize(PermissionNames.Admissions_Documents_View)]
    public async Task<ListResultDto<ApplicationDocumentDto>> GetAllByApplicationAsync(Guid applicationId)
    {
        var documents = await _documentRepository
            .GetAll()
            .Include(d => d.Application)
            .Where(d => d.ApplicationId == applicationId)
            .OrderBy(d => d.Category)
            .ThenByDescending(d => d.UploadedDate)
            .ToListAsync();

        return new ListResultDto<ApplicationDocumentDto>(
            ObjectMapper.Map<List<ApplicationDocumentDto>>(documents));
    }

    [AbpAuthorize(PermissionNames.Admissions_Documents_View)]
    public async Task<RequiredDocumentsStatusDto> GetRequiredDocumentsStatusAsync(Guid applicationId)
    {
        var application = await _applicationRepository
            .GetAll()
            .Include(a => a.AppliedGrade)
            .Include(a => a.ApplicationDocuments)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound, "Application not found.");

        // Get required documents from settings
        var settings = await GetAdmissionSettingsAsync(application.AcademicYearId, application.AppliedGradeId);

        var requiredCategories = GetRequiredDocumentCategories(application.IsSACitizen, application.AppliedGrade?.GradeName);

        var status = new RequiredDocumentsStatusDto
        {
            ApplicationId = applicationId,
            ApplicationNumber = application.ApplicationNumber,
            GradeName = application.AppliedGrade?.GradeName,
            IsSACitizen = application.IsSACitizen,
            TotalRequired = requiredCategories.Count,
            Documents = new List<RequiredDocumentItemDto>()
        };

        foreach (var category in requiredCategories)
        {
            var uploadedDoc = application.ApplicationDocuments?
                .FirstOrDefault(d => d.Category == category.Category);

            status.Documents.Add(new RequiredDocumentItemDto
            {
                Category = category.Category,
                Description = category.Description,
                IsRequired = category.IsRequired,
                IsUploaded = uploadedDoc != null,
                IsVerified = uploadedDoc?.IsVerified ?? false,
                DocumentId = uploadedDoc?.Id,
                FileName = uploadedDoc?.FileName,
                UploadedDate = uploadedDoc?.UploadedDate
            });
        }

        status.TotalUploaded = status.Documents.Count(d => d.IsUploaded);
        status.TotalVerified = status.Documents.Count(d => d.IsVerified);

        return status;
    }

    /// <summary>
    /// Step 1 of the direct upload (SF-03): validate the application accepts
    /// uploads + the file extension, then mint a one-time signed URL the client
    /// PUTs the bytes to (bytes never pass through this server). The file lands
    /// in the PRIVATE "documents" bucket; the client then calls UploadAsync with
    /// the resulting object key.
    /// </summary>
    [AbpAuthorize(PermissionNames.Admissions_Documents_Upload)]
    public async Task<FileUploadTicket> RequestUploadUrlAsync(RequestDocumentUploadUrlDto input)
    {
        var application = await _applicationRepository.GetAsync(input.ApplicationId);
        EnsureApplicationAcceptsUploads(application);
        ValidateDocumentExtension(input.FileName);

        var key = BuildDocumentObjectKey(input.ApplicationId, input.FileName);
        return await _fileStorage.CreateUploadTicketAsync(DocumentsBucket, key);
    }

    [AbpAuthorize(PermissionNames.Admissions_Documents_Upload)]
    public async Task<ApplicationDocumentDto> UploadAsync(UploadDocumentDto input)
    {
        var application = await _applicationRepository.GetAsync(input.ApplicationId);
        EnsureApplicationAcceptsUploads(application);

        // The bytes were PUT directly to the private bucket via the
        // RequestUploadUrl ticket. Validate the key belongs to this tenant +
        // application, confirm the file exists, enforce ADM-009 against the REAL
        // stored size/type, and keep the object key (not a public URL — the
        // bucket is private; downloads are signed on demand).
        var (objectKey, size, contentType) =
            await ResolveUploadedDocumentAsync(input.ObjectKey, input.ApplicationId, input.FileName);

        var document = new ApplicationDocument(
            Guid.NewGuid(),
            input.ApplicationId,
            input.Category,
            $"{input.Category}_{application.ApplicationNumber}",
            input.FileName,
            objectKey,
            size)
        {
            TenantId = AbpSession.TenantId,
            ContentType = contentType,
            IsRequired = true // Determined by category
        };

        await _documentRepository.InsertAsync(document);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(document.Id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Documents_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var document = await _documentRepository.GetAsync(id);

        if (document.IsVerified)
            throw new UserFriendlyException(AdmissionsExceptionCodes.CannotDeleteVerifiedDocument,
                "Cannot delete a verified document. Upload a replacement instead.");

        // Best-effort: remove the blob from the private bucket. FileUrl holds
        // the object key (see UploadAsync). We swallow storage errors so a
        // transient storage hiccup can't block deleting the DB row; an orphaned
        // blob is harmless (private, unreferenced) and can be reaped later.
        if (IsDocumentObjectKey(document.FileUrl))
        {
            try { await _fileStorage.DeleteAsync(DocumentsBucket, document.FileUrl); }
            catch (Exception ex) { Logger.Warn($"Failed to delete document blob '{document.FileUrl}': {ex.Message}"); }
        }

        await _documentRepository.DeleteAsync(document);
    }

    [AbpAuthorize(PermissionNames.Admissions_Documents_Verify)]
    public async Task<ApplicationDocumentDto> VerifyAsync(Guid id, VerifyDocumentDto input)
    {
        var document = await _documentRepository.GetAsync(id);

        document.IsVerified = true;
        document.VerifiedByUserId = AbpSession.UserId;
        document.VerifiedDate = DateTime.UtcNow;

        await _documentRepository.UpdateAsync(document);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Documents_Reject)]
    public async Task<ApplicationDocumentDto> RejectAsync(Guid id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new UserFriendlyException(AdmissionsExceptionCodes.DocumentRejectionReasonRequired,
                "A reason is required when rejecting a document.");

        var document = await _documentRepository.GetAsync(id);

        document.IsVerified = false;
        document.VerifiedByUserId = AbpSession.UserId;
        document.VerifiedDate = DateTime.UtcNow;
        document.RejectionReason = reason;

        // Update application status to request documents
        var application = await _applicationRepository.GetAsync(document.ApplicationId);
        if (application.Status == ApplicationStatus.UnderReview)
        {
            application.RequestDocuments();
            await _applicationRepository.UpdateAsync(application);
        }

        await _documentRepository.UpdateAsync(document);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Documents_Download)]
    public async Task<string> GetDownloadUrlAsync(Guid id)
    {
        var document = await _documentRepository
            .GetAll()
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);
        if (document == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.DocumentNotFound, "Document not found.");

        // The bucket is private — return a short-lived signed URL rather than a
        // public link. FileUrl holds the storage object key (see UploadAsync).
        if (!IsDocumentObjectKey(document.FileUrl))
            throw new UserFriendlyException(AdmissionsExceptionCodes.DocumentNotFound,
                "This document has no stored file. Please re-upload it.");

        return await _fileStorage.CreateSignedDownloadUrlAsync(
            DocumentsBucket, document.FileUrl, DownloadUrlExpirySeconds);
    }

    #region Private Methods

    /// <summary>Tenant + application-scoped object key for a document file.</summary>
    private string BuildDocumentObjectKey(Guid applicationId, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return $"{AbpSession.TenantId ?? 0}/{applicationId}/{Guid.NewGuid()}{ext}";
    }

    /// <summary>
    /// True if the stored value looks like a real storage object key (this
    /// tenant's prefix) rather than a legacy placeholder "/api/documents/..."
    /// URL from before SF-03.
    /// </summary>
    private bool IsDocumentObjectKey(string value)
        => !string.IsNullOrWhiteSpace(value)
           && value.StartsWith($"{AbpSession.TenantId ?? 0}/", StringComparison.Ordinal);

    private void EnsureApplicationAcceptsUploads(Application application)
    {
        if (application.Status == ApplicationStatus.Enrolled
            || application.Status == ApplicationStatus.Withdrawn
            || application.Status == ApplicationStatus.Expired
            || application.Status == ApplicationStatus.Rejected)
        {
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidDocumentUpload,
                "Cannot upload documents to this application.");
        }
    }

    /// <summary>Extension whitelist (ADM-009), checked before minting a ticket.</summary>
    private void ValidateDocumentExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidDocumentFormat,
                "Invalid file format. Allowed formats: PDF, JPG, JPEG, PNG.");
    }

    /// <summary>
    /// Validates a client-supplied object key (must sit in this tenant +
    /// application prefix, no traversal), confirms the file exists in storage,
    /// and enforces ADM-009 (extension + 10 MB cap + content-type whitelist)
    /// against the REAL stored metadata. Returns the validated key + real size
    /// + content type.
    /// </summary>
    private async Task<(string ObjectKey, long Size, string ContentType)> ResolveUploadedDocumentAsync(
        string objectKey, Guid applicationId, string fileName)
    {
        var expectedPrefix = $"{AbpSession.TenantId ?? 0}/{applicationId}/";
        if (string.IsNullOrWhiteSpace(objectKey)
            || !objectKey.StartsWith(expectedPrefix, StringComparison.Ordinal)
            || objectKey.Contains(".."))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidDocumentUpload,
                "Invalid upload reference. Call RequestUploadUrl and upload the file first.");

        // Validate the extension on the authoritative object key (what is
        // actually stored), not just the cosmetic FileName.
        ValidateDocumentExtension(objectKey);

        var info = await _fileStorage.GetObjectInfoAsync(DocumentsBucket, objectKey);
        if (info == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidDocumentUpload,
                "The uploaded file was not found in storage. Please re-upload.");

        if (info.SizeBytes == 0)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidDocumentUpload, "No file provided.");

        if (info.SizeBytes > MaxFileSizeBytes)
            throw new UserFriendlyException(AdmissionsExceptionCodes.DocumentTooLarge,
                "File size exceeds maximum allowed (10MB).");

        var contentType = string.IsNullOrWhiteSpace(info.ContentType)
            ? "application/octet-stream"
            : info.ContentType;
        // Compare on the media type only — storage may report a parameterised
        // value like "image/jpeg; charset=binary" which must still match.
        var mediaType = contentType.Split(';')[0].Trim().ToLowerInvariant();
        if (!AllowedContentTypes.Contains(mediaType))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidDocumentFormat,
                "Invalid file content type.");

        return (objectKey, info.SizeBytes, mediaType);
    }

    private async Task<Domain.Admissions.Entities.AdmissionSettings> GetAdmissionSettingsAsync(Guid academicYearId, Guid gradeId)
    {
        var settings = await _settingsRepository
            .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == gradeId);

        if (settings == null)
        {
            settings = await _settingsRepository
                .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == null);
        }

        return settings;
    }

    private List<(DocumentCategory Category, string Description, bool IsRequired)> GetRequiredDocumentCategories(
        bool isSACitizen, string gradeName)
    {
        var categories = new List<(DocumentCategory, string, bool)>
        {
            // Required for ALL applications (ADM-008)
            (DocumentCategory.BirthCertificate, isSACitizen ? "Birth Certificate" : "Passport Copy", true),
            (DocumentCategory.ParentIdDocument, "Parent/Guardian ID Document", true),
            (DocumentCategory.PassportPhoto, "Passport-size Photo of Student", true),
            (DocumentCategory.ProofOfResidence, "Proof of Residence (not older than 3 months)", true)
        };

        // Grade R specific
        if (gradeName?.ToUpperInvariant() == "GRADE R" || gradeName?.ToUpperInvariant() == "R")
        {
            categories.Add((DocumentCategory.ImmunizationRecord, "Immunization Record (Road to Health Card)", true));
        }

        // Grades 1-12
        if (gradeName != null && !gradeName.ToUpperInvariant().Contains("R"))
        {
            categories.Add((DocumentCategory.PreviousSchoolReport, "Previous School Report Card", true));
            categories.Add((DocumentCategory.TransferLetter, "Transfer Letter (if mid-year)", false));
        }

        // Non-SA students
        if (!isSACitizen)
        {
            categories.Add((DocumentCategory.StudyPermit, "Valid Study Permit", true));
        }

        return categories;
    }

    #endregion
}
