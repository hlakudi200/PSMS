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
    /// Represents a fee assigned to a specific student
    /// </summary>
    [Table("StudentFees")]
    public class StudentFee : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxNotesLength = 2000;
        public const int MaxConcurrencyStampLength = 40;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Student who owes this fee
        /// </summary>
        [Required]
        public Guid StudentId { get; set; }

        /// <summary>
        /// Fee structure this is based on
        /// </summary>
        [Required]
        public Guid FeeStructureId { get; set; }

        /// <summary>
        /// Total amount due for this fee
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountDue { get; set; }

        /// <summary>
        /// Total amount paid towards this fee
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        /// <summary>
        /// Discount applied to this fee
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Date when this fee is due
        /// </summary>
        [Required]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Current status of the fee
        /// </summary>
        [Required]
        public FeeStatus Status { get; set; }

        /// <summary>
        /// Additional notes about this fee
        /// </summary>
        [StringLength(MaxNotesLength)]
        public string Notes { get; set; }

        /// <summary>
        /// Concurrency stamp for optimistic locking
        /// </summary>
        [StringLength(MaxConcurrencyStampLength)]
        public string ConcurrencyStamp { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(StudentId))]
        public virtual Student Student { get; set; }

        [ForeignKey(nameof(FeeStructureId))]
        public virtual FeeStructure FeeStructure { get; set; }

        // Collections
        public virtual ICollection<PaymentAllocation> PaymentAllocations { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected StudentFee()
        {
            PaymentAllocations = new HashSet<PaymentAllocation>();
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public StudentFee(
            Guid id,
            int? tenantId,
            Guid studentId,
            Guid feeStructureId,
            decimal amountDue,
            DateTime dueDate) : this()
        {
            Id = id;
            TenantId = tenantId;
            StudentId = studentId;
            FeeStructureId = feeStructureId;
            AmountDue = amountDue;
            DueDate = dueDate;
            AmountPaid = 0;
            DiscountAmount = 0;
            Status = FeeStatus.Pending;
            ConcurrencyStamp = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Gets the outstanding balance
        /// </summary>
        public decimal GetOutstandingBalance()
        {
            return AmountDue - DiscountAmount - AmountPaid;
        }

        /// <summary>
        /// Records a payment towards this fee
        /// </summary>
        public void RecordPayment(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Payment amount must be positive.", nameof(amount));

            AmountPaid += amount;
            UpdateStatus();
        }

        /// <summary>
        /// Applies a discount to this fee
        /// </summary>
        public void ApplyDiscount(decimal discount)
        {
            if (discount < 0)
                throw new ArgumentException("Discount cannot be negative.", nameof(discount));

            if (discount > AmountDue)
                throw new ArgumentException("Discount cannot exceed amount due.", nameof(discount));

            DiscountAmount = discount;
            UpdateStatus();
        }

        /// <summary>
        /// Waives the fee
        /// </summary>
        public void Waive()
        {
            Status = FeeStatus.Waived;
        }

        /// <summary>
        /// Updates the status based on payment amounts
        /// </summary>
        private void UpdateStatus()
        {
            var outstanding = GetOutstandingBalance();

            if (outstanding <= 0)
            {
                Status = FeeStatus.Paid;
            }
            else if (AmountPaid > 0)
            {
                Status = FeeStatus.PartiallyPaid;
            }
            else if (DateTime.UtcNow > DueDate)
            {
                Status = FeeStatus.Overdue;
            }
            else
            {
                Status = FeeStatus.Pending;
            }
        }

        /// <summary>
        /// Checks and marks as overdue if past due date
        /// </summary>
        public void CheckOverdue()
        {
            if (Status == FeeStatus.Pending && DateTime.UtcNow > DueDate)
            {
                Status = FeeStatus.Overdue;
            }
        }
    }
}
