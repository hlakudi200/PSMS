using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace psms.Domain.Communication.Entities
{
    /// <summary>
    /// Tracks which users have read an announcement
    /// </summary>
    [Table("AnnouncementReads")]
    public class AnnouncementRead : Entity<Guid>
    {
        /// <summary>
        /// Reference to the announcement
        /// </summary>
        [Required]
        public Guid AnnouncementId { get; set; }

        /// <summary>
        /// User who read the announcement
        /// </summary>
        [Required]
        public long UserId { get; set; }

        /// <summary>
        /// Date when the announcement was read
        /// </summary>
        [Required]
        public DateTime ReadDate { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(AnnouncementId))]
        public virtual Announcement Announcement { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected AnnouncementRead()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public AnnouncementRead(
            Guid id,
            Guid announcementId,
            long userId) : this()
        {
            Id = id;
            AnnouncementId = announcementId;
            UserId = userId;
            ReadDate = DateTime.UtcNow;
        }
    }
}
