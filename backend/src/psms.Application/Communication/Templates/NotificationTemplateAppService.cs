using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Communication.Templates.Dto;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Communication.Templates;

/// <summary>
/// COMM-06: authoring + management of notification templates. Config-grade
/// (same permission as creating notifications). Enforces one template per
/// (tenant, key, channel, language).
/// </summary>
[AbpAuthorize(PermissionNames.Communication_Notifications_Configure)]
public class NotificationTemplateAppService : ApplicationService, INotificationTemplateAppService
{
    private readonly IRepository<NotificationTemplate, Guid> _templateRepository;

    public NotificationTemplateAppService(IRepository<NotificationTemplate, Guid> templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<PagedResultDto<NotificationTemplateListDto>> GetAllAsync(GetTemplatesInput input)
    {
        var query = _templateRepository.GetAll()
            .WhereIf(!string.IsNullOrWhiteSpace(input.TemplateKey), t => t.TemplateKey == input.TemplateKey)
            .WhereIf(input.Channel.HasValue, t => t.Channel == input.Channel.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Language), t => t.Language == input.Language.Trim().ToLower())
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                t => t.TemplateKey.ToLower().Contains(input.Search.Trim().ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "TemplateKey ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<NotificationTemplateListDto>(
            totalCount,
            items.Select(ToListDto).ToList());
    }

    public async Task<NotificationTemplateDto> GetAsync(Guid id)
    {
        var template = await GetTemplateAsync(id);
        return ToDto(template);
    }

    public async Task<NotificationTemplateDto> CreateAsync(CreateNotificationTemplateDto input)
    {
        if (!Enum.IsDefined(typeof(NotificationChannel), input.Channel))
            throw new UserFriendlyException("Unknown channel.");

        var key = input.TemplateKey.Trim();
        var language = input.Language.Trim().ToLowerInvariant();

        var exists = await _templateRepository.GetAll().AnyAsync(t =>
            t.TenantId == AbpSession.TenantId
            && t.TemplateKey == key
            && t.Channel == input.Channel
            && t.Language == language);

        if (exists)
            throw new UserFriendlyException("A template already exists for this key, channel and language.");

        var template = new NotificationTemplate(
            Guid.NewGuid(), AbpSession.TenantId, key, input.Channel, language,
            input.Title?.Trim(), input.Body, input.ProviderTemplateName?.Trim());

        await _templateRepository.InsertAsync(template);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ToDto(template);
    }

    public async Task<NotificationTemplateDto> UpdateAsync(Guid id, UpdateNotificationTemplateDto input)
    {
        var template = await GetTemplateAsync(id);
        template.Update(input.Title?.Trim(), input.Body, input.ProviderTemplateName?.Trim(), input.IsActive);
        await _templateRepository.UpdateAsync(template);
        await CurrentUnitOfWork.SaveChangesAsync();
        return ToDto(template);
    }

    public async Task DeleteAsync(Guid id)
    {
        var template = await GetTemplateAsync(id);
        await _templateRepository.DeleteAsync(template);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    public async Task<NotificationTemplateDto> SetApprovalStatusAsync(Guid id, SetTemplateApprovalDto input)
    {
        if (!Enum.IsDefined(typeof(TemplateApprovalStatus), input.ApprovalStatus))
            throw new UserFriendlyException("Unknown approval status.");

        var template = await GetTemplateAsync(id);
        template.SetApprovalStatus(input.ApprovalStatus);
        await _templateRepository.UpdateAsync(template);
        await CurrentUnitOfWork.SaveChangesAsync();
        return ToDto(template);
    }

    private async Task<NotificationTemplate> GetTemplateAsync(Guid id)
    {
        var template = await _templateRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);
        if (template == null)
            throw new UserFriendlyException("Template not found.");
        return template;
    }

    private static NotificationTemplateDto ToDto(NotificationTemplate t) => new NotificationTemplateDto
    {
        Id = t.Id,
        TemplateKey = t.TemplateKey,
        Channel = t.Channel,
        Language = t.Language,
        Title = t.Title,
        Body = t.Body,
        ProviderTemplateName = t.ProviderTemplateName,
        ApprovalStatus = t.ApprovalStatus,
        IsActive = t.IsActive,
        CreationTime = t.CreationTime
    };

    private static NotificationTemplateListDto ToListDto(NotificationTemplate t) => new NotificationTemplateListDto
    {
        Id = t.Id,
        TemplateKey = t.TemplateKey,
        Channel = t.Channel,
        Language = t.Language,
        Title = t.Title,
        ApprovalStatus = t.ApprovalStatus,
        IsActive = t.IsActive
    };
}
