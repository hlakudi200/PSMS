using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Communication.Entities
{
    /// <summary>
    /// Represents a notification sent to a user
    /// </summary>
    [Table("Notifications")]
    public class Notification : CreationAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxTitleLength = 200;
        public const int MaxMessageLength = 1000;
        public const int MaxActionUrlLength = 500;
        public const int MaxEntityTypeLength = 100;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// User who receives the notification
        /// </summary>
        [Required]
        public long UserId { get; set; }

        /// <summary>
        /// Title of the notification
        /// </summary>
        [Required]
        [StringLength(MaxTitleLength)]
        public string Title { get; set; }

        /// <summary>
        /// Message content
        /// </summary>
        [Required]
        [StringLength(MaxMessageLength)]
        public string Message { get; set; }

        /// <summary>
        /// Type of notification
        /// </summary>
        [Required]
        public NotificationType Type { get; set; }

        /// <summary>
        /// Priority level
        /// </summary>
        [Required]
        public NotificationPriority Priority { get; set; }

        /// <summary>
        /// URL to navigate to when notification is clicked
        /// </summary>
        [StringLength(MaxActionUrlLength)]
        public string ActionUrl { get; set; }

        /// <summary>
        /// Related entity type (e.g., "Assessment", "Payment")
        /// </summary>
        [StringLength(MaxEntityTypeLength)]
        public string EntityType { get; set; }

        /// <summary>
        /// Related entity ID
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Whether the notification has been read
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Date when the notification was read
        /// </summary>
        public DateTime? ReadDate { get; set; }

        /// <summary>
        /// Whether email was sent
        /// </summary>
        public bool EmailSent { get; set; }

        /// <summary>
        /// Date when email was sent
        /// </summary>
        public DateTime? EmailSentDate { get; set; }

        /// <summary>
        /// Whether push notification was sent
        /// </summary>
        public bool PushSent { get; set; }

        /// <summary>
        /// Date when push was sent
        /// </summary>
        public DateTime? PushSentDate { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Notification()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Notification(
            Guid id,
            int? tenantId,
            long userId,
            string title,
            string message,
            NotificationType type) : this()
        {
            Id = id;
            TenantId = tenantId;
            UserId = userId;
            Title = title;
            Message = message;
            Type = type;
            Priority = NotificationPriority.Normal;
            IsRead = false;
            EmailSent = false;
            PushSent = false;
        }

        /// <summary>
        /// Marks the notification as read
        /// </summary>
        public void MarkAsRead()
        {
            if (!IsRead)
            {
                IsRead = true;
                ReadDate = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Records that email was sent
        /// </summary>
        public void RecordEmailSent()
        {
            EmailSent = true;
            EmailSentDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Records that push notification was sent
        /// </summary>
        public void RecordPushSent()
        {
            PushSent = true;
            PushSentDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Links notification to a related entity
        /// </summary>
        public void LinkToEntity(string entityType, Guid entityId, string actionUrl = null)
        {
            EntityType = entityType;
            EntityId = entityId;
            ActionUrl = actionUrl;
        }
    }
}
