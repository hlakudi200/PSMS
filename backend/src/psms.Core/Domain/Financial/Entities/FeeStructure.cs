using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Financial.Entities
{
    /// <summary>
    /// Represents a fee structure for a specific grade and academic year
    /// </summary>
    [Table("FeeStructures")]
    public class FeeStructure : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxFeeNameLength = 200;
        public const int MaxCurrencyLength = 10;
        public const int MaxBillingFrequencyLength = 50;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Grade this fee structure applies to
        /// </summary>
        [Required]
        public Guid GradeId { get; set; }

        /// <summary>
        /// Academic year this fee structure applies to
        /// </summary>
        [Required]
        public Guid AcademicYearId { get; set; }

        /// <summary>
        /// Type of fee (Tuition, Registration, Transport, etc.)
        /// </summary>
        [Required]
        public SouthAfricanFeeType FeeType { get; set; }

        /// <summary>
        /// Display name for the fee
        /// </summary>
        [Required]
        [StringLength(MaxFeeNameLength)]
        public string FeeName { get; set; }

        /// <summary>
        /// Amount of the fee
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency code (default ZAR for South Africa)
        /// </summary>
        [Required]
        [StringLength(MaxCurrencyLength)]
        public string Currency { get; set; }

        /// <summary>
        /// How often the fee is billed (Monthly, Quarterly, Annually, Per Term)
        /// </summary>
        [StringLength(MaxBillingFrequencyLength)]
        public string BillingFrequency { get; set; }

        /// <summary>
        /// Day of month when fee is due (1-31)
        /// </summary>
        [Range(1, 31)]
        public int DueDay { get; set; }

        /// <summary>
        /// Indicates if this fee structure is currently active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(GradeId))]
        public virtual Grade Grade { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        // Collections
        public virtual ICollection<StudentFee> StudentFees { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected FeeStructure()
        {
            StudentFees = new HashSet<StudentFee>();
            Currency = "ZAR";
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public FeeStructure(
            Guid id,
            int? tenantId,
            Guid gradeId,
            Guid academicYearId,
            SouthAfricanFeeType feeType,
            string feeName,
            decimal amount) : this()
        {
            Id = id;
            TenantId = tenantId;
            GradeId = gradeId;
            AcademicYearId = academicYearId;
            FeeType = feeType;
            FeeName = feeName;
            Amount = amount;
            IsActive = true;
            IsDeleted = false;
            DueDay = 1;
        }

        /// <summary>
        /// Gets the formatted amount with currency
        /// </summary>
        public string GetFormattedAmount()
        {
            return $"{Currency} {Amount:N2}";
        }

        /// <summary>
        /// Activates the fee structure
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Deactivates the fee structure
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
