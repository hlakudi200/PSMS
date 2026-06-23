using psms.Domain.Shared.Enums;
using System.Collections.Generic;

namespace psms.Communication.Dispatch.Routing;

/// <summary>
/// COMM-11: how a request fans out across channels.
/// - <see cref="AlwaysChannels"/> are each attempted independently (e.g. in-app the
///   inbox of record, and push — a free, non-exclusive nudge).
/// - <see cref="CascadeChannels"/> are tried in order and stop at the first success
///   (e.g. WhatsApp → SMS → Email: don't pay for SMS if WhatsApp delivered).
/// </summary>
public class RoutingPlan
{
    public IReadOnlyList<NotificationChannel> AlwaysChannels { get; }
    public IReadOnlyList<NotificationChannel> CascadeChannels { get; }

    public RoutingPlan(IReadOnlyList<NotificationChannel> alwaysChannels, IReadOnlyList<NotificationChannel> cascadeChannels)
    {
        AlwaysChannels = alwaysChannels ?? new List<NotificationChannel>();
        CascadeChannels = cascadeChannels ?? new List<NotificationChannel>();
    }
}
