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
using psms.Learning.LearningMaterials.Dto;
using psms.Learning.Shared;
using System;
using System.Collections.Generic;
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

    public LearningMaterialAppService(
        IRepository<LearningMaterial, Guid> learningMaterialRepository,
        IRepository<ClassSubject, Guid> classSubjectRepository,
        IRepository<Term, Guid> termRepository)
    {
        _learningMaterialRepository = learningMaterialRepository;
        _classSubjectRepository = classSubjectRepository;
        _termRepository = termRepository;
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
        // Validate ClassSubject exists
        var classSubject = await _classSubjectRepository
            .GetAll()
            .FirstOrDefaultAsync(cs => cs.Id == input.ClassSubjectId && cs.TenantId == AbpSession.TenantId);

        if (classSubject == null)
            throw new UserFriendlyException(LearningExceptionCodes.ClassSubjectNotFound,
                "Class-subject assignment not found.");

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
}
