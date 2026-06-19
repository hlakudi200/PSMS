namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// COMM-02: the delivery lifecycle of a notification on a single channel.
    /// In-app reaches Delivered immediately (it lands in the inbox); external
    /// channels move Sent → Delivered/Read via provider webhooks (COMM-07+), or
    /// to Failed. Pending is the initial state before the first attempt.
    /// </summary>
    public enum NotificationDeliveryStatus
    {
        Pending = 1,
        Sent = 2,
        Delivered = 3,
        Read = 4,
        Failed = 5,

        /// <summary>COMM-05: not sent on this channel — blocked by the recipient's
        /// preferences or missing consent. Recorded for audit, never retried.</summary>
        Suppressed = 6
    }
}
