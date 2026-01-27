using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.Academic.Entities
{
    /// <summary>
    /// Represents POPIA (Protection of Personal Information Act) consent for a student
    /// </summary>
    [Table("POPIAConsents")]
    public class POPIAConsent : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Student this consent belongs to
        /// </summary>
        [Required]
        public Guid StudentId { get; set; }

        /// <summary>
        /// Date when consent was given
        /// </summary>
        [Required]
        public DateTime ConsentDate { get; set; }

        /// <summary>
        /// Allow photography and video recording
        /// </summary>
        public bool AllowPhotography { get; set; }

        /// <summary>
        /// Allow sharing data with third parties
        /// </summary>
        public bool AllowDataSharing { get; set; }

        /// <summary>
        /// Allow use of name in publications
        /// </summary>
        public bool AllowNameInPublications { get; set; }

        /// <summary>
        /// Allow use of image in marketing materials
        /// </summary>
        public bool AllowMarketingUse { get; set; }

        /// <summary>
        /// Parent/Guardian user ID who signed (long type)
        /// </summary>
        [Required]
        public long ParentSignatureUserId { get; set; }

        /// <summary>
        /// IP address from which consent was given
        /// </summary>
        [StringLength(50)]
        public string IpAddress { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected POPIAConsent()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public POPIAConsent(
            Guid id,
            int? tenantId,
            Guid studentId,
            DateTime consentDate,
            long parentSignatureUserId) : this()
        {
            Id = id;
            TenantId = tenantId;
            StudentId = studentId;
            ConsentDate = consentDate;
            ParentSignatureUserId = parentSignatureUserId;
            IsDeleted = false;
        }
    }
}
