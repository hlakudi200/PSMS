using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Communication.Documents.Dto;
using psms.Communication.Shared;
using psms.Domain.Academic.Entities;
using psms.Domain.Communication.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Communication.Documents;

[AbpAuthorize(PermissionNames.Communication_Documents)]
public class DocumentAppService : ApplicationService, IDocumentAppService
{
    private readonly IRepository<Document, Guid> _documentRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;

    public DocumentAppService(
        IRepository<Document, Guid> documentRepository,
        IRepository<AcademicYear, Guid> academicYearRepository)
    {
        _documentRepository = documentRepository;
        _academicYearRepository = academicYearRepository;
    }

    [AbpAuthorize(PermissionNames.Communication_Documents_View)]
    public async Task<DocumentDto> GetAsync(Guid id)
    {
        var document = await _documentRepository
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

        if (document == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.DocumentNotFound,
                "Document not found.");

        return ObjectMapper.Map<DocumentDto>(document);
    }

    [AbpAuthorize(PermissionNames.Communication_Documents_View)]
    public async Task<PagedResultDto<DocumentListDto>> GetAllAsync(GetDocumentsInput input)
    {
        var query = _documentRepository
            .GetAll()
            .Where(d => d.TenantId == AbpSession.TenantId)
            .WhereIf(input.DocumentType.HasValue, d => d.DocumentType == input.DocumentType.Value)
            .WhereIf(input.TargetAudience.HasValue, d => d.TargetAudience == input.TargetAudience.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Category),
                d => d.Category.ToLower() == input.Category.Trim().ToLower())
            .WhereIf(input.IsPublished.HasValue, d => d.IsPublished == input.IsPublished.Value)
            .WhereIf(input.AcademicYearId.HasValue, d => d.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                d => d.Title.ToLower().Contains(input.Search.Trim().ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                d => d.Title.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<DocumentListDto>(
            totalCount,
            ObjectMapper.Map<List<DocumentListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Communication_Documents_Upload)]
    public async Task<DocumentDto> CreateAsync(CreateDocumentDto input)
    {
        // Validate AcademicYear if provided
        if (input.AcademicYearId.HasValue)
        {
            var academicYearExists = await _academicYearRepository
                .GetAll()
                .AnyAsync(ay => ay.Id == input.AcademicYearId.Value && ay.TenantId == AbpSession.TenantId);

            if (!academicYearExists)
                throw new UserFriendlyException(CommunicationExceptionCodes.AcademicYearNotFound,
                    "Academic year not found.");
        }

        var document = new Document(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.Title.Trim(),
            input.FileName.Trim(),
            input.FileUrl.Trim(),
            input.FileSizeBytes,
            input.DocumentType,
            input.TargetAudience,
            AbpSession.UserId.Value)
        {
            Description = input.Description?.Trim(),
            ContentType = input.ContentType?.Trim(),
            Category = input.Category?.Trim(),
            AcademicYearId = input.AcademicYearId
        };

        await _documentRepository.InsertAsync(document);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(document.Id);
    }

    [AbpAuthorize(PermissionNames.Communication_Documents_Upload)]
    public async Task<DocumentDto> UpdateAsync(Guid id, UpdateDocumentDto input)
    {
        var document = await _documentRepository
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

        if (document == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.DocumentNotFound,
                "Document not found.");

        // Validate AcademicYear if provided
        if (input.AcademicYearId.HasValue)
        {
            var academicYearExists = await _academicYearRepository
                .GetAll()
                .AnyAsync(ay => ay.Id == input.AcademicYearId.Value && ay.TenantId == AbpSession.TenantId);

            if (!academicYearExists)
                throw new UserFriendlyException(CommunicationExceptionCodes.AcademicYearNotFound,
                    "Academic year not found.");
        }

        if (input.Title != null) document.Title = input.Title.Trim();
        if (input.Description != null) document.Description = input.Description.Trim();
        if (input.FileName != null) document.FileName = input.FileName.Trim();
        if (input.FileUrl != null) document.FileUrl = input.FileUrl.Trim();
        if (input.FileSizeBytes.HasValue) document.FileSizeBytes = input.FileSizeBytes.Value;
        if (input.ContentType != null) document.ContentType = input.ContentType.Trim();
        if (input.Category != null) document.Category = input.Category.Trim();
        if (input.DocumentType.HasValue) document.DocumentType = input.DocumentType.Value;
        if (input.TargetAudience.HasValue) document.TargetAudience = input.TargetAudience.Value;
        if (input.ClearAcademicYearId == true) document.AcademicYearId = null;
        else if (input.AcademicYearId.HasValue) document.AcademicYearId = input.AcademicYearId;

        await _documentRepository.UpdateAsync(document);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Communication_Documents_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var document = await _documentRepository
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

        if (document == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.DocumentNotFound,
                "Document not found.");

        await _documentRepository.DeleteAsync(document);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Communication_Documents_Approve)]
    public async Task<DocumentDto> PublishAsync(Guid id)
    {
        var document = await _documentRepository
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

        if (document == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.DocumentNotFound,
                "Document not found.");

        if (document.IsPublished)
            throw new UserFriendlyException(CommunicationExceptionCodes.DocumentAlreadyPublished,
                "Document is already published.");

        document.Publish();
        await _documentRepository.UpdateAsync(document);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Communication_Documents_Approve)]
    public async Task<DocumentDto> UnpublishAsync(Guid id)
    {
        var document = await _documentRepository
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

        if (document == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.DocumentNotFound,
                "Document not found.");

        if (!document.IsPublished)
            throw new UserFriendlyException(CommunicationExceptionCodes.DocumentNotPublished,
                "Document is not published.");

        document.Unpublish();
        await _documentRepository.UpdateAsync(document);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Communication_Documents_Download)]
    public async Task<DocumentDto> RecordDownloadAsync(Guid id)
    {
        var document = await _documentRepository
            .FirstOrDefaultAsync(d => d.Id == id && d.TenantId == AbpSession.TenantId);

        if (document == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.DocumentNotFound,
                "Document not found.");

        if (!document.IsPublished)
            throw new UserFriendlyException(CommunicationExceptionCodes.DocumentNotPublished,
                "Document must be published before it can be downloaded.");

        document.IncrementDownloadCount();
        await _documentRepository.UpdateAsync(document);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
