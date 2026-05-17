using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Learning.Entities;
using psms.Domain.Shared.Enums;
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
    private readonly IRepository<ClassSubject, Guid> _classSubjectRepository;
    private readonly IRepository<Term, Guid> _termRepository;
    private readonly IRepository<Teacher, Guid> _teacherRepository;

    public LearningMaterialAppService(
        IRepository<LearningMaterial, Guid> learningMaterialRepository,
        IRepository<ClassSubject, Guid> classSubjectRepository,
        IRepository<Term, Guid> termRepository,
        IRepository<Teacher, Guid> teacherRepository)
    {
        _learningMaterialRepository = learningMaterialRepository;
        _classSubjectRepository = classSubjectRepository;
        _termRepository = termRepository;
        _teacherRepository = teacherRepository;
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

        // Set file info if provided
        if (!string.IsNullOrWhiteSpace(input.FileName) && !string.IsNullOrWhiteSpace(input.FileUrl))
        {
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
        if (isExternalLinkOnly)
        {
            if (string.IsNullOrWhiteSpace(input.ExternalLink))
                throw new UserFriendlyException(LearningExceptionCodes.InvalidLearningMaterialUpload,
                    "An external link is required for ExternalLink material type.");
        }
        else
        {
            ValidateFile(input.File, input.MaterialType);
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

        // ── Stage the file. TODO: integrate real blob storage (Azure Blob /
        // S3 / local file system). This currently mirrors the
        // ApplicationDocument/Upload pattern: we record a path reference
        // without writing the bytes anywhere. The DownloadAsync flow will
        // need to round-trip the path back to whatever store we adopt.
        if (!isExternalLinkOnly && input.File != null && input.File.Length > 0)
        {
            var fileExtension = Path.GetExtension(input.File.FileName).ToLowerInvariant();
            var storedFileName = $"{Guid.NewGuid()}{fileExtension}";
            // Build a relative path WITHOUT the leading "learning-materials"
            // segment — that segment is provided once by the URL prefix
            // below. Doubling it (as the analogous ApplicationDocument code
            // currently does) would produce
            // /api/learning-materials/learning-materials/{cs}/{guid}.ext
            // which becomes permanent on the row once real storage lands.
            var storagePath = Path.Combine(
                input.ClassSubjectId.ToString(),
                storedFileName);
            var fileUrl = $"/api/learning-materials/{storagePath.Replace('\\', '/')}";

            material.SetFile(
                input.File.FileName,
                fileUrl,
                input.File.Length,
                input.File.ContentType ?? "application/octet-stream");
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

    #region Private Methods

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

    private static void ValidateFile(IFormFile file, LearningMaterialType materialType)
    {
        if (file == null || file.Length == 0)
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLearningMaterialUpload,
                "No file provided.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var (allowed, sizeCap, label) = materialType switch
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

        if (!allowed.Contains(extension))
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLearningMaterialUpload,
                $"Unsupported {label} format '{extension}'. Allowed: {string.Join(", ", allowed)}.");

        if (file.Length > sizeCap)
        {
            var actualMb = Math.Round(file.Length / 1024.0 / 1024.0, 1);
            var capMb = sizeCap / 1024 / 1024;
            throw new UserFriendlyException(LearningExceptionCodes.LearningMaterialTooLarge,
                $"File size {actualMb} MB exceeds the {capMb} MB cap for {label} uploads.");
        }
    }

    #endregion
}
