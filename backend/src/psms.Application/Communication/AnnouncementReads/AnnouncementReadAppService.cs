using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Communication.AnnouncementReads.Dto;
using psms.Communication.Shared;
using psms.Domain.Communication.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Communication.AnnouncementReads;

[AbpAuthorize(PermissionNames.Communication_Announcements)]
public class AnnouncementReadAppService : ApplicationService, IAnnouncementReadAppService
{
    private readonly IRepository<AnnouncementRead, Guid> _announcementReadRepository;
    private readonly IRepository<Announcement, Guid> _announcementRepository;

    public AnnouncementReadAppService(
        IRepository<AnnouncementRead, Guid> announcementReadRepository,
        IRepository<Announcement, Guid> announcementRepository)
    {
        _announcementReadRepository = announcementReadRepository;
        _announcementRepository = announcementRepository;
    }

    [AbpAuthorize(PermissionNames.Communication_Announcements_View)]
    public async Task MarkAsReadAsync(Guid announcementId)
    {
        // Verify announcement exists and belongs to tenant
        var announcementExists = await _announcementRepository
            .GetAll()
            .AnyAsync(a => a.Id == announcementId && a.TenantId == AbpSession.TenantId);

        if (!announcementExists)
            throw new UserFriendlyException(CommunicationExceptionCodes.AnnouncementNotFound,
                "Announcement not found.");

        var currentUserId = AbpSession.UserId.Value;

        // Check if already read
        var alreadyRead = await _announcementReadRepository
            .GetAll()
            .AnyAsync(ar => ar.AnnouncementId == announcementId
                && ar.UserId == currentUserId
                && ar.Announcement.TenantId == AbpSession.TenantId);

        if (alreadyRead)
            throw new UserFriendlyException(CommunicationExceptionCodes.AlreadyMarkedAsRead,
                "Announcement is already marked as read.");

        var read = new AnnouncementRead(
            Guid.NewGuid(),
            announcementId,
            currentUserId);

        await _announcementReadRepository.InsertAsync(read);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Communication_Announcements_Edit)]
    public async Task<ListResultDto<AnnouncementReadDto>> GetByAnnouncementAsync(Guid announcementId)
    {
        // Verify announcement exists and belongs to tenant
        var announcementExists = await _announcementRepository
            .GetAll()
            .AnyAsync(a => a.Id == announcementId && a.TenantId == AbpSession.TenantId);

        if (!announcementExists)
            throw new UserFriendlyException(CommunicationExceptionCodes.AnnouncementNotFound,
                "Announcement not found.");

        var reads = await _announcementReadRepository
            .GetAll()
            .Where(ar => ar.AnnouncementId == announcementId
                && ar.Announcement.TenantId == AbpSession.TenantId)
            .OrderByDescending(ar => ar.ReadDate)
            .ToListAsync();

        return new ListResultDto<AnnouncementReadDto>(
            ObjectMapper.Map<List<AnnouncementReadDto>>(reads));
    }

    [AbpAuthorize(PermissionNames.Communication_Announcements_View)]
    public async Task<int> GetUnreadCountAsync()
    {
        var currentUserId = AbpSession.UserId.Value;

        // Count published announcements for this tenant that the current user hasn't read
        var unreadCount = await _announcementRepository
            .GetAll()
            .Where(a => a.TenantId == AbpSession.TenantId
                && a.IsPublished
                && !a.Reads.Any(r => r.UserId == currentUserId))
            .CountAsync();

        return unreadCount;
    }
}
