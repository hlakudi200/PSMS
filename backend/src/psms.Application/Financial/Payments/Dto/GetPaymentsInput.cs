using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Financial.Payments.Dto;

/// <summary>
/// Input DTO for querying payments with filters.
/// </summary>
public class GetPaymentsInput : PagedAndSortedResultRequestDto
{
    public Guid? StudentId { get; set; }
    public Guid? ParentId { get; set; }
    public PaymentStatus? Status { get; set; }
    public SouthAfricanPaymentMethod? PaymentMethod { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string ReceiptNumber { get; set; }
    public string StudentName { get; set; }
}
