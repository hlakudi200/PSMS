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
    /// Represents a direct message between users
    /// </summary>
    [Table("Messages")]
    public class Message : CreationAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxSubjectLength = 200;
        public const int MaxContentLength = 10000;
        public const int MaxAttachmentUrlLength = 500;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Sender user ID
        /// </summary>
        [Required]
        public long SenderUserId { get; set; }

        /// <summary>
        /// Recipient user ID
        /// </summary>
        [Required]
        public long RecipientUserId { get; set; }

        /// <summary>
        /// Subject of the message
        /// </summary>
        [StringLength(MaxSubjectLength)]
        public string Subject { get; set; }

        /// <summary>
        /// Content of the message
        /// </summary>
        [Required]
        [StringLength(MaxContentLength)]
        public string Content { get; set; }

        /// <summary>
        /// URL to attachment if any
        /// </summary>
        [StringLength(MaxAttachmentUrlLength)]
        public string AttachmentUrl { get; set; }

        /// <summary>
        /// Whether the message has been read
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Date when the message was read
        /// </summary>
        public DateTime? ReadDate { get; set; }

        /// <summary>
        /// Whether the sender deleted this message
        /// </summary>
        public bool IsDeletedBySender { get; set; }

        /// <summary>
        /// Whether the recipient deleted this message
        /// </summary>
        public bool IsDeletedByRecipient { get; set; }

        /// <summary>
        /// Parent message ID for replies (thread)
        /// </summary>
        public Guid? ParentMessageId { get; set; }

        /// <summary>
        /// Conversation thread ID
        /// </summary>
        public Guid? ThreadId { get; set; }

        /// <summary>
        /// Soft delete flag (for hard delete scenarios)
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ParentMessageId))]
        public virtual Message ParentMessage { get; set; }

        public virtual ICollection<Message> Replies { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Message()
        {
            Replies = new HashSet<Message>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Message(
            Guid id,
            int? tenantId,
            long senderUserId,
            long recipientUserId,
            string content,
            string subject = null,
            Guid? parentMessageId = null) : this()
        {
            Id = id;
            TenantId = tenantId;
            SenderUserId = senderUserId;
            RecipientUserId = recipientUserId;
            Content = content;
            Subject = subject;
            ParentMessageId = parentMessageId;
            IsRead = false;
            IsDeletedBySender = false;
            IsDeletedByRecipient = false;
            IsDeleted = false;

            // Set thread ID - use parent's thread or create new
            ThreadId = parentMessageId ?? id;
        }

        /// <summary>
        /// Marks the message as read
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
        /// Deletes message for sender
        /// </summary>
        public void DeleteForSender()
        {
            IsDeletedBySender = true;
        }

        /// <summary>
        /// Deletes message for recipient
        /// </summary>
        public void DeleteForRecipient()
        {
            IsDeletedByRecipient = true;
        }

        /// <summary>
        /// Checks if message is visible to a user
        /// </summary>
        public bool IsVisibleToUser(long userId)
        {
            if (IsDeleted) return false;

            if (userId == SenderUserId)
                return !IsDeletedBySender;

            if (userId == RecipientUserId)
                return !IsDeletedByRecipient;

            return false;
        }
    }
}
