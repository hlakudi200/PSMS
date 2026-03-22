using Abp.Application.Services.Dto;
using System;

namespace psms.HR.StaffLeaveRequests.Dto;

/// <summary>
/// Lightweight list DTO for staff leave requests.
/// </summary>
public class StaffLeaveRequestListDto : EntityDto<Guid>
{
    public string LeaveNumber { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; }
    public int LeaveType { get; set; }
    public int Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public DateTime CreationTime { get; set; }
}
