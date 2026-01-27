using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;

namespace psms.Domain.Financial.Entities
{
    /// <summary>
    /// Represents how a payment is allocated to specific student fees
    /// </summary>
    [Table("PaymentAllocations")]
    public class PaymentAllocation : Entity<Guid>
    {
        /// <summary>
        /// Payment being allocated
        /// </summary>
        [Required]
        public Guid PaymentId { get; set; }

        /// <summary>
        /// Student fee receiving the allocation
        /// </summary>
        [Required]
        public Guid StudentFeeId { get; set; }

        /// <summary>
        /// Amount allocated to this fee
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Date when allocation was made
        /// </summary>
        [Required]
        public DateTime AllocatedDate { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(PaymentId))]
        public virtual Payment Payment { get; set; }

        [ForeignKey(nameof(StudentFeeId))]
        public virtual StudentFee StudentFee { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected PaymentAllocation()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public PaymentAllocation(
            Guid id,
            Guid paymentId,
            Guid studentFeeId,
            decimal amount) : this()
        {
            if (amount <= 0)
                throw new ArgumentException("Allocation amount must be positive.", nameof(amount));

            Id = id;
            PaymentId = paymentId;
            StudentFeeId = studentFeeId;
            Amount = amount;
            AllocatedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the allocation amount
        /// </summary>
        public void UpdateAmount(decimal newAmount)
        {
            if (newAmount <= 0)
                throw new ArgumentException("Allocation amount must be positive.", nameof(newAmount));

            Amount = newAmount;
        }
    }
}
