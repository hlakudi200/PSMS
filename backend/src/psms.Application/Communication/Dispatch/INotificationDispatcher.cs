using System.Threading.Tasks;

namespace psms.Communication.Dispatch;

/// <summary>
/// COMM-01: the single entry point for raising notifications. Resolves the target
/// channels (in-app only for now — per-user preferences land in COMM-05, priority
/// routing + fallback cascade in COMM-11) and fans the request out to the matching
/// channel providers. Callers never reference a provider or channel SDK directly.
/// </summary>
public interface INotificationDispatcher
{
    Task<NotificationDispatchResult> DispatchAsync(NotificationRequest request);
}
