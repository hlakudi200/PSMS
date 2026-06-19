using Abp.Domain.Repositories;
using psms.Communication.Templates;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
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
    private readonly INotificationTemplateRenderer _templateRenderer;

    public InAppNotificationProvider(
        IRepository<Notification, Guid> notificationRepository,
        INotificationTemplateRenderer templateRenderer)
    {
        _notificationRepository = notificationRepository;
        _templateRenderer = templateRenderer;
    }

    public NotificationChannel Channel => NotificationChannel.InApp;

    public async Task<ChannelSendResult> SendAsync(NotificationRequest request, long recipientUserId)
    {
        // COMM-06: if the request names a template, render it for in-app; otherwise
        // (and if the template isn't found) use the literal Title/Message.
        var title = request.Title;
        var message = request.Message;
        if (!string.IsNullOrWhiteSpace(request.TemplateKey))
        {
            var vars = request.Variables != null ? new Dictionary<string, string>(request.Variables) : null;
            var rendered = await _templateRenderer.RenderAsync(
                request.TemplateKey, NotificationChannel.InApp, request.Language, vars);
            if (rendered.Found)
            {
                title = rendered.Title;
                message = rendered.Body;
            }
        }

        var notification = new Notification(
            Guid.NewGuid(),
            request.TenantId,
            recipientUserId,
            (title ?? string.Empty).Trim(),
            (message ?? string.Empty).Trim(),
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
