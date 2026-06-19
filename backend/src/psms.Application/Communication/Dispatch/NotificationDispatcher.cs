using Abp.Dependency;
using Abp.Domain.Repositories;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Communication.Dispatch;

/// <summary>
/// COMM-01/02: default dispatcher. Discovers the registered channel providers,
/// picks the channels for this request, delivers per recipient × channel, and
/// records every attempt in the delivery log (COMM-02). The channel set is in-app
/// only today; COMM-05 (preferences/consent) and COMM-11 (routing/fallback) replace
/// <see cref="ResolveChannels"/> with real logic.
/// </summary>
public class NotificationDispatcher : INotificationDispatcher, ITransientDependency
{
    private readonly IIocResolver _iocResolver;
    private readonly IRepository<NotificationDeliveryLog, Guid> _deliveryLogRepository;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public NotificationDispatcher(
        IIocResolver iocResolver,
        IRepository<NotificationDeliveryLog, Guid> deliveryLogRepository)
    {
        _iocResolver = iocResolver;
        _deliveryLogRepository = deliveryLogRepository;
    }

    public async Task<NotificationDispatchResult> DispatchAsync(NotificationRequest request)
    {
        var result = new NotificationDispatchResult();
        if (request?.RecipientUserIds == null || !request.RecipientUserIds.Any())
            return result;

        var channels = ResolveChannels(request);

        // ResolveAll returns transient instances we must release when done.
        var providers = _iocResolver.ResolveAll<INotificationChannelProvider>();
        try
        {
            var selected = providers.Where(p => channels.Contains(p.Channel)).ToList();

            foreach (var recipientUserId in request.RecipientUserIds.Distinct())
            {
                foreach (var provider in selected)
                {
                    // Idempotency: skip a channel for a recipient that was already
                    // delivered for this key (guards retries / duplicate triggers).
                    if (!string.IsNullOrEmpty(request.IdempotencyKey)
                        && await AlreadyDeliveredAsync(request, provider.Channel, recipientUserId))
                    {
                        result.Results.Add(ChannelSendResult.Ok(provider.Channel, recipientUserId));
                        continue;
                    }

                    ChannelSendResult sendResult;
                    try
                    {
                        // Isolate each channel: a provider that throws (real channels
                        // hit networks/SDKs and will) must not abort delivery to the
                        // other recipients/channels — record the failure instead.
                        sendResult = await provider.SendAsync(request, recipientUserId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Notification channel '{provider.Channel}' failed for user {recipientUserId}: {ex.Message}", ex);
                        sendResult = ChannelSendResult.Failed(provider.Channel, recipientUserId, ex.Message);
                    }

                    await WriteDeliveryLogAsync(request, sendResult);
                    result.Results.Add(sendResult);
                }
            }
        }
        finally
        {
            foreach (var provider in providers)
            {
                _iocResolver.Release(provider);
            }
        }

        return result;
    }

    private async Task<bool> AlreadyDeliveredAsync(NotificationRequest request, NotificationChannel channel, long recipientUserId)
    {
        // The explicit TenantId predicate is load-bearing, not redundant: in the
        // request path ABP's IMayHaveTenant filter already scopes by the ambient
        // tenant, but when this runs from a session-less background job (COMM-07)
        // the ambient filter resolves to null-tenant — then this predicate is the
        // only thing scoping the dedup check to the right tenant. Keep it.
        return await _deliveryLogRepository.GetAll().AnyAsync(l =>
            l.TenantId == request.TenantId
            && l.IdempotencyKey == request.IdempotencyKey
            && l.Channel == channel
            && l.RecipientUserId == recipientUserId
            && l.Status != NotificationDeliveryStatus.Failed);
    }

    private async Task WriteDeliveryLogAsync(NotificationRequest request, ChannelSendResult sendResult)
    {
        var log = new NotificationDeliveryLog(
            Guid.NewGuid(),
            request.TenantId,
            sendResult.Channel,
            sendResult.RecipientUserId)
        {
            IdempotencyKey = request.IdempotencyKey,
            ReferenceId = sendResult.ReferenceId
        };

        // Link the in-app row so a webhook / read-sync can find it later.
        if (sendResult.Channel == NotificationChannel.InApp
            && Guid.TryParse(sendResult.ReferenceId, out var notificationId))
        {
            log.NotificationId = notificationId;
        }

        if (!sendResult.Success)
        {
            log.MarkFailed(sendResult.Error);
        }
        else if (sendResult.Channel == NotificationChannel.InApp)
        {
            // In-app lands directly in the inbox → Delivered on send.
            log.MarkDelivered(sendResult.ReferenceId);
        }
        else
        {
            // External channels are Sent now; a provider webhook (COMM-07+) moves
            // them to Delivered/Read.
            log.MarkSent(sendResult.ReferenceId);
        }

        await _deliveryLogRepository.InsertAsync(log);
    }

    /// <summary>
    /// COMM-01: in-app only. COMM-05 will narrow/expand this by the recipient's
    /// channel preferences + consent; COMM-11 will add priority-based routing and
    /// the fallback cascade.
    /// </summary>
    private static IReadOnlyCollection<NotificationChannel> ResolveChannels(NotificationRequest request)
        => new[] { NotificationChannel.InApp };
}
