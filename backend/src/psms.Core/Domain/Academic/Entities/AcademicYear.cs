using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents an academic year with SA 4-term structure
    /// </summary>
    [Table("AcademicYears")]
    public class AcademicYear : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxYearNameLength = 50;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Year value (e.g., 2024)
        /// </summary>
        [Required]
        public int Year { get; set; }

        /// <summary>
        /// Year name (e.g., "2024 Academic Year")
        /// </summary>
        [Required]
        [StringLength(MaxYearNameLength)]
        public string YearName { get; set; }

        /// <summary>
        /// Start date of academic year
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of academic year
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Indicates if this is the current academic year
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public virtual ICollection<Term> Terms { get; set; }
        public virtual ICollection<Class> Classes { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected AcademicYear()
        {
            Terms = new HashSet<Term>();
            Classes = new HashSet<Class>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public AcademicYear(
            Guid id,
            int? tenantId,
            int year,
            DateTime startDate,
            DateTime endDate) : this()
        {
            Id = id;
            TenantId = tenantId;
            Year = year;
            YearName = $"{year} Academic Year";
            StartDate = startDate;
            EndDate = endDate;
            IsCurrent = false;
            IsDeleted = false;
        }
    }
}
