using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Communication.Dispatch;
using psms.Communication.Notifications.Dto;
using psms.Communication.Shared;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
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
    private readonly INotificationDispatcher _dispatcher;

    public NotificationAppService(
        IRepository<Notification, Guid> notificationRepository,
        INotificationDispatcher dispatcher)
    {
        _notificationRepository = notificationRepository;
        _dispatcher = dispatcher;
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
        // COMM-01: route through the channel-agnostic dispatcher instead of writing
        // the Notification row directly. Today that resolves to the in-app channel
        // only, so behaviour is unchanged; later tickets add channels/preferences
        // behind the same call.
        var request = new NotificationRequest
        {
            TenantId = AbpSession.TenantId,
            RecipientUserIds = new List<long> { input.UserId },
            Type = input.Type,
            Priority = input.Priority,
            Title = input.Title,
            Message = input.Message,
            ActionUrl = input.ActionUrl,
            EntityType = input.EntityType,
            EntityId = input.EntityId
        };

        var dispatch = await _dispatcher.DispatchAsync(request);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Preserve the existing contract: return the created in-app notification.
        var inAppResult = dispatch.Results.FirstOrDefault(r =>
            r.Channel == NotificationChannel.InApp && r.Success && r.RecipientUserId == input.UserId);

        if (inAppResult == null || !Guid.TryParse(inAppResult.ReferenceId, out var notificationId))
        {
            throw new UserFriendlyException(CommunicationExceptionCodes.NotificationDispatchFailed,
                "Notification could not be created.");
        }

        var notification = await _notificationRepository.GetAsync(notificationId);
        return ObjectMapper.Map<NotificationDto>(notification);
    }
}
