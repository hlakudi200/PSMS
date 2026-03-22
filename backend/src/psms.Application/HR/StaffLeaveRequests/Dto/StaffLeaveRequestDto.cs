using Abp.Application.Services.Dto;
using System;

namespace psms.HR.StaffLeaveRequests.Dto;

/// <summary>
/// Full DTO for a staff leave request.
/// </summary>
public class StaffLeaveRequestDto : FullAuditedEntityDto<Guid>
{
    public string LeaveNumber { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; }
    public int LeaveType { get; set; }
    public int Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Reason { get; set; }
    public Guid? SubstituteTeacherId { get; set; }
    public string SubstituteTeacherName { get; set; }
    public long? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string RejectionReason { get; set; }
    public string SupportingDocumentUrl { get; set; }
}
