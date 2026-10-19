using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Communication.Announcements.Dto;
using psms.Communication.Shared;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Communication.Announcements;

[AbpAuthorize(PermissionNames.Communication_Announcements)]
public class AnnouncementAppService : ApplicationService, IAnnouncementAppService
{
    private readonly IRepository<Announcement, Guid> _announcementRepository;
    private readonly IFamilyAnnouncementScope _familyScope;

    public AnnouncementAppService(
        IRepository<Announcement, Guid> announcementRepository,
        IFamilyAnnouncementScope familyScope)
    {
        _announcementRepository = announcementRepository;
        _familyScope = familyScope;
    }

    [AbpAuthorize(PermissionNames.Communication_Announcements_View)]
    public async Task<AnnouncementDto> GetAsync(Guid id)
    {
        // #203: a parent or learner reads only what is addressed to them.
        var audience = await _familyScope.ResolveAsync();

        var announcement = await _announcementRepository
            .GetAll()
            .Include(a => a.Reads)
            .Where(a => a.TenantId == AbpSession.TenantId)
            .VisibleTo(audience)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (announcement == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.AnnouncementNotFound,
                "Announcement not found.");

        var dto = ObjectMapper.Map<AnnouncementDto>(announcement);
        dto.IsReadByMe = announcement.Reads.Any(r => r.UserId == AbpSession.UserId);
        return dto;
    }

