using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using psms.Communication.Dispatch.Jobs;
using psms.Communication.Dispatch.Routing;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Communication.Dispatch;

/// <summary>
/// COMM-01/02/05/11: the dispatcher. A routing policy (COMM-11) decides the channels;
/// per recipient, the "always" channels (in-app, push) each send independently and the
/// "cascade" channels (WhatsApp→SMS→Email) are tried in order until one succeeds.
/// Every attempt is consent/preference-gated (COMM-05, in-app exempt), idempotent
/// (COMM-02), and recorded in the delivery log (sent/delivered/failed/suppressed).
/// </summary>
public class NotificationDispatcher : INotificationDispatcher, ITransientDependency
{
    private enum SendOutcome { Succeeded, Failed, Suppressed, NoProvider }

    private readonly IIocResolver _iocResolver;
    private readonly IRepository<NotificationDeliveryLog, Guid> _deliveryLogRepository;
    private readonly IRepository<NotificationConsent, Guid> _consentRepository;
    private readonly IRepository<NotificationPreference, Guid> _preferenceRepository;
    private readonly INotificationRoutingPolicy _routingPolicy;
    private readonly IBackgroundJobManager _backgroundJobManager;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public NotificationDispatcher(
        IIocResolver iocResolver,
        IRepository<NotificationDeliveryLog, Guid> deliveryLogRepository,
        IRepository<NotificationConsent, Guid> consentRepository,
        IRepository<NotificationPreference, Guid> preferenceRepository,
        INotificationRoutingPolicy routingPolicy,
        IBackgroundJobManager backgroundJobManager)
    {
        _iocResolver = iocResolver;
        _deliveryLogRepository = deliveryLogRepository;
        _consentRepository = consentRepository;
        _preferenceRepository = preferenceRepository;
        _routingPolicy = routingPolicy;
        _backgroundJobManager = backgroundJobManager;
    }

