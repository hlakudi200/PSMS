using Abp.Application.Services.Dto;
using System;

namespace psms.Financial.ExpenseRequests.Dto;

/// <summary>
/// Full DTO for an expense request.
/// </summary>
public class ExpenseRequestDto : FullAuditedEntityDto<Guid>
{
    public string RequestNumber { get; set; }
    public long RequestedByUserId { get; set; }
    public string RequestedByName { get; set; }
    public string Department { get; set; }
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }
    public int Category { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public int Status { get; set; }
    public string Vendor { get; set; }
    public string QuotationUrl { get; set; }
    public string InvoiceUrl { get; set; }
    public string ReceiptUrl { get; set; }
    public int Priority { get; set; }
    public DateTime? RequiredByDate { get; set; }
    public long? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string RejectionReason { get; set; }
    public string PaymentReference { get; set; }
    public DateTime? PaymentDate { get; set; }
}
