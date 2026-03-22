using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Academic.Entities;

[Table("StudentTransferRequests")]
public class StudentTransferRequest : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
{
    public int? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string TransferNumber { get; set; }

    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public TransferType TransferType { get; set; }

    [Required]
    public TransferStatus Status { get; set; }

    [Required]
    [StringLength(1000)]
    public string Reason { get; set; }

    [StringLength(200)]
    public string FromSchoolName { get; set; }

    [StringLength(200)]
    public string ToSchoolName { get; set; }

    [Required]
    public DateTime RequestedDate { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public Guid? TransferGradeId { get; set; }

    [StringLength(2048)]
    public string PreviousReportUrl { get; set; }

    [StringLength(2048)]
    public string TransferCertificateUrl { get; set; }

    public long? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }

    [StringLength(1000)]
    public string Notes { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; }

    [ForeignKey(nameof(AcademicYearId))]
    public virtual AcademicYear AcademicYear { get; set; }

    [ForeignKey(nameof(TransferGradeId))]
    public virtual Grade TransferGrade { get; set; }

    protected StudentTransferRequest() { }

    public StudentTransferRequest(Guid id, int? tenantId, string transferNumber,
        Guid studentId, Guid academicYearId, TransferType transferType, string reason)
    {
        Id = id;
        TenantId = tenantId;
        TransferNumber = transferNumber;
        StudentId = studentId;
        AcademicYearId = academicYearId;
        TransferType = transferType;
        Reason = reason;
        RequestedDate = DateTime.UtcNow;
        Status = TransferStatus.Draft;
    }

    public void Submit()
    {
        if (Status != TransferStatus.Draft)
            throw new InvalidOperationException("Only draft transfers can be submitted.");
        Status = TransferStatus.Submitted;
    }

    public void Approve(long userId)
    {
        if (Status != TransferStatus.Submitted && Status != TransferStatus.UnderReview)
            throw new InvalidOperationException("Transfer must be submitted or under review to approve.");
        ApprovedByUserId = userId;
        ApprovedDate = DateTime.UtcNow;
        Status = TransferStatus.Approved;
    }

    public void Reject(long userId, string notes)
    {
        if (Status != TransferStatus.Submitted && Status != TransferStatus.UnderReview)
            throw new InvalidOperationException("Transfer must be submitted or under review to reject.");
        ApprovedByUserId = userId;
        ApprovedDate = DateTime.UtcNow;
        Notes = notes;
        Status = TransferStatus.Rejected;
    }

    public void Complete(string certificateUrl = null)
    {
        if (Status != TransferStatus.Approved)
            throw new InvalidOperationException("Transfer must be approved to complete.");
        TransferCertificateUrl = certificateUrl;
        Status = TransferStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == TransferStatus.Completed || Status == TransferStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel a completed or already cancelled transfer.");
        Status = TransferStatus.Cancelled;
    }
}