    public async Task<NotificationDispatchResult> DispatchAsync(NotificationRequest request)
    {
        var result = new NotificationDispatchResult();
        if (request?.RecipientUserIds == null || !request.RecipientUserIds.Any())
            return result;

        var plan = _routingPolicy.Resolve(request);

        // ResolveAll returns transient instances we must release when done.
        var providers = _iocResolver.ResolveAll<INotificationChannelProvider>();
        try
        {
            var providerByChannel = providers.ToDictionary(p => p.Channel, p => p);

            foreach (var recipientUserId in request.RecipientUserIds.Distinct())
            {
                var gate = await LoadGateAsync(request, recipientUserId);

                // Always-channels: each sent independently (in-app record, push nudge).
                foreach (var channel in plan.AlwaysChannels)
                {
                    await TrySendAsync(channel, request, recipientUserId, providerByChannel, gate, result);
                }

                // Cascade: try in order, stop at the first successful send. NOTE:
                // "success" here is provider-ACCEPTED (external = Sent), not delivery-
                // CONFIRMED — true non-delivery escalation needs the delivery webhook
                // (COMM-14). So this is a send-failure cascade today, not a receipt one.
                foreach (var channel in plan.CascadeChannels)
                {
                    var outcome = await TrySendAsync(channel, request, recipientUserId, providerByChannel, gate, result);
                    if (outcome == SendOutcome.Succeeded)
                        break;
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

    public async Task EnqueueAsync(NotificationRequest request)
    {
        if (request?.RecipientUserIds == null || !request.RecipientUserIds.Any())
            return;

        await _backgroundJobManager.EnqueueAsync<NotificationDispatchJob, NotificationDispatchJobArgs>(
            new NotificationDispatchJobArgs
            {
                TenantId = request.TenantId,
                RecipientUserIds = request.RecipientUserIds.ToList(),
                Type = request.Type,
                Priority = request.Priority,
                Title = request.Title,
                Message = request.Message,
                ActionUrl = request.ActionUrl,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                IdempotencyKey = request.IdempotencyKey,
                TemplateKey = request.TemplateKey,
                Language = request.Language,
                Variables = request.Variables != null ? new Dictionary<string, string>(request.Variables) : null,
                RequestedChannels = request.RequestedChannels?.ToList()
            });
    }

    private async Task<SendOutcome> TrySendAsync(
        NotificationChannel channel,
        NotificationRequest request,
        long recipientUserId,
        IReadOnlyDictionary<NotificationChannel, INotificationChannelProvider> providerByChannel,
        RecipientGate gate,
        NotificationDispatchResult result)
    {
        // COMM-05 gate — in-app is the inbox of record and is exempt.
        if (channel != NotificationChannel.InApp)
        {
            if (gate.DisabledChannels.Contains(channel))
            {
                await WriteSuppressedLogAsync(request, recipientUserId, channel, $"Disabled by the recipient's preference for {request.Type}.");
                return SendOutcome.Suppressed;
            }
            if (!gate.GrantedChannels.Contains(channel))
            {
                await WriteSuppressedLogAsync(request, recipientUserId, channel, "No consent on file for this channel.");
                return SendOutcome.Suppressed;
            }
        }

        // Idempotency: an already-delivered channel for this key counts as success
        // (so the cascade stops) without re-sending.
        if (!string.IsNullOrEmpty(request.IdempotencyKey)
            && await AlreadyDeliveredAsync(request, channel, recipientUserId))
        {
            result.Results.Add(ChannelSendResult.Ok(channel, recipientUserId));
            return SendOutcome.Succeeded;
        }

        if (!providerByChannel.TryGetValue(channel, out var provider))
        {
            // A routed channel with no registered provider — a misconfiguration. Make
            // it visible rather than silently skipping (all 5 are registered today).
            Logger.Warn($"No provider registered for routed channel '{channel}'.");
            return SendOutcome.NoProvider;
        }

        ChannelSendResult sendResult;
        try
        {
            sendResult = await provider.SendAsync(request, recipientUserId);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Notification channel '{channel}' failed for user {recipientUserId}: {ex.Message}", ex);
            sendResult = ChannelSendResult.Failed(channel, recipientUserId, ex.Message);
        }

        await WriteDeliveryLogAsync(request, sendResult);
        result.Results.Add(sendResult);
        return sendResult.Success ? SendOutcome.Succeeded : SendOutcome.Failed;
    }

    private sealed class RecipientGate
    {
        public HashSet<NotificationChannel> DisabledChannels { get; set; } = new HashSet<NotificationChannel>();
        public HashSet<NotificationChannel> GrantedChannels { get; set; } = new HashSet<NotificationChannel>();
    }

    /// <summary>Load the recipient's per-category opt-outs + granted consents once per recipient.</summary>
    private async Task<RecipientGate> LoadGateAsync(NotificationRequest request, long userId)
    {
        var disabled = await _preferenceRepository.GetAll()
            .Where(p => p.UserId == userId && p.Category == request.Type && !p.IsEnabled)
            .Select(p => p.Channel)
            .ToListAsync();

        var granted = await _consentRepository.GetAll()
            .Where(c => c.UserId == userId && c.IsGranted)
            .Select(c => c.Channel)
            .ToListAsync();

        return new RecipientGate
        {
            DisabledChannels = disabled.ToHashSet(),
            GrantedChannels = granted.ToHashSet()
        };
    }

    private async Task WriteSuppressedLogAsync(NotificationRequest request, long recipientUserId, NotificationChannel channel, string reason)
    {
        var log = new NotificationDeliveryLog(Guid.NewGuid(), request.TenantId, channel, recipientUserId)
        {
            IdempotencyKey = request.IdempotencyKey
        };
        log.MarkSuppressed(reason);
        await _deliveryLogRepository.InsertAsync(log);
    }

    private async Task<bool> AlreadyDeliveredAsync(NotificationRequest request, NotificationChannel channel, long recipientUserId)
    {
        // The explicit TenantId predicate is load-bearing for the session-less
        // background-job path (COMM-11), where the ambient tenant filter is null.
        return await _deliveryLogRepository.GetAll().AnyAsync(l =>
            l.TenantId == request.TenantId
            && l.IdempotencyKey == request.IdempotencyKey
            && l.Channel == channel
            && l.RecipientUserId == recipientUserId
            && l.Status != NotificationDeliveryStatus.Failed
            && l.Status != NotificationDeliveryStatus.Suppressed);
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
            // External channels are Sent now; a provider webhook (COMM-14) moves
            // them to Delivered/Read.
            log.MarkSent(sendResult.ReferenceId);
        }

        await _deliveryLogRepository.InsertAsync(log);
    }
}
