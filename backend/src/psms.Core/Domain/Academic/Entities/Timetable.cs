using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents a timetable for a class
    /// </summary>
    [Table("Timetables")]
    public class Timetable : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Class this timetable belongs to
        /// </summary>
        [Required]
        public Guid ClassId { get; set; }

        /// <summary>
        /// Effective date when this timetable starts
        /// </summary>
        [Required]
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// End date when this timetable expires (optional)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Indicates if timetable is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ClassId))]
        public virtual Class Class { get; set; }

        public virtual ICollection<TimetableSlot> TimetableSlots { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Timetable()
        {
            TimetableSlots = new HashSet<TimetableSlot>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Timetable(
            Guid id,
            int? tenantId,
            Guid classId,
            DateTime effectiveDate) : this()
        {
            Id = id;
            TenantId = tenantId;
            ClassId = classId;
            EffectiveDate = effectiveDate;
            IsActive = true;
            IsDeleted = false;
        }
    }
}
