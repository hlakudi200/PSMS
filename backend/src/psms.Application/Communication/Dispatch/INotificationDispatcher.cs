using System.Threading.Tasks;

namespace psms.Communication.Dispatch;

/// <summary>
/// COMM-01/05/11: the single entry point for raising notifications. A routing policy
/// resolves the channels (per the request's opted-in channels), each is consent/
/// preference-gated (COMM-05), and the request fans out — always-channels independently,
/// cascade-channels in order until one succeeds. Callers never reference a provider/SDK.
/// </summary>
public interface INotificationDispatcher
{
    /// <summary>Dispatch synchronously (in the caller's unit of work).</summary>
    Task<NotificationDispatchResult> DispatchAsync(NotificationRequest request);

    /// <summary>
    /// COMM-11: queue the request to a background job so a bulk/external send doesn't
    /// block the request. Per-channel failures are logged (not job-retried) — the
    /// cascade provides cross-channel resilience; set an IdempotencyKey to make any
    /// job-level retry safe. Use for fan-outs and slow external channels.
    /// </summary>
    Task EnqueueAsync(NotificationRequest request);
}
