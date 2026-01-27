using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Communication.Entities
{
    /// <summary>
    /// Represents a school-wide or targeted announcement
    /// </summary>
    [Table("Announcements")]
    public class Announcement : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxTitleLength = 200;
        public const int MaxContentLength = 10000;
        public const int MaxAttachmentUrlLength = 500;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Title of the announcement
        /// </summary>
        [Required]
        [StringLength(MaxTitleLength)]
        public string Title { get; set; }

        /// <summary>
        /// Content of the announcement (HTML supported)
        /// </summary>
        [Required]
        [StringLength(MaxContentLength)]
        public string Content { get; set; }

        /// <summary>
        /// Type of announcement
        /// </summary>
        [Required]
        public AnnouncementType Type { get; set; }

        /// <summary>
        /// Priority level
        /// </summary>
        [Required]
        public AnnouncementPriority Priority { get; set; }

        /// <summary>
        /// Target audience
        /// </summary>
        [Required]
        public AnnouncementAudience TargetAudience { get; set; }

        /// <summary>
        /// Specific grade to target (if TargetAudience is Grade)
        /// </summary>
        public Guid? TargetGradeId { get; set; }

        /// <summary>
        /// Specific class to target (if TargetAudience is Class)
        /// </summary>
        public Guid? TargetClassId { get; set; }

        /// <summary>
        /// URL to attachment if any
        /// </summary>
        [StringLength(MaxAttachmentUrlLength)]
        public string AttachmentUrl { get; set; }

        /// <summary>
        /// When to publish the announcement
        /// </summary>
        [Required]
        public DateTime PublishDate { get; set; }

        /// <summary>
        /// When the announcement expires (no longer shown)
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Whether the announcement is published
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// Whether to pin at top of list
        /// </summary>
        public bool IsPinned { get; set; }

        /// <summary>
        /// Whether to send email notification
        /// </summary>
        public bool SendEmailNotification { get; set; }

        /// <summary>
        /// Whether to send push notification
        /// </summary>
        public bool SendPushNotification { get; set; }

        /// <summary>
        /// User who created the announcement
        /// </summary>
        [Required]
        public long CreatedByUserId { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Collections
        public virtual ICollection<AnnouncementRead> Reads { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Announcement()
        {
            Reads = new HashSet<AnnouncementRead>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Announcement(
            Guid id,
            int? tenantId,
            string title,
            string content,
            AnnouncementType type,
            AnnouncementAudience targetAudience,
            long createdByUserId) : this()
        {
            Id = id;
            TenantId = tenantId;
            Title = title;
            Content = content;
            Type = type;
            TargetAudience = targetAudience;
            CreatedByUserId = createdByUserId;
            Priority = AnnouncementPriority.Normal;
            PublishDate = DateTime.UtcNow;
            IsPublished = false;
            IsPinned = false;
            SendEmailNotification = false;
            SendPushNotification = false;
            IsDeleted = false;
        }

        /// <summary>
        /// Publishes the announcement
        /// </summary>
        public void Publish()
        {
            IsPublished = true;
            PublishDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Unpublishes the announcement
        /// </summary>
        public void Unpublish()
        {
            IsPublished = false;
        }

        /// <summary>
        /// Pins the announcement
        /// </summary>
        public void Pin()
        {
            IsPinned = true;
        }

        /// <summary>
        /// Unpins the announcement
        /// </summary>
        public void Unpin()
        {
            IsPinned = false;
        }

        /// <summary>
        /// Checks if the announcement is currently active
        /// </summary>
        public bool IsActive()
        {
            var now = DateTime.UtcNow;
            return IsPublished && PublishDate <= now && (!ExpiryDate.HasValue || ExpiryDate > now);
        }
    }
}
