using psms.Domain.Shared.Enums;

namespace psms.Communication.Dispatch;

/// <summary>
/// COMM-01: the outcome of sending one notification to one recipient on one
/// channel. <see cref="ReferenceId"/> holds a channel-specific handle — for the
/// in-app channel it's the created Notification row id; later channels store the
/// provider message id, which delivery webhooks (COMM-02/07) match against.
/// </summary>
public class ChannelSendResult
{
    public NotificationChannel Channel { get; set; }

    public long RecipientUserId { get; set; }

    public bool Success { get; set; }

    public string ReferenceId { get; set; }

    public string Error { get; set; }

    public static ChannelSendResult Ok(NotificationChannel channel, long recipientUserId, string referenceId = null)
        => new ChannelSendResult { Channel = channel, RecipientUserId = recipientUserId, Success = true, ReferenceId = referenceId };

    public static ChannelSendResult Failed(NotificationChannel channel, long recipientUserId, string error)
        => new ChannelSendResult { Channel = channel, RecipientUserId = recipientUserId, Success = false, Error = error };
}
