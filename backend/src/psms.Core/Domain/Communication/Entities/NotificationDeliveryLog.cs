using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Communication.Entities
{
    /// <summary>
    /// COMM-02: one record per (notification × channel × recipient) send attempt,
    /// with a delivery-status lifecycle. This is the uniform cross-channel delivery
    /// record that provider webhooks (COMM-07), the fallback cascade (COMM-11) and
    /// analytics (COMM-12) build on. For the in-app channel it complements the
    /// Notification row; for external channels it is the only delivery record.
    /// </summary>
    [Table("NotificationDeliveryLogs")]
    public class NotificationDeliveryLog : CreationAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxReferenceIdLength = 256;
        public const int MaxErrorLength = 1000;
        public const int MaxIdempotencyKeyLength = 200;

        public int? TenantId { get; set; }

        /// <summary>The in-app Notification row this delivery relates to (channel = InApp). Null for external-only sends.</summary>
        public Guid? NotificationId { get; set; }

        public NotificationChannel Channel { get; set; }

        public long RecipientUserId { get; set; }

        public NotificationDeliveryStatus Status { get; set; }

        /// <summary>Channel handle — in-app: the Notification id; external: the provider message id (matched by delivery webhooks).</summary>
        [StringLength(MaxReferenceIdLength)]
        public string ReferenceId { get; set; }

        [StringLength(MaxErrorLength)]
        public string Error { get; set; }

        /// <summary>Optional dedup key — a non-failed row for the same (key, channel, recipient) means already delivered.</summary>
        [StringLength(MaxIdempotencyKeyLength)]
        public string IdempotencyKey { get; set; }

        /// <summary>
        /// Attempts for THIS row. Today each dispatch inserts a fresh row, so this is
        /// always 1 after the first attempt; it becomes a real retry counter once
        /// COMM-07 reuses a row across queued retries. Don't read it as a retry metric yet.
        /// </summary>
        public int AttemptCount { get; set; }

        public DateTime? SentDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        public DateTime? ReadDate { get; set; }

        public DateTime? FailedDate { get; set; }

        protected NotificationDeliveryLog()
        {
        }

        public NotificationDeliveryLog(Guid id, int? tenantId, NotificationChannel channel, long recipientUserId)
        {
            Id = id;
            TenantId = tenantId;
            Channel = channel;
            RecipientUserId = recipientUserId;
            Status = NotificationDeliveryStatus.Pending;
            AttemptCount = 0;
        }

        public void MarkSent(string referenceId = null)
        {
            Status = NotificationDeliveryStatus.Sent;
            SentDate = DateTime.UtcNow;
            AttemptCount++;
            if (referenceId != null) ReferenceId = referenceId;
        }

        /// <summary>In-app lands directly in the inbox, so it is Delivered on send.</summary>
        public void MarkDelivered(string referenceId = null)
        {
            Status = NotificationDeliveryStatus.Delivered;
            DeliveredDate = DateTime.UtcNow;
            if (SentDate == null) SentDate = DeliveredDate;
            if (AttemptCount == 0) AttemptCount = 1;
            if (referenceId != null) ReferenceId = referenceId;
        }

        /// <summary>
        /// Set by provider read-receipts (COMM-07+). NOTE: in-app read state is NOT
        /// synced here yet — it lives on the Notification row (IsRead/ReadDate). So for
        /// the in-app channel a log row stays Delivered; analytics (COMM-12) must read
        /// in-app "read" from the Notification, not from this column.
        /// </summary>
        public void MarkRead()
        {
            Status = NotificationDeliveryStatus.Read;
            ReadDate = DateTime.UtcNow;
        }

        public void MarkFailed(string error)
        {
            Status = NotificationDeliveryStatus.Failed;
            FailedDate = DateTime.UtcNow;
            AttemptCount++;
            Error = error;
        }

        /// <summary>COMM-05: channel was not attempted — blocked by preference/consent.</summary>
        public void MarkSuppressed(string reason)
        {
            Status = NotificationDeliveryStatus.Suppressed;
            Error = reason;
        }
    }
}
