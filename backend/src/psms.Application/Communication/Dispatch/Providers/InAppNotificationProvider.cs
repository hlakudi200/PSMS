using Abp.Domain.Repositories;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Threading.Tasks;

namespace psms.Communication.Dispatch.Providers;

/// <summary>
/// COMM-01: the in-app channel — persists a <see cref="Notification"/> row the
/// user reads in the app. Mirrors the behaviour the old NotificationAppService
/// .CreateAsync had inline. Real-time push of this row to the client is COMM-03.
/// Registered against <see cref="INotificationChannelProvider"/> in
/// psmsApplicationModule (not via a marker interface), so it is not double-registered.
/// </summary>
public class InAppNotificationProvider : INotificationChannelProvider
{
    private readonly IRepository<Notification, Guid> _notificationRepository;

    public InAppNotificationProvider(IRepository<Notification, Guid> notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public NotificationChannel Channel => NotificationChannel.InApp;

    public async Task<ChannelSendResult> SendAsync(NotificationRequest request, long recipientUserId)
    {
        var notification = new Notification(
            Guid.NewGuid(),
            request.TenantId,
            recipientUserId,
            (request.Title ?? string.Empty).Trim(),
            (request.Message ?? string.Empty).Trim(),
            request.Type)
        {
            Priority = request.Priority
        };

        if (!string.IsNullOrWhiteSpace(request.EntityType) && request.EntityId.HasValue)
        {
            notification.LinkToEntity(request.EntityType.Trim(), request.EntityId.Value, request.ActionUrl?.Trim());
        }
        else if (!string.IsNullOrWhiteSpace(request.ActionUrl))
        {
            notification.ActionUrl = request.ActionUrl.Trim();
        }

        await _notificationRepository.InsertAsync(notification);

        return ChannelSendResult.Ok(Channel, recipientUserId, notification.Id.ToString());
    }
}
