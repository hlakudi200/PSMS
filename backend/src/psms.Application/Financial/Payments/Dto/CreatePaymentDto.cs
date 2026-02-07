using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Financial.Payments.Dto;

/// <summary>
/// Input DTO for recording a manual payment.
/// </summary>
public class CreatePaymentDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid ParentId { get; set; }

    [Required]
    [Range(0.01, 9999999.99)]
    public decimal Amount { get; set; }

    [Required]
    public SouthAfricanPaymentMethod PaymentMethod { get; set; }

    public DateTime? PaymentDate { get; set; }

    [StringLength(100)]
    public string PaymentReference { get; set; }

    [StringLength(2000)]
    public string Notes { get; set; }
}
