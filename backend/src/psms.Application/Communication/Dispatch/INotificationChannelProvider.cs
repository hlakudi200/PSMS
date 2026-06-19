using psms.Domain.Shared.Enums;
using System.Threading.Tasks;

namespace psms.Communication.Dispatch;

/// <summary>
/// COMM-01: one delivery channel. Adding a channel = implement this once; the
/// dispatcher discovers it automatically (providers are registered against this
/// interface in psmsApplicationModule), so call sites never change.
/// </summary>
public interface INotificationChannelProvider
{
    /// <summary>The channel this provider delivers on.</summary>
    NotificationChannel Channel { get; }

    /// <summary>Deliver the request to a single resolved recipient on this channel.</summary>
    Task<ChannelSendResult> SendAsync(NotificationRequest request, long recipientUserId);
}
