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
/// COMM-01/02/05: default dispatcher. Discovers the registered channel providers,
/// resolves which channels are allowed for each recipient (preferences + consent),
/// delivers, and records every attempt — including suppressions — in the delivery
/// log. The candidate channel set is in-app only today; COMM-11 (routing/fallback)
/// expands <see cref="CandidateChannels"/>.
/// </summary>
public class NotificationDispatcher : INotificationDispatcher, ITransientDependency
{
    private readonly IIocResolver _iocResolver;
    private readonly IRepository<NotificationDeliveryLog, Guid> _deliveryLogRepository;
    private readonly IRepository<NotificationConsent, Guid> _consentRepository;
    private readonly IRepository<NotificationPreference, Guid> _preferenceRepository;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public NotificationDispatcher(
        IIocResolver iocResolver,
        IRepository<NotificationDeliveryLog, Guid> deliveryLogRepository,
        IRepository<NotificationConsent, Guid> consentRepository,
        IRepository<NotificationPreference, Guid> preferenceRepository)
    {
        _iocResolver = iocResolver;
        _deliveryLogRepository = deliveryLogRepository;
        _consentRepository = consentRepository;
        _preferenceRepository = preferenceRepository;
    }

    public async Task<NotificationDispatchResult> DispatchAsync(NotificationRequest request)
    {
        var result = new NotificationDispatchResult();
        if (request?.RecipientUserIds == null || !request.RecipientUserIds.Any())
            return result;

        var candidates = CandidateChannels(request);

        // ResolveAll returns transient instances we must release when done.
        var providers = _iocResolver.ResolveAll<INotificationChannelProvider>();
        try
        {
            foreach (var recipientUserId in request.RecipientUserIds.Distinct())
            {
                // COMM-05: filter the candidate channels by the recipient's
                // preferences + consent; suppressed channels are logged for audit.
                var (allowed, suppressed) = await ResolveChannelsForRecipientAsync(request, recipientUserId, candidates);

                foreach (var (channel, reason) in suppressed)
                {
                    await WriteSuppressedLogAsync(request, recipientUserId, channel, reason);
                }

                foreach (var provider in providers.Where(p => allowed.Contains(p.Channel)))
                {
                    // Idempotency: skip a channel for a recipient already delivered
                    // for this key (guards retries / duplicate triggers).
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

    /// <summary>
    /// COMM-05: which of the candidate channels may be used for this recipient.
    /// In-app is the inbox of record — always delivered, never gated. External
    /// channels require a granted consent (POPIA) and must not be disabled for the
    /// notification's category by the user's preferences.
    /// </summary>
    private async Task<(List<NotificationChannel> allowed, List<(NotificationChannel channel, string reason)> suppressed)>
        ResolveChannelsForRecipientAsync(NotificationRequest request, long userId, IReadOnlyCollection<NotificationChannel> candidates)
    {
        var allowed = new List<NotificationChannel>();
        var suppressed = new List<(NotificationChannel, string)>();

        var externalCandidates = candidates.Where(c => c != NotificationChannel.InApp).ToList();

        // Explicit per-category opt-outs and granted consents — only needed for the
        // external candidates (in-app is exempt). One query each, evaluated in memory.
        List<NotificationChannel> disabledChannels = new List<NotificationChannel>();
        List<NotificationChannel> grantedChannels = new List<NotificationChannel>();
        if (externalCandidates.Count > 0)
        {
            disabledChannels = await _preferenceRepository.GetAll()
                .Where(p => p.UserId == userId && p.Category == request.Type && !p.IsEnabled)
                .Select(p => p.Channel)
                .ToListAsync();

            grantedChannels = await _consentRepository.GetAll()
                .Where(c => c.UserId == userId && c.IsGranted)
                .Select(c => c.Channel)
                .ToListAsync();
        }

        foreach (var channel in candidates)
        {
            if (channel == NotificationChannel.InApp)
            {
                allowed.Add(channel);
                continue;
            }

            if (disabledChannels.Contains(channel))
            {
                suppressed.Add((channel, $"Disabled by the recipient's preference for {request.Type}."));
                continue;
            }

            if (!grantedChannels.Contains(channel))
            {
                suppressed.Add((channel, "No consent on file for this channel."));
                continue;
            }

            allowed.Add(channel);
        }

        return (allowed, suppressed);
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
    /// COMM-01: in-app only. COMM-11 will add priority-based routing and the
    /// fallback cascade that surface the external channels here.
    /// </summary>
    private static IReadOnlyCollection<NotificationChannel> CandidateChannels(NotificationRequest request)
        => new[] { NotificationChannel.InApp };
}
