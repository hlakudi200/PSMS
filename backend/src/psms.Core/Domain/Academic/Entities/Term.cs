using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents a school term (SA: 4 terms per year)
    /// </summary>
    [Table("Terms")]
    public class Term : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxTermNameLength = 50;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Academic year this term belongs to
        /// </summary>
        [Required]
        public Guid AcademicYearId { get; set; }

        /// <summary>
        /// Term number (1-4 for SA)
        /// </summary>
        [Required]
        public SouthAfricanTermNumber TermNumber { get; set; }

        /// <summary>
        /// Term name (e.g., "Term 1", "Term 2")
        /// </summary>
        [Required]
        [StringLength(MaxTermNameLength)]
        public string TermName { get; set; }

        /// <summary>
        /// Start date of term
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of term
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Indicates if this is the current term
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        public virtual ICollection<TermEvent> TermEvents { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Term()
        {
            TermEvents = new HashSet<TermEvent>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Term(
            Guid id,
            int? tenantId,
            Guid academicYearId,
            SouthAfricanTermNumber termNumber,
            DateTime startDate,
            DateTime endDate) : this()
        {
            Id = id;
            TenantId = tenantId;
            AcademicYearId = academicYearId;
            TermNumber = termNumber;
            TermName = $"Term {(int)termNumber}";
            StartDate = startDate;
            EndDate = endDate;
            IsCurrent = false;
            IsDeleted = false;
        }
    }
}
