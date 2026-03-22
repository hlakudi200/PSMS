using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.HR.Entities;

[Table("StaffLeaveRequests")]
public class StaffLeaveRequest : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
{
    public int? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string LeaveNumber { get; set; }

    [Required]
    public long UserId { get; set; }

    [Required]
    [StringLength(256)]
    public string UserName { get; set; }

    [Required]
    public StaffLeaveType LeaveType { get; set; }

    [Required]
    public StaffLeaveStatus Status { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [NotMapped]
    public decimal TotalDays => (decimal)(EndDate - StartDate).TotalDays + 1;

    [Required]
    [StringLength(1000)]
    public string Reason { get; set; }

    public Guid? SubstituteTeacherId { get; set; }

    [StringLength(256)]
    public string SubstituteTeacherName { get; set; }

    public long? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }

    [StringLength(500)]
    public string RejectionReason { get; set; }

    [StringLength(2048)]
    public string SupportingDocumentUrl { get; set; }

    [ForeignKey(nameof(SubstituteTeacherId))]
    public virtual Teacher SubstituteTeacher { get; set; }

    protected StaffLeaveRequest() { }

    public StaffLeaveRequest(Guid id, int? tenantId, string leaveNumber,
        long userId, string userName, StaffLeaveType leaveType,
        DateTime startDate, DateTime endDate, string reason)
    {
        Id = id;
        TenantId = tenantId;
        LeaveNumber = leaveNumber;
        UserId = userId;
        UserName = userName;
        LeaveType = leaveType;
        StartDate = startDate;
        EndDate = endDate;
        Reason = reason;
        Status = StaffLeaveStatus.Draft;
    }

    public void Submit()
    {
        if (Status != StaffLeaveStatus.Draft)
            throw new InvalidOperationException("Only draft leave requests can be submitted.");
        if (EndDate < StartDate)
            throw new InvalidOperationException("End date must be after start date.");
        Status = StaffLeaveStatus.Submitted;
    }

    public void Approve(long userId)
    {
        if (Status != StaffLeaveStatus.Submitted && Status != StaffLeaveStatus.HODApproved)
            throw new InvalidOperationException("Leave must be submitted or HOD-approved to approve.");
        ApprovedByUserId = userId;
        ApprovedDate = DateTime.UtcNow;
        Status = StaffLeaveStatus.Approved;
    }

    public void Reject(long userId, string reason)
    {
        if (Status != StaffLeaveStatus.Submitted && Status != StaffLeaveStatus.HODApproved)
            throw new InvalidOperationException("Leave must be submitted or HOD-approved to reject.");
        ApprovedByUserId = userId;
        ApprovedDate = DateTime.UtcNow;
        RejectionReason = reason;
        Status = StaffLeaveStatus.Rejected;
    }

    public void Cancel()
    {
        if (Status == StaffLeaveStatus.Approved || Status == StaffLeaveStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel approved or already cancelled leave.");
        Status = StaffLeaveStatus.Cancelled;
    }
}
