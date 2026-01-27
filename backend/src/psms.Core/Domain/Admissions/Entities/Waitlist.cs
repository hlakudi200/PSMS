using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Admissions.Entities
{
    /// <summary>
    /// Represents a waitlist entry for applications when grade capacity is full
    /// </summary>
    [Table("Waitlists")]
    public class Waitlist : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxNotesLength = 2000;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Reference to the application
        /// </summary>
        [Required]
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// Grade the applicant is waitlisted for
        /// </summary>
        [Required]
        public Guid GradeId { get; set; }

        /// <summary>
        /// Position in the waitlist queue (1 = first in line)
        /// </summary>
        [Required]
        public int Position { get; set; }

        /// <summary>
        /// Current status of the waitlist entry
        /// </summary>
        [Required]
        public WaitlistStatus Status { get; set; }

        /// <summary>
        /// Date when added to waitlist
        /// </summary>
        [Required]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Date when applicant was notified of position availability
        /// </summary>
        public DateTime? NotifiedDate { get; set; }

        /// <summary>
        /// Date when the offer expires if position becomes available
        /// </summary>
        public DateTime? OfferExpiryDate { get; set; }

        /// <summary>
        /// Additional notes about the waitlist entry
        /// </summary>
        [StringLength(MaxNotesLength)]
        public string Notes { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ApplicationId))]
        public virtual Application Application { get; set; }

        [ForeignKey(nameof(GradeId))]
        public virtual Grade Grade { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected Waitlist()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public Waitlist(
            Guid id,
            int? tenantId,
            Guid applicationId,
            Guid gradeId,
            int position) : this()
        {
            Id = id;
            TenantId = tenantId;
            ApplicationId = applicationId;
            GradeId = gradeId;
            Position = position;
            AddedDate = DateTime.UtcNow;
            Status = WaitlistStatus.Active;
        }

        /// <summary>
        /// Offers the position to the applicant
        /// </summary>
        public void OfferPosition(int expiryDays = 7)
        {
            if (Status != WaitlistStatus.Active)
                throw new InvalidOperationException("Only active waitlist entries can be offered.");

            Status = WaitlistStatus.Offered;
            NotifiedDate = DateTime.UtcNow;
            OfferExpiryDate = DateTime.UtcNow.AddDays(expiryDays);
        }

        /// <summary>
        /// Accepts the waitlist offer
        /// </summary>
        public void AcceptOffer()
        {
            if (Status != WaitlistStatus.Offered)
                throw new InvalidOperationException("Only offered positions can be accepted.");

            Status = WaitlistStatus.Accepted;
        }

        /// <summary>
        /// Declines the waitlist offer
        /// </summary>
        public void DeclineOffer()
        {
            if (Status != WaitlistStatus.Offered)
                throw new InvalidOperationException("Only offered positions can be declined.");

            Status = WaitlistStatus.Declined;
        }

        /// <summary>
        /// Withdraws from the waitlist
        /// </summary>
        public void Withdraw()
        {
            if (Status == WaitlistStatus.Accepted || Status == WaitlistStatus.Withdrawn)
                throw new InvalidOperationException("Cannot withdraw from accepted or already withdrawn waitlist.");

            Status = WaitlistStatus.Withdrawn;
        }
    }
}
