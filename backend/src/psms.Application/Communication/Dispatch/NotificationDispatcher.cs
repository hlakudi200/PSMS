using Abp.Dependency;
using Castle.Core.Logging;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Communication.Dispatch;

/// <summary>
/// COMM-01: default dispatcher. Discovers the registered channel providers, picks
/// the channels for this request, and delivers per recipient × channel. The
/// channel set is in-app only today; COMM-05 (preferences/consent) and COMM-11
/// (routing/fallback) replace <see cref="ResolveChannels"/> with real logic.
/// </summary>
public class NotificationDispatcher : INotificationDispatcher, ITransientDependency
{
    private readonly IIocResolver _iocResolver;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public NotificationDispatcher(IIocResolver iocResolver)
    {
        _iocResolver = iocResolver;
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
                    // Isolate each channel: a provider that throws (real channels hit
                    // networks/SDKs and will) must not abort delivery to the other
                    // recipients/channels. Convert the failure into a result instead.
                    try
                    {
                        result.Results.Add(await provider.SendAsync(request, recipientUserId));
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Notification channel '{provider.Channel}' failed for user {recipientUserId}: {ex.Message}", ex);
                        result.Results.Add(ChannelSendResult.Failed(provider.Channel, recipientUserId, ex.Message));
                    }
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
    /// COMM-01: in-app only. COMM-05 will narrow/expand this by the recipient's
    /// channel preferences + consent; COMM-11 will add priority-based routing and
    /// the fallback cascade.
    /// </summary>
    private static IReadOnlyCollection<NotificationChannel> ResolveChannels(NotificationRequest request)
        => new[] { NotificationChannel.InApp };
}
