using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Learning.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Shared.Storage;
using psms.Learning.LearningMaterials.Dto;
using psms.Learning.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Learning.LearningMaterials;

/// <summary>
/// Service for managing learning materials.
/// </summary>
[AbpAuthorize(PermissionNames.Learning_Materials)]
public class LearningMaterialAppService : ApplicationService, ILearningMaterialAppService
{
    private readonly IRepository<LearningMaterial, Guid> _learningMaterialRepository;
    private readonly IRepository<LearningMaterialVersion, Guid> _versionRepository;
    private readonly IRepository<ClassSubject, Guid> _classSubjectRepository;
    private readonly IRepository<Term, Guid> _termRepository;
    private readonly IRepository<Teacher, Guid> _teacherRepository;
    private readonly IFileStorageService _fileStorage;

    // LM-003 retention cap. Beyond this we prune the oldest version on
    // every new upload to keep history bounded.
    private const int MaxVersionsPerMaterial = 10;

    // Supabase bucket for learning-material files (public-read).
    private const string MaterialsBucket = "materials";

    public LearningMaterialAppService(
        IRepository<LearningMaterial, Guid> learningMaterialRepository,
        IRepository<LearningMaterialVersion, Guid> versionRepository,
        IRepository<ClassSubject, Guid> classSubjectRepository,
        IRepository<Term, Guid> termRepository,
        IRepository<Teacher, Guid> teacherRepository,
        IFileStorageService fileStorage)
    {
        _learningMaterialRepository = learningMaterialRepository;
        _versionRepository = versionRepository;
        _classSubjectRepository = classSubjectRepository;
        _termRepository = termRepository;
        _teacherRepository = teacherRepository;
        _fileStorage = fileStorage;
    }

    /// <summary>Tenant-scoped object key for a material file.</summary>
    private string BuildMaterialObjectKey(Guid classSubjectId, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return $"{AbpSession.TenantId ?? 0}/{classSubjectId}/{Guid.NewGuid()}{ext}";
    }

    /// <summary>
    /// Validates a client-supplied object key (must be in this tenant +
    /// class-subject prefix), confirms the file actually exists in storage,
    /// reads its REAL size/type (the client's reported values are not
    /// trusted), enforces LM-001, and returns the server-derived public URL.
    /// </summary>
    private async Task<(string FileUrl, long Size, string ContentType)> ResolveUploadedFileAsync(
        string objectKey, Guid classSubjectId, string fileName, LearningMaterialType materialType)
    {
        var expectedPrefix = $"{AbpSession.TenantId ?? 0}/{classSubjectId}/";
        if (string.IsNullOrWhiteSpace(objectKey)
            || !objectKey.StartsWith(expectedPrefix, StringComparison.Ordinal)
            || objectKey.Contains(".."))
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLearningMaterialUpload,
                "Invalid upload reference. Call RequestUploadUrl and upload the file first.");

