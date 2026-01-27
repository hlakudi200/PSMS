using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Admissions.Entities
{
    /// <summary>
    /// Represents the application fee payment
    /// </summary>
    [Table("ApplicationFees")]
    public class ApplicationFee : CreationAuditedEntity<Guid>
    {
        public const int MaxCurrencyLength = 3;
        public const int MaxReferenceLength = 100;
        public const int MaxReceiptNumberLength = 50;

        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(MaxCurrencyLength)]
        public string Currency { get; set; } = "ZAR";

        [Required]
        public PaymentStatus Status { get; set; }

        public SouthAfricanPaymentMethod? PaymentMethod { get; set; }

        [StringLength(MaxReferenceLength)]
        public string PaymentReference { get; set; }

        public DateTime? PaymentDate { get; set; }

        [StringLength(MaxReceiptNumberLength)]
        public string ReceiptNumber { get; set; }

        public bool IsRefundable { get; set; } = false;

        [ForeignKey(nameof(ApplicationId))]
        public virtual Application Application { get; set; }

        protected ApplicationFee() { }

        public ApplicationFee(Guid id, Guid applicationId, decimal amount, string currency = "ZAR")
        {
            Id = id;
            ApplicationId = applicationId;
            Amount = amount;
            Currency = currency;
            Status = PaymentStatus.Pending;
            IsRefundable = false;
        }

        public void MarkAsPaid(SouthAfricanPaymentMethod method, string reference, string receipt)
        {
            Status = PaymentStatus.Completed;
            PaymentMethod = method;
            PaymentReference = reference;
            ReceiptNumber = receipt;
            PaymentDate = DateTime.UtcNow;
        }
    }
}
