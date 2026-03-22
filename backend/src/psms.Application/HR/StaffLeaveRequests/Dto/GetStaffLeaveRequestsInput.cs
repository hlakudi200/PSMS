using Abp.Application.Services.Dto;
using System;

namespace psms.HR.StaffLeaveRequests.Dto;

/// <summary>
/// Input DTO for querying staff leave requests.
/// </summary>
public class GetStaffLeaveRequestsInput : PagedAndSortedResultRequestDto
{
    public long? UserId { get; set; }
    public int? LeaveType { get; set; }
    public int? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Search { get; set; }
}
