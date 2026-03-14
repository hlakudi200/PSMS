using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Communication.Notifications.Dto;
using psms.Communication.Shared;
using psms.Domain.Communication.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Communication.Notifications;

[AbpAuthorize(PermissionNames.Communication_Notifications)]
public class NotificationAppService : ApplicationService, INotificationAppService
{
    private readonly IRepository<Notification, Guid> _notificationRepository;

    public NotificationAppService(IRepository<Notification, Guid> notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    [AbpAuthorize(PermissionNames.Communication_Notifications_View)]
    public async Task<NotificationDto> GetAsync(Guid id)
    {
        var currentUserId = AbpSession.UserId.Value;

        var notification = await _notificationRepository
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == AbpSession.TenantId);

        if (notification == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.NotificationNotFound,
                "Notification not found.");

        if (notification.UserId != currentUserId)
            throw new UserFriendlyException(CommunicationExceptionCodes.NotificationNotForCurrentUser,
                "This notification does not belong to you.");

        return ObjectMapper.Map<NotificationDto>(notification);
    }

    [AbpAuthorize(PermissionNames.Communication_Notifications_View)]
    public async Task<PagedResultDto<NotificationListDto>> GetAllAsync(GetNotificationsInput input)
    {
        var currentUserId = AbpSession.UserId.Value;

        var query = _notificationRepository
            .GetAll()
            .Where(n => n.TenantId == AbpSession.TenantId && n.UserId == currentUserId)
            .WhereIf(input.Type.HasValue, n => n.Type == input.Type.Value)
            .WhereIf(input.Priority.HasValue, n => n.Priority == input.Priority.Value)
            .WhereIf(input.IsRead.HasValue, n => n.IsRead == input.IsRead.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                n => n.Title.ToLower().Contains(input.Search.Trim().ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                n => n.Title.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<NotificationListDto>(
            totalCount,
            ObjectMapper.Map<List<NotificationListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Communication_Notifications_View)]
    public async Task<int> GetUnreadCountAsync()
    {
        var currentUserId = AbpSession.UserId.Value;

        return await _notificationRepository
            .GetAll()
            .CountAsync(n => n.TenantId == AbpSession.TenantId
                && n.UserId == currentUserId
                && !n.IsRead);
    }

    [AbpAuthorize(PermissionNames.Communication_Notifications_View)]
    public async Task MarkAsReadAsync(Guid id)
    {
        var currentUserId = AbpSession.UserId.Value;

        var notification = await _notificationRepository
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == AbpSession.TenantId);

        if (notification == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.NotificationNotFound,
                "Notification not found.");

        if (notification.UserId != currentUserId)
            throw new UserFriendlyException(CommunicationExceptionCodes.NotificationNotForCurrentUser,
                "This notification does not belong to you.");

        notification.MarkAsRead();
        await _notificationRepository.UpdateAsync(notification);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Communication_Notifications_View)]
    public async Task MarkAllAsReadAsync()
    {
        var currentUserId = AbpSession.UserId.Value;

        var unreadNotifications = await _notificationRepository
            .GetAll()
            .Where(n => n.TenantId == AbpSession.TenantId
                && n.UserId == currentUserId
                && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
            await _notificationRepository.UpdateAsync(notification);
        }

        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Communication_Notifications_Configure)]
    public async Task<NotificationDto> CreateAsync(CreateNotificationDto input)
    {
        var notification = new Notification(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.UserId,
            input.Title.Trim(),
            input.Message.Trim(),
            input.Type)
        {
            Priority = input.Priority
        };

        if (!string.IsNullOrWhiteSpace(input.EntityType) && input.EntityId.HasValue)
        {
            notification.LinkToEntity(input.EntityType.Trim(), input.EntityId.Value, input.ActionUrl?.Trim());
        }
        else if (!string.IsNullOrWhiteSpace(input.ActionUrl))
        {
            notification.ActionUrl = input.ActionUrl.Trim();
        }

        await _notificationRepository.InsertAsync(notification);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<NotificationDto>(notification);
    }
}