    [AbpAuthorize(PermissionNames.Communication_Announcements_View)]
    public async Task<PagedResultDto<AnnouncementListDto>> GetAllAsync(GetAnnouncementsInput input)
    {
        // #203: a parent or learner lists only what is addressed to them.
        var audience = await _familyScope.ResolveAsync();

        var query = _announcementRepository
            .GetAll()
            .Include(a => a.Reads)
            .Where(a => a.TenantId == AbpSession.TenantId)
            .VisibleTo(audience)
            .WhereIf(input.Type.HasValue, a => a.Type == input.Type.Value)
            .WhereIf(input.Priority.HasValue, a => a.Priority == input.Priority.Value)
            .WhereIf(input.TargetAudience.HasValue, a => a.TargetAudience == input.TargetAudience.Value)
            .WhereIf(input.IsPublished.HasValue, a => a.IsPublished == input.IsPublished.Value)
            .WhereIf(input.IsPinned.HasValue, a => a.IsPinned == input.IsPinned.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                a => a.Title.ToLower().Contains(input.Search.Trim().ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                a => a.Title.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "PublishDate DESC")
            .PageBy(input)
            .ToListAsync();

        var dtos = ObjectMapper.Map<List<AnnouncementListDto>>(items);
        foreach (var (dto, item) in dtos.Zip(items))
            dto.IsReadByMe = item.Reads.Any(r => r.UserId == AbpSession.UserId);

        return new PagedResultDto<AnnouncementListDto>(totalCount, dtos);
    }

    [AbpAuthorize(PermissionNames.Communication_Announcements_Create)]
    public async Task<AnnouncementDto> CreateAsync(CreateAnnouncementDto input)
    {
        // School-wide requires special permission
        if (input.TargetAudience == AnnouncementAudience.All)
        {
            PermissionChecker.Authorize(PermissionNames.Communication_Announcements_SendSchoolWide);
        }

        // TargetGradeId required when audience is Grade
        if (input.TargetAudience == AnnouncementAudience.Grade && !input.TargetGradeId.HasValue)
            throw new UserFriendlyException(CommunicationExceptionCodes.TargetGradeRequired,
                "Target grade is required when audience is Grade.");

        // TargetClassId required when audience is Class
        if (input.TargetAudience == AnnouncementAudience.Class && !input.TargetClassId.HasValue)
            throw new UserFriendlyException(CommunicationExceptionCodes.TargetClassRequired,
                "Target class is required when audience is Class.");

        // Validate ExpiryDate after PublishDate
        var publishDate = input.PublishDate ?? DateTime.UtcNow;
        if (input.ExpiryDate.HasValue && input.ExpiryDate.Value <= publishDate)
            throw new UserFriendlyException(CommunicationExceptionCodes.ExpiryDateBeforePublishDate,
                "Expiry date must be after publish date.");

        var announcement = new Announcement(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.Title.Trim(),
            input.Content.Trim(),
            input.Type,
            input.TargetAudience,
            AbpSession.UserId.Value)
        {
            Priority = input.Priority,
            TargetGradeId = input.TargetGradeId,
            TargetClassId = input.TargetClassId,
            AttachmentUrl = input.AttachmentUrl?.Trim(),
            PublishDate = publishDate,
            ExpiryDate = input.ExpiryDate,
            SendEmailNotification = input.SendEmailNotification,
            SendPushNotification = input.SendPushNotification
        };

        await _announcementRepository.InsertAsync(announcement);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(announcement.Id);
    }

    [AbpAuthorize(PermissionNames.Communication_Announcements_Edit)]
    public async Task<AnnouncementDto> UpdateAsync(Guid id, UpdateAnnouncementDto input)
    {
        var announcement = await _announcementRepository
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (announcement == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.AnnouncementNotFound,
                "Announcement not found.");

        // Check audience-specific requirements if audience is changing
        var newAudience = input.TargetAudience ?? announcement.TargetAudience;
        if (newAudience == AnnouncementAudience.All && input.TargetAudience.HasValue)
        {
            PermissionChecker.Authorize(PermissionNames.Communication_Announcements_SendSchoolWide);
        }

        if (newAudience == AnnouncementAudience.Grade
            && !input.TargetGradeId.HasValue && !announcement.TargetGradeId.HasValue)
            throw new UserFriendlyException(CommunicationExceptionCodes.TargetGradeRequired,
                "Target grade is required when audience is Grade.");

        if (newAudience == AnnouncementAudience.Class
            && !input.TargetClassId.HasValue && !announcement.TargetClassId.HasValue)
            throw new UserFriendlyException(CommunicationExceptionCodes.TargetClassRequired,
                "Target class is required when audience is Class.");

        // Validate ExpiryDate after PublishDate
        var newPublishDate = input.PublishDate ?? announcement.PublishDate;
        var newExpiryDate = input.ExpiryDate ?? announcement.ExpiryDate;
        if (newExpiryDate.HasValue && newExpiryDate.Value <= newPublishDate)
            throw new UserFriendlyException(CommunicationExceptionCodes.ExpiryDateBeforePublishDate,
                "Expiry date must be after publish date.");

        if (input.Title != null) announcement.Title = input.Title.Trim();
        if (input.Content != null) announcement.Content = input.Content.Trim();
        if (input.Type.HasValue) announcement.Type = input.Type.Value;
        if (input.Priority.HasValue) announcement.Priority = input.Priority.Value;
        if (input.TargetAudience.HasValue)
        {
            announcement.TargetAudience = input.TargetAudience.Value;
            // Clear stale FK fields when audience changes
            if (newAudience != AnnouncementAudience.Grade) announcement.TargetGradeId = null;
            if (newAudience != AnnouncementAudience.Class) announcement.TargetClassId = null;
        }
        if (input.TargetGradeId.HasValue) announcement.TargetGradeId = input.TargetGradeId;
        if (input.TargetClassId.HasValue) announcement.TargetClassId = input.TargetClassId;
        if (input.AttachmentUrl != null) announcement.AttachmentUrl = input.AttachmentUrl.Trim();
        if (input.PublishDate.HasValue) announcement.PublishDate = input.PublishDate.Value;
        if (input.ClearExpiryDate == true) announcement.ExpiryDate = null;
        else if (input.ExpiryDate.HasValue) announcement.ExpiryDate = input.ExpiryDate;
        if (input.SendEmailNotification.HasValue) announcement.SendEmailNotification = input.SendEmailNotification.Value;
        if (input.SendPushNotification.HasValue) announcement.SendPushNotification = input.SendPushNotification.Value;

        await _announcementRepository.UpdateAsync(announcement);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Communication_Announcements_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var announcement = await _announcementRepository
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (announcement == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.AnnouncementNotFound,
                "Announcement not found.");

        await _announcementRepository.DeleteAsync(announcement);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Communication_Announcements_Edit)]
    public async Task<AnnouncementDto> PublishAsync(Guid id)
    {
        var announcement = await _announcementRepository
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (announcement == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.AnnouncementNotFound,
                "Announcement not found.");

        if (announcement.IsPublished)
            throw new UserFriendlyException(CommunicationExceptionCodes.AnnouncementAlreadyPublished,
                "Announcement is already published.");

        announcement.Publish();
        await _announcementRepository.UpdateAsync(announcement);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Communication_Announcements_Edit)]
    public async Task<AnnouncementDto> UnpublishAsync(Guid id)
    {
        var announcement = await _announcementRepository
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (announcement == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.AnnouncementNotFound,
                "Announcement not found.");

        if (!announcement.IsPublished)
            throw new UserFriendlyException(CommunicationExceptionCodes.AnnouncementNotPublished,
                "Announcement is not published.");

        announcement.Unpublish();
        await _announcementRepository.UpdateAsync(announcement);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Communication_Announcements_Edit)]
    public async Task<AnnouncementDto> PinAsync(Guid id)
    {
        var announcement = await _announcementRepository
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (announcement == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.AnnouncementNotFound,
                "Announcement not found.");

        announcement.Pin();
        await _announcementRepository.UpdateAsync(announcement);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Communication_Announcements_Edit)]
    public async Task<AnnouncementDto> UnpinAsync(Guid id)
    {
        var announcement = await _announcementRepository
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (announcement == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.AnnouncementNotFound,
                "Announcement not found.");

        announcement.Unpin();
        await _announcementRepository.UpdateAsync(announcement);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
