using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.ApplicationDocuments.Dto;
using psms.Admissions.Shared;
using psms.Authorization;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
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

    // Allowed file types (ADM-009)
    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
    private static readonly string[] AllowedContentTypes = {
        "application/pdf",
        "image/jpeg",
        "image/png"
    };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    public ApplicationDocumentAppService(
        IRepository<ApplicationDocument, Guid> documentRepository,
        IRepository<Application, Guid> applicationRepository,
        IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> settingsRepository)
    {
        _documentRepository = documentRepository;
        _applicationRepository = applicationRepository;
        _settingsRepository = settingsRepository;
    }

    [AbpAuthorize(PermissionNames.Admissions_Documents_View)]
    public async Task<ApplicationDocumentDto> GetAsync(Guid id)
    {
        var document = await _documentRepository
            .GetAll()
            .Include(d => d.Application)
            .FirstOrDefaultAsync(d => d.Id == id);

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

    [AbpAuthorize(PermissionNames.Admissions_Documents_Upload)]
    public async Task<ApplicationDocumentDto> UploadAsync(UploadDocumentDto input)
    {
        var application = await _applicationRepository.GetAsync(input.ApplicationId);

        // Validate application status allows uploads
        if (application.Status == ApplicationStatus.Enrolled
            || application.Status == ApplicationStatus.Withdrawn
            || application.Status == ApplicationStatus.Expired
            || application.Status == ApplicationStatus.Rejected)
        {
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidDocumentUpload,
                "Cannot upload documents to this application.");
        }

        // Validate file (ADM-009)
        ValidateFile(input.File);

        // Generate storage path and file name
        var fileExtension = Path.GetExtension(input.File.FileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid()}{fileExtension}";
        var storagePath = Path.Combine("documents", "applications", application.ApplicationNumber, storedFileName);

        // TODO: Implement actual file storage (Azure Blob, AWS S3, or local file system)
        // For now, we'll just store the path reference
        var fileUrl = $"/api/documents/{storagePath}";

        var document = new ApplicationDocument(
            Guid.NewGuid(),
            input.ApplicationId,
            input.Category,
            $"{input.Category}_{application.ApplicationNumber}",
            input.File.FileName,
            fileUrl,
            input.File.Length)
        {
            ContentType = input.File.ContentType,
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

        // TODO: Delete actual file from storage

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
        var document = await _documentRepository.GetAsync(id);
        return document.FileUrl;
    }

    #region Private Methods

    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidDocumentUpload, "No file provided.");

        if (file.Length > MaxFileSizeBytes)
            throw new UserFriendlyException(AdmissionsExceptionCodes.DocumentTooLarge,
                "File size exceeds maximum allowed (10MB).");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidDocumentFormat,
                "Invalid file format. Allowed formats: PDF, JPG, JPEG, PNG.");

        if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidDocumentFormat,
                "Invalid file content type.");
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
