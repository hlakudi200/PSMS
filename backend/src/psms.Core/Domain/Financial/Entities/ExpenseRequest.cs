using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Financial.Entities;

[Table("ExpenseRequests")]
public class ExpenseRequest : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
{
    public int? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string RequestNumber { get; set; }

    [Required]
    public long RequestedByUserId { get; set; }

    [Required]
    [StringLength(256)]
    public string RequestedByName { get; set; }

    [StringLength(200)]
    public string Department { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public ExpenseCategory Category { get; set; }

    [Required]
    [StringLength(1000)]
    public string Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(10)]
    public string Currency { get; set; } = "ZAR";

    [Required]
    public ExpenseStatus Status { get; set; }

    [StringLength(200)]
    public string Vendor { get; set; }

    [StringLength(2048)]
    public string QuotationUrl { get; set; }

    [StringLength(2048)]
    public string InvoiceUrl { get; set; }

    [StringLength(2048)]
    public string ReceiptUrl { get; set; }

    [Required]
    public ExpensePriority Priority { get; set; }

    public DateTime? RequiredByDate { get; set; }

    public long? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ApprovedAmount { get; set; }

    [StringLength(500)]
    public string RejectionReason { get; set; }

    [StringLength(100)]
    public string PaymentReference { get; set; }

    public DateTime? PaymentDate { get; set; }

    [ForeignKey(nameof(AcademicYearId))]
    public virtual AcademicYear AcademicYear { get; set; }

    protected ExpenseRequest() { }

    public ExpenseRequest(Guid id, int? tenantId, string requestNumber,
        long requestedByUserId, string requestedByName, Guid academicYearId,
        ExpenseCategory category, string description, decimal amount, ExpensePriority priority)
    {
        Id = id;
        TenantId = tenantId;
        RequestNumber = requestNumber;
        RequestedByUserId = requestedByUserId;
        RequestedByName = requestedByName;
        AcademicYearId = academicYearId;
        Category = category;
        Description = description;
        Amount = amount;
        Priority = priority;
        Currency = "ZAR";
        Status = ExpenseStatus.Draft;
    }

    public void Submit()
    {
        if (Status != ExpenseStatus.Draft)
            throw new InvalidOperationException("Only draft expenses can be submitted.");
        Status = ExpenseStatus.Submitted;
    }

    /// <summary>
    /// WF-31: the approval workflow entered review — the record is now with a
    /// reviewer and can no longer be edited by the requester.
    /// </summary>
    public void StartReview()
    {
        if (Status != ExpenseStatus.Submitted)
            throw new InvalidOperationException("Only submitted expenses can move to review.");
        Status = ExpenseStatus.UnderReview;
    }

    /// <summary>
    /// WF-31: the approval workflow was cancelled or recalled before a decision —
    /// return the expense to the requester as a draft.
    /// </summary>
    public void ReopenAsDraft()
    {
        if (Status != ExpenseStatus.Submitted && Status != ExpenseStatus.UnderReview)
            throw new InvalidOperationException("Only submitted or in-review expenses can be reopened.");
        Status = ExpenseStatus.Draft;
    }

    public void Approve(long userId, decimal approvedAmount)
    {
        if (Status != ExpenseStatus.Submitted && Status != ExpenseStatus.UnderReview)
            throw new InvalidOperationException("Expense must be submitted or under review to approve.");
        ApprovedByUserId = userId;
        ApprovedDate = DateTime.UtcNow;
        ApprovedAmount = approvedAmount;
        Status = ExpenseStatus.Approved;
    }

    public void Reject(long userId, string reason)
    {
        if (Status != ExpenseStatus.Submitted && Status != ExpenseStatus.UnderReview)
            throw new InvalidOperationException("Expense must be submitted or under review to reject.");
        ApprovedByUserId = userId;
        ApprovedDate = DateTime.UtcNow;
        RejectionReason = reason;
        Status = ExpenseStatus.Rejected;
    }

    public void Cancel()
    {
        if (Status == ExpenseStatus.Paid || Status == ExpenseStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel a paid or already cancelled expense.");
        Status = ExpenseStatus.Cancelled;
    }

    public void MarkAsPaid(string reference)
    {
        if (Status != ExpenseStatus.Approved)
            throw new InvalidOperationException("Expense must be approved to mark as paid.");
        PaymentReference = reference;
        PaymentDate = DateTime.UtcNow;
        Status = ExpenseStatus.Paid;
    }
}
