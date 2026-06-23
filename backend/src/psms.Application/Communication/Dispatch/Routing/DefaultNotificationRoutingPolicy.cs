using psms.Domain.Shared.Enums;
using System.Collections.Generic;
using System.Linq;

namespace psms.Communication.Dispatch.Routing;

/// <summary>
/// COMM-11 default routing. The channel SET comes from the request's opt-in
/// (<see cref="NotificationRequest.RequestedChannels"/>) — defaulting to in-app only,
/// so a manual/admin notification never fans out to external channels unintentionally.
/// This policy only CLASSIFIES + ORDERS the opted-in channels:
///   - always: in-app (record), push (free nudge) — sent independently;
///   - cascade: WhatsApp → SMS → Email — tried in SA-value order, stop at first success.
/// Plain class (no marker) — registered explicitly in psmsApplicationModule.
/// </summary>
public class DefaultNotificationRoutingPolicy : INotificationRoutingPolicy
{
    // SA-value cascade order for the "reach the parent" channels.
    private static readonly NotificationChannel[] CascadeOrder =
        { NotificationChannel.WhatsApp, NotificationChannel.Sms, NotificationChannel.Email };

    private static readonly NotificationChannel[] AlwaysOrder =
        { NotificationChannel.InApp, NotificationChannel.Push };

    public RoutingPlan Resolve(NotificationRequest request)
    {
        var requested = (request?.RequestedChannels != null && request.RequestedChannels.Count > 0)
            ? request.RequestedChannels.Distinct().ToHashSet()
            : new HashSet<NotificationChannel> { NotificationChannel.InApp };

        var always = AlwaysOrder.Where(requested.Contains).ToList();
        var cascade = CascadeOrder.Where(requested.Contains).ToList();

        return new RoutingPlan(always, cascade);
    }
}
