using System.Collections.Generic;

namespace psms.Communication.Dispatch;

/// <summary>COMM-01: aggregate of every per-recipient, per-channel send attempt.</summary>
public class NotificationDispatchResult
{
    public List<ChannelSendResult> Results { get; set; } = new List<ChannelSendResult>();
}
