using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Financial.Entities;

[Table("FeeWaivers")]
public class FeeWaiver : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
{
    public int? TenantId { get; set; }

    [Required]
    public Guid StudentId { get; set; }

    public Guid? StudentFeeId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public FeeWaiverType WaiverType { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RequestedAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ApprovedAmount { get; set; }

    [Required]
    [StringLength(1000)]
    public string Reason { get; set; }

    [Required]
    public FeeWaiverStatus Status { get; set; }

    [StringLength(2048)]
    public string SupportingDocumentUrl { get; set; }

    public long? ReviewedByUserId { get; set; }
    public DateTime? ReviewedDate { get; set; }

    [StringLength(500)]
    public string ReviewNotes { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; }

    [ForeignKey(nameof(AcademicYearId))]
    public virtual AcademicYear AcademicYear { get; set; }

    protected FeeWaiver() { }

    public FeeWaiver(Guid id, int? tenantId, Guid studentId, Guid academicYearId,
        FeeWaiverType waiverType, decimal requestedAmount, string reason)
    {
        Id = id;
        TenantId = tenantId;
        StudentId = studentId;
        AcademicYearId = academicYearId;
        WaiverType = waiverType;
        RequestedAmount = requestedAmount;
        Reason = reason;
        Status = FeeWaiverStatus.Draft;
    }

    public void Submit()
    {
        if (Status != FeeWaiverStatus.Draft)
            throw new InvalidOperationException("Only draft waivers can be submitted.");
        Status = FeeWaiverStatus.Submitted;
    }

    public void Approve(long userId, decimal approvedAmount, string notes = null)
    {
        if (Status != FeeWaiverStatus.Submitted && Status != FeeWaiverStatus.UnderReview)
            throw new InvalidOperationException("Waiver must be submitted or under review to approve.");
        Status = FeeWaiverStatus.Approved;
        ApprovedAmount = approvedAmount;
        ReviewedByUserId = userId;
        ReviewedDate = DateTime.UtcNow;
        ReviewNotes = notes;
    }

    public void Reject(long userId, string notes)
    {
        if (Status != FeeWaiverStatus.Submitted && Status != FeeWaiverStatus.UnderReview)
            throw new InvalidOperationException("Waiver must be submitted or under review to reject.");
        Status = FeeWaiverStatus.Rejected;
        ReviewedByUserId = userId;
        ReviewedDate = DateTime.UtcNow;
        ReviewNotes = notes;
    }

    public void Cancel()
    {
        if (Status == FeeWaiverStatus.Approved || Status == FeeWaiverStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel an approved or already cancelled waiver.");
        Status = FeeWaiverStatus.Cancelled;
    }
}
