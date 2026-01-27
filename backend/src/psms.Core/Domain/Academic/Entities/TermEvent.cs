using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents an event in a school term
    /// </summary>
    [Table("TermEvents")]
    public class TermEvent : CreationAuditedEntity<Guid>
    {
        public const int MaxEventNameLength = 200;
        public const int MaxDescriptionLength = 1000;

        /// <summary>
        /// Term this event belongs to
        /// </summary>
        [Required]
        public Guid TermId { get; set; }

        /// <summary>
        /// Event name
        /// </summary>
        [Required]
        [StringLength(MaxEventNameLength)]
        public string EventName { get; set; }

        /// <summary>
        /// Event type
        /// </summary>
        [Required]
        public EventType EventType { get; set; }

        /// <summary>
        /// Event date
        /// </summary>
        [Required]
        public DateTime EventDate { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(TermId))]
        public virtual Term Term { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected TermEvent()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public TermEvent(
            Guid id,
            Guid termId,
            string eventName,
            EventType eventType,
            DateTime eventDate) : this()
        {
            Id = id;
            TermId = termId;
            EventName = eventName;
            EventType = eventType;
            EventDate = eventDate;
        }
    }
}