        var info = await _fileStorage.GetObjectInfoAsync(MaterialsBucket, objectKey);
        if (info == null)
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLearningMaterialUpload,
                "The uploaded file was not found in storage. Please re-upload.");

        // Enforce LM-001 against the REAL stored size, not a client claim.
        ValidateFileMetadata(fileName, info.SizeBytes, materialType);

        return (
            _fileStorage.GetPublicUrl(MaterialsBucket, objectKey),
            info.SizeBytes,
            string.IsNullOrWhiteSpace(info.ContentType) ? "application/octet-stream" : info.ContentType);
    }

    /// <summary>
    /// Step 1 of the direct upload: validate ownership + file type, then mint
    /// a one-time signed URL the client PUTs the bytes to (bytes never pass
    /// through the server). The client then calls UploadAsync with the
    /// resulting public URL.
    /// </summary>
    [AbpAuthorize(PermissionNames.Learning_Materials_Upload)]
    public async Task<FileUploadTicket> RequestUploadUrlAsync(RequestMaterialUploadUrlDto input)
    {
        var teacherId = await ResolveCurrentTeacherIdOrThrowAsync();
        var classSubject = await _classSubjectRepository
            .GetAll()
            .FirstOrDefaultAsync(cs => cs.Id == input.ClassSubjectId
                                    && cs.TenantId == AbpSession.TenantId
                                    && cs.TeacherId == teacherId);
        if (classSubject == null)
            throw new UserFriendlyException(LearningExceptionCodes.ClassSubjectNotFound,
                "Class-subject assignment not found for the current teacher.");

        ValidateFileExtension(input.FileName, input.MaterialType);

        var key = BuildMaterialObjectKey(input.ClassSubjectId, input.FileName);
        return await _fileStorage.CreateUploadTicketAsync(MaterialsBucket, key);
    }

    /// <summary>
    /// Step 1 of the direct upload for a new version of an existing material.
    /// </summary>
    [AbpAuthorize(PermissionNames.Learning_Materials_ManageVersions)]
    public async Task<FileUploadTicket> RequestVersionUploadUrlAsync(RequestVersionUploadUrlDto input)
    {
        var teacherId = await ResolveCurrentTeacherIdOrThrowAsync();
        var material = await _learningMaterialRepository
            .GetAll()
            .Include(lm => lm.ClassSubject)
            .FirstOrDefaultAsync(lm => lm.Id == input.LearningMaterialId
                                    && lm.TenantId == AbpSession.TenantId);
        if (material == null || material.ClassSubject?.TeacherId != teacherId)
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialNotFound,
                "You may only upload new versions for materials in your own classes.");

        ValidateFileExtension(input.FileName, material.MaterialType);

        var key = BuildMaterialObjectKey(material.ClassSubjectId, input.FileName);
        return await _fileStorage.CreateUploadTicketAsync(MaterialsBucket, key);
    }

    [AbpAuthorize(PermissionNames.Learning_Materials_View)]
    public async Task<LearningMaterialDto> GetAsync(Guid id)
    {
        var material = await _learningMaterialRepository
            .GetAll()
            .Include(lm => lm.ClassSubject).ThenInclude(cs => cs.Class)
            .Include(lm => lm.ClassSubject).ThenInclude(cs => cs.Subject)
            .Include(lm => lm.Term)
            .FirstOrDefaultAsync(lm => lm.Id == id && lm.TenantId == AbpSession.TenantId);

        if (material == null)
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialNotFound,
                "Learning material not found.");

        return ObjectMapper.Map<LearningMaterialDto>(material);
    }

    [AbpAuthorize(PermissionNames.Learning_Materials_View)]
    public async Task<PagedResultDto<LearningMaterialListDto>> GetAllAsync(GetLearningMaterialsInput input)
    {
        var query = _learningMaterialRepository
            .GetAll()
            .Where(lm => lm.TenantId == AbpSession.TenantId)
            .WhereIf(input.ClassSubjectId.HasValue, lm => lm.ClassSubjectId == input.ClassSubjectId.Value)
            .WhereIf(input.TermId.HasValue, lm => lm.TermId == input.TermId.Value)
            .WhereIf(input.MaterialType.HasValue, lm => lm.MaterialType == input.MaterialType.Value)
            .WhereIf(input.IsPublished.HasValue, lm => lm.IsPublished == input.IsPublished.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                lm => lm.Title.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "DisplayOrder ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<LearningMaterialListDto>(
            totalCount,
            ObjectMapper.Map<List<LearningMaterialListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Learning_Materials_View)]
    public async Task<ListResultDto<LearningMaterialListDto>> GetByClassSubjectAsync(Guid classSubjectId)
    {
        var items = await _learningMaterialRepository
            .GetAll()
            .Where(lm => lm.TenantId == AbpSession.TenantId && lm.ClassSubjectId == classSubjectId)
            .OrderBy(lm => lm.DisplayOrder)
            .ToListAsync();

        return new ListResultDto<LearningMaterialListDto>(
            ObjectMapper.Map<List<LearningMaterialListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Learning_Materials_Upload)]
    public async Task<LearningMaterialDto> CreateAsync(CreateLearningMaterialDto input)
    {
        // Resolve the calling teacher and scope the ClassSubject lookup to
        // their own assignments — matches the ownership guard added to
        // UploadAsync.
        var teacherId = await ResolveCurrentTeacherIdOrThrowAsync();

        var classSubject = await _classSubjectRepository
            .GetAll()
            .FirstOrDefaultAsync(cs => cs.Id == input.ClassSubjectId
                                    && cs.TenantId == AbpSession.TenantId
                                    && cs.TeacherId == teacherId);

        if (classSubject == null)
            throw new UserFriendlyException(LearningExceptionCodes.ClassSubjectNotFound,
                "Class-subject assignment not found for the current teacher.");

        // Validate Term exists (if provided)
        if (input.TermId.HasValue)
        {
            var term = await _termRepository.FirstOrDefaultAsync(t => t.Id == input.TermId.Value);
            if (term == null)
                throw new UserFriendlyException(LearningExceptionCodes.TermNotFound, "Term not found.");
        }

        // Duplicate title check scoped to ClassSubject + tenant
        var duplicateExists = await _learningMaterialRepository
            .GetAll()
            .AnyAsync(lm => lm.TenantId == AbpSession.TenantId
                && lm.ClassSubjectId == input.ClassSubjectId
                && lm.Title.ToLower() == input.Title.Trim().ToLower());

        if (duplicateExists)
            throw new UserFriendlyException(LearningExceptionCodes.DuplicateLearningMaterialTitle,
                "A learning material with this title already exists for this class-subject.");

        var material = new LearningMaterial(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.ClassSubjectId,
            input.Title.Trim(),
            input.MaterialType,
            AbpSession.UserId.Value)
        {
            Description = input.Description,
            TermId = input.TermId,
            DisplayOrder = input.DisplayOrder,
            ExternalLink = input.ExternalLink
        };

        // Set file info if provided. Guard against arbitrary/cross-tenant
        // URLs: a supplied FileUrl must point inside this tenant's materials
        // space (the secure upload path is RequestUploadUrl + UploadAsync).
        if (!string.IsNullOrWhiteSpace(input.FileName) && !string.IsNullOrWhiteSpace(input.FileUrl))
        {
            var allowedPrefix = _fileStorage.GetPublicUrl(MaterialsBucket, $"{AbpSession.TenantId ?? 0}/");
            if (!input.FileUrl.StartsWith(allowedPrefix, StringComparison.Ordinal))
                throw new UserFriendlyException(LearningExceptionCodes.InvalidLearningMaterialUpload,
                    "File URL must reference an uploaded file in this school's materials storage.");
            material.SetFile(input.FileName, input.FileUrl, input.FileSizeBytes ?? 0, input.ContentType);
        }

        await _learningMaterialRepository.InsertAsync(material);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(material.Id);
    }

    [AbpAuthorize(PermissionNames.Learning_Materials_Upload)]
    public async Task<LearningMaterialDto> UploadAsync(UploadLearningMaterialDto input)
    {
        // ── Resolve the calling teacher (used for ownership scope below).
        // Without a teacher profile the upload cannot proceed — only
        // teachers may post materials.
        var teacherId = await ResolveCurrentTeacherIdOrThrowAsync();

        // ── Validate ClassSubject exists, belongs to this tenant, AND is
        // assigned to the calling teacher. This is the teacher-ownership
        // guard required by US-TCH-001's acceptance criteria: a teacher
        // can only upload to classes/subjects they teach.
        var classSubject = await _classSubjectRepository
            .GetAll()
            .FirstOrDefaultAsync(cs => cs.Id == input.ClassSubjectId
                                    && cs.TenantId == AbpSession.TenantId
                                    && cs.TeacherId == teacherId);
        if (classSubject == null)
            throw new UserFriendlyException(LearningExceptionCodes.ClassSubjectNotFound,
                "Class-subject assignment not found for the current teacher.");

        // ── Validate Term (if provided)
        if (input.TermId.HasValue)
        {
            var term = await _termRepository.FirstOrDefaultAsync(t => t.Id == input.TermId.Value);
            if (term == null)
                throw new UserFriendlyException(LearningExceptionCodes.TermNotFound, "Term not found.");
        }

        // ── Duplicate title check (LM-002 — unique title per class subject).
        // Cheap to run; do it BEFORE the more expensive file validation so
        // a teacher who collides on title doesn't pay the cost of streaming
        // a 500 MB video up the wire only to be rejected at the end.
        var duplicateExists = await _learningMaterialRepository
            .GetAll()
            .AnyAsync(lm => lm.TenantId == AbpSession.TenantId
                && lm.ClassSubjectId == input.ClassSubjectId
                && lm.Title.ToLower() == input.Title.Trim().ToLower());
        if (duplicateExists)
            throw new UserFriendlyException(LearningExceptionCodes.DuplicateLearningMaterialTitle,
                "A learning material with this title already exists for this class-subject.");

        // ── Validate input combination
        var isExternalLinkOnly = input.MaterialType == LearningMaterialType.ExternalLink;
        (string FileUrl, long Size, string ContentType)? resolvedFile = null;
        if (isExternalLinkOnly)
        {
            if (string.IsNullOrWhiteSpace(input.ExternalLink))
                throw new UserFriendlyException(LearningExceptionCodes.InvalidLearningMaterialUpload,
                    "An external link is required for ExternalLink material type.");
        }
        else
        {
            resolvedFile = await ResolveUploadedFileAsync(
                input.ObjectKey, input.ClassSubjectId, input.FileName, input.MaterialType);
        }

        var material = new LearningMaterial(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.ClassSubjectId,
            input.Title.Trim(),
            input.MaterialType,
            AbpSession.UserId.Value)
        {
            Description = input.Description,
            TermId = input.TermId,
            DisplayOrder = input.DisplayOrder,
            ExternalLink = input.ExternalLink
        };

        // The file was uploaded directly to storage via the RequestUploadUrl
        // ticket; record the server-derived URL + real size/type.
        if (resolvedFile != null)
        {
            material.SetFile(
                input.FileName,
                resolvedFile.Value.FileUrl,
                resolvedFile.Value.Size,
                resolvedFile.Value.ContentType);
        }

        await _learningMaterialRepository.InsertAsync(material);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(material.Id);
    }

    [AbpAuthorize(PermissionNames.Learning_Materials_Edit)]
    public async Task<LearningMaterialDto> UpdateAsync(Guid id, UpdateLearningMaterialDto input)
    {
        var material = await _learningMaterialRepository
            .FirstOrDefaultAsync(lm => lm.Id == id && lm.TenantId == AbpSession.TenantId);

        if (material == null)
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialNotFound,
                "Learning material not found.");

        // Duplicate title check if title is changing
        if (input.Title != null)
        {
            var duplicateExists = await _learningMaterialRepository
                .GetAll()
                .AnyAsync(lm => lm.TenantId == AbpSession.TenantId
                    && lm.ClassSubjectId == material.ClassSubjectId
                    && lm.Id != id
                    && lm.Title.ToLower() == input.Title.Trim().ToLower());

            if (duplicateExists)
                throw new UserFriendlyException(LearningExceptionCodes.DuplicateLearningMaterialTitle,
                    "A learning material with this title already exists for this class-subject.");

            material.Title = input.Title.Trim();
        }

        if (input.Description != null) material.Description = input.Description;
        if (input.MaterialType.HasValue) material.MaterialType = input.MaterialType.Value;
        if (input.TermId.HasValue)
        {
            var term = await _termRepository.FirstOrDefaultAsync(t => t.Id == input.TermId.Value);
            if (term == null)
                throw new UserFriendlyException(LearningExceptionCodes.TermNotFound, "Term not found.");
            material.TermId = input.TermId.Value;
        }
        if (input.FileName != null) material.FileName = input.FileName;
        if (input.FileUrl != null) material.FileUrl = input.FileUrl;
        if (input.FileSizeBytes.HasValue) material.FileSizeBytes = input.FileSizeBytes.Value;
        if (input.ContentType != null) material.ContentType = input.ContentType;
        if (input.ExternalLink != null) material.ExternalLink = input.ExternalLink;
        if (input.DisplayOrder.HasValue) material.DisplayOrder = input.DisplayOrder.Value;

        await _learningMaterialRepository.UpdateAsync(material);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Learning_Materials_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var material = await _learningMaterialRepository
            .FirstOrDefaultAsync(lm => lm.Id == id && lm.TenantId == AbpSession.TenantId);

        if (material == null)
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialNotFound,
                "Learning material not found.");

        await _learningMaterialRepository.DeleteAsync(material);
    }

    [AbpAuthorize(PermissionNames.Learning_Materials_Edit)]
    public async Task<LearningMaterialDto> PublishAsync(Guid id)
    {
        var material = await _learningMaterialRepository
            .FirstOrDefaultAsync(lm => lm.Id == id && lm.TenantId == AbpSession.TenantId);

        if (material == null)
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialNotFound,
                "Learning material not found.");

        if (material.IsPublished)
            throw new UserFriendlyException(LearningExceptionCodes.MaterialAlreadyPublished,
                "Learning material is already published.");

        material.Publish();
        await _learningMaterialRepository.UpdateAsync(material);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Learning_Materials_Edit)]
    public async Task<LearningMaterialDto> UnpublishAsync(Guid id)
    {
        var material = await _learningMaterialRepository
            .FirstOrDefaultAsync(lm => lm.Id == id && lm.TenantId == AbpSession.TenantId);

        if (material == null)
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialNotFound,
                "Learning material not found.");

        if (!material.IsPublished)
            throw new UserFriendlyException(LearningExceptionCodes.MaterialNotPublished,
                "Learning material is not published.");

        material.Unpublish();
        await _learningMaterialRepository.UpdateAsync(material);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Learning_Materials_View)]
    public async Task IncrementViewCountAsync(Guid id)
    {
        var material = await _learningMaterialRepository
            .FirstOrDefaultAsync(lm => lm.Id == id && lm.TenantId == AbpSession.TenantId);

        if (material == null)
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialNotFound,
                "Learning material not found.");

        material.IncrementViewCount();
        await _learningMaterialRepository.UpdateAsync(material);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Learning_Materials_View)]
    public async Task<ListResultDto<LearningMaterialVersionDto>> GetVersionsAsync(Guid learningMaterialId)
    {
        // Teacher-ownership scope: a teacher can only enumerate version
        // history for materials in classes they teach. Without this guard
        // any teacher (or any role holding the view permission) could
        // enumerate another teacher's change descriptions and file URLs.
        var teacherId = await ResolveCurrentTeacherIdOrThrowAsync();

        var materialOwned = await _learningMaterialRepository
            .GetAll()
            .Include(lm => lm.ClassSubject)
            .AnyAsync(lm => lm.Id == learningMaterialId
                         && lm.TenantId == AbpSession.TenantId
                         && lm.ClassSubject.TeacherId == teacherId);
        if (!materialOwned)
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialNotFound,
                "Learning material not found for the current teacher.");

        var versions = await _versionRepository
            .GetAll()
            .Where(v => v.LearningMaterialId == learningMaterialId
                     && v.TenantId == AbpSession.TenantId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();

        return new ListResultDto<LearningMaterialVersionDto>(
            ObjectMapper.Map<List<LearningMaterialVersionDto>>(versions));
    }

    [AbpAuthorize(PermissionNames.Learning_Materials_ManageVersions)]
    public async Task<LearningMaterialDto> UploadNewVersionAsync(UploadNewVersionDto input)
    {
        var teacherId = await ResolveCurrentTeacherIdOrThrowAsync();

        // Load the material WITH its current ClassSubject so we can scope
        // the ownership check to the calling teacher.
        var material = await _learningMaterialRepository
            .GetAll()
            .Include(lm => lm.ClassSubject)
            .FirstOrDefaultAsync(lm => lm.Id == input.LearningMaterialId
                                    && lm.TenantId == AbpSession.TenantId);
        if (material == null)
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialNotFound,
                "Learning material not found.");

        if (material.ClassSubject?.TeacherId != teacherId)
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialNotFound,
                "You may only upload new versions for materials in your own classes.");

        // The bytes were uploaded directly to storage via
        // RequestVersionUploadUrl; validate the key + measure the real file
        // server-side (LM-001 against the parent material's type).
        var resolved = await ResolveUploadedFileAsync(
            input.ObjectKey, material.ClassSubjectId, input.FileName, material.MaterialType);

        // Best-effort next number; the unique index on
        // (LearningMaterialId, VersionNumber) is the source of truth and is
        // what *detects* a concurrent insert — see the try/catch below.
        // Tenant filter is defence-in-depth: a query without it would still
        // be correct because LearningMaterialId already implies the tenant,
        // but it keeps the behaviour stable even if ABP's tenant filter is
        // ever disabled upstream.
        var nextVersionNumber = await _versionRepository
            .GetAll()
            .Where(v => v.LearningMaterialId == material.Id
                     && v.TenantId == AbpSession.TenantId)
            .Select(v => (int?)v.VersionNumber)
            .MaxAsync() ?? 0;
        nextVersionNumber += 1;

        var version = new LearningMaterialVersion(
            Guid.NewGuid(),
            AbpSession.TenantId,
            material.Id,
            nextVersionNumber,
            // The column has a pre-existing NOT NULL constraint (no
            // migration for this change), so an omitted description is
            // stored as empty rather than null.
            input.ChangeDescription?.Trim() ?? string.Empty,
            AbpSession.UserId.Value);

        version.SetFile(
            input.FileName,
            resolved.FileUrl,
            resolved.Size,
            resolved.ContentType);

        await _versionRepository.InsertAsync(version);

        // Move the parent material's current pointer to the new version.
        material.SetFile(
            input.FileName,
            resolved.FileUrl,
            resolved.Size,
            resolved.ContentType);

        // Enforce LM-003 — retain at most MaxVersionsPerMaterial rows.
        // SaveChanges first so the new row is visible to the count query.
        // Concurrent uploads can race on MaxAsync(VersionNumber)+1 — surface
        // *only* the unique-index violation as a friendly retry. Anything
        // else (FK, NOT NULL, deadlock, etc.) must bubble so logs are useful.
        // NOTE: callers must NOT retry within the same UoW — `material` is
        // already dirty (SetFile above) and `version` has been Insert'd; on
        // rollback the DB is consistent but ABP's ChangeTracker still sees
        // the mutations.
        try
        {
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new UserFriendlyException(LearningExceptionCodes.VersionConflict,
                "Another version was saved for this material at the same time. Please retry.");
        }
        await PruneOldVersionsAsync(material.Id);

        return await GetAsync(material.Id);
    }

    [AbpAuthorize(PermissionNames.Learning_Materials_ManageVersions)]
    public async Task<LearningMaterialDto> RestoreVersionAsync(Guid learningMaterialId, Guid versionId)
    {
        var teacherId = await ResolveCurrentTeacherIdOrThrowAsync();

        var material = await _learningMaterialRepository
            .GetAll()
            .Include(lm => lm.ClassSubject)
            .FirstOrDefaultAsync(lm => lm.Id == learningMaterialId
                                    && lm.TenantId == AbpSession.TenantId);
        if (material == null)
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialNotFound,
                "Learning material not found.");

        if (material.ClassSubject?.TeacherId != teacherId)
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialNotFound,
                "You may only restore versions on materials in your own classes.");

        var sourceVersion = await _versionRepository
            .GetAll()
            .FirstOrDefaultAsync(v => v.Id == versionId
                                   && v.LearningMaterialId == learningMaterialId
                                   && v.TenantId == AbpSession.TenantId);
        if (sourceVersion == null)
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialNotFound,
                "Version not found for this material.");

        var nextVersionNumber = await _versionRepository
            .GetAll()
            .Where(v => v.LearningMaterialId == material.Id
                     && v.TenantId == AbpSession.TenantId)
            .Select(v => (int?)v.VersionNumber)
            .MaxAsync() ?? 0;
        nextVersionNumber += 1;

        var restored = new LearningMaterialVersion(
            Guid.NewGuid(),
            AbpSession.TenantId,
            material.Id,
            nextVersionNumber,
            $"Restored from v{sourceVersion.VersionNumber}",
            AbpSession.UserId.Value);
        restored.SetFile(
            sourceVersion.FileName,
            sourceVersion.FileUrl,
            sourceVersion.FileSizeBytes,
            sourceVersion.ContentType);

        await _versionRepository.InsertAsync(restored);

        // Point the material at the restored copy.
        material.SetFile(
            sourceVersion.FileName,
            sourceVersion.FileUrl,
            sourceVersion.FileSizeBytes ?? 0,
            sourceVersion.ContentType);

        // Same race window as UploadNewVersionAsync — guard the unique-index
        // only, let other DbUpdateException causes bubble. Same UoW caveat:
        // do not retry inside the same call.
        try
        {
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new UserFriendlyException(LearningExceptionCodes.VersionConflict,
                "Another version was saved for this material at the same time. Please retry.");
        }
        // Restore copies sourceVersion.FileUrl onto `restored`, so the two
        // rows share a blob URL. Today PruneOldVersionsAsync only deletes
        // *rows* (never blobs), so a future prune of `sourceVersion` cannot
        // strand or double-free the file the parent material now points at.
        // If/when prune ever learns to delete blobs, this must switch to
        // ref-counting before that change is enabled — otherwise restoring an
        // old version and then uploading enough new ones to prune the source
        // row would 404 the material's file. Follow-up: file a ticket when
        // the storage layer changes.
        await PruneOldVersionsAsync(material.Id);

        return await GetAsync(material.Id);
    }

    #region Private Methods

    /// <summary>
    /// LM-003: trim the oldest versions when the retention cap is exceeded
    /// so a material's history never grows beyond MaxVersionsPerMaterial
    /// rows. Hard-deletes the trimmed rows; the unique-index on
    /// (MaterialId, VersionNumber) means restored copies always slot in
    /// at the top with a freshly-allocated number.
    /// </summary>
    private async Task PruneOldVersionsAsync(Guid learningMaterialId)
    {
        var totalCount = await _versionRepository
            .GetAll()
            .Where(v => v.LearningMaterialId == learningMaterialId
                     && v.TenantId == AbpSession.TenantId)
            .CountAsync();
        if (totalCount <= MaxVersionsPerMaterial) return;

        var overage = totalCount - MaxVersionsPerMaterial;
        var oldestIds = await _versionRepository
            .GetAll()
            .Where(v => v.LearningMaterialId == learningMaterialId
                     && v.TenantId == AbpSession.TenantId)
            .OrderBy(v => v.VersionNumber)
            .Take(overage)
            .Select(v => v.Id)
            .ToListAsync();
        foreach (var id in oldestIds)
        {
            await _versionRepository.DeleteAsync(id);
        }
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Returns true when the EF Core exception was raised by a unique-index
    /// or unique-constraint violation. Walks the inner-exception chain and
    /// duck-types the provider-specific exception so we don't need a direct
    /// reference to Microsoft.Data.SqlClient / Npgsql in this assembly.
    /// </summary>
    /// <remarks>
    /// SQL Server: error number 2601 (duplicate key, unique index with
    /// IGNORE_DUP_KEY off) or 2627 (unique constraint violation).
    /// PostgreSQL: SQLSTATE 23505 (unique_violation).
    /// </remarks>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
        {
            var typeName = inner.GetType().FullName;
            if (typeName == "Microsoft.Data.SqlClient.SqlException"
                || typeName == "System.Data.SqlClient.SqlException")
            {
                var numberValue = inner.GetType().GetProperty("Number")?.GetValue(inner);
                if (numberValue is int number && (number == 2601 || number == 2627))
                    return true;
            }
            else if (typeName == "Npgsql.PostgresException")
            {
                var sqlStateValue = inner.GetType().GetProperty("SqlState")?.GetValue(inner);
                if (sqlStateValue is string sqlState && sqlState == "23505")
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Resolves the Teacher.Id for the active session user. Used to scope
    /// class-subject lookups so a teacher can only upload to subjects they
    /// are assigned to (US-TCH-001 acceptance criterion). Throws when the
    /// active user has no Teacher record linked — Parents/Students can
    /// hold the Learning_Materials_Upload permission in principle but the
    /// upload flow itself is teacher-only.
    /// </summary>
    private async Task<Guid> ResolveCurrentTeacherIdOrThrowAsync()
    {
        if (AbpSession.UserId == null)
            throw new UserFriendlyException(LearningExceptionCodes.ClassSubjectNotFound,
                "No active user session.");

        var teacher = await _teacherRepository
            .GetAll()
            .FirstOrDefaultAsync(t => t.UserId == AbpSession.UserId.Value
                                   && t.TenantId == AbpSession.TenantId);
        if (teacher == null)
            throw new UserFriendlyException(LearningExceptionCodes.ClassSubjectNotFound,
                "No teacher profile is linked to the current user.");

        return teacher.Id;
    }

    // File caps per LM-001 (Supplementary). Video gets a larger cap; other
    // material types share the document/audio/image cap.
    private const long DocumentSizeCapBytes = 50L * 1024 * 1024;   //  50 MB
    private const long VideoSizeCapBytes    = 500L * 1024 * 1024;  // 500 MB

    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx",
        ".txt", ".csv", ".rtf", ".zip"
    };

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg"
    };

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".ogg", ".m4a", ".flac"
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mov", ".avi", ".webm", ".mkv"
    };

    // Interactive material is the only type that legitimately accepts
    // packaged web content; kept as its own set so it doesn't dilute the
    // generic document whitelist.
    private static readonly HashSet<string> InteractiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".html"
    };

    private static (HashSet<string> allowed, long sizeCap, string label) GetTypeRules(LearningMaterialType materialType)
        => materialType switch
        {
            LearningMaterialType.Video         => (VideoExtensions,       VideoSizeCapBytes,    "video"),
            LearningMaterialType.Audio         => (AudioExtensions,       DocumentSizeCapBytes, "audio"),
            LearningMaterialType.Image         => (ImageExtensions,       DocumentSizeCapBytes, "image"),
            LearningMaterialType.Presentation  => (DocumentExtensions,    DocumentSizeCapBytes, "presentation"),
            LearningMaterialType.Worksheet     => (DocumentExtensions,    DocumentSizeCapBytes, "worksheet"),
            LearningMaterialType.Document      => (DocumentExtensions,    DocumentSizeCapBytes, "document"),
            LearningMaterialType.Interactive   => (InteractiveExtensions, DocumentSizeCapBytes, "interactive"),
            _                                  => (DocumentExtensions,    DocumentSizeCapBytes, "file"),
        };

    // LM-001 type whitelist. Checked at upload-URL request time (size unknown)
    // and again on Create against the client-reported size.
    private static void ValidateFileExtension(string fileName, LearningMaterialType materialType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLearningMaterialUpload,
                "No file provided.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var (allowed, _, label) = GetTypeRules(materialType);

        if (!allowed.Contains(extension))
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLearningMaterialUpload,
                $"Unsupported {label} format '{extension}'. Allowed: {string.Join(", ", allowed)}.");
    }

    // LM-001 type whitelist + size cap, validated from metadata (the bytes are
    // uploaded directly to storage, not through this server).
    private static void ValidateFileMetadata(string fileName, long sizeBytes, LearningMaterialType materialType)
    {
        ValidateFileExtension(fileName, materialType);
        var (_, sizeCap, label) = GetTypeRules(materialType);

        if (sizeBytes <= 0)
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLearningMaterialUpload,
                "No file provided.");

        if (sizeBytes > sizeCap)
        {
            var actualMb = Math.Round(sizeBytes / 1024.0 / 1024.0, 1);
            var capMb = sizeCap / 1024 / 1024;
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialTooLarge,
                $"File size {actualMb} MB exceeds the {capMb} MB cap for {label} uploads.");
        }
    }

    #endregion
}
