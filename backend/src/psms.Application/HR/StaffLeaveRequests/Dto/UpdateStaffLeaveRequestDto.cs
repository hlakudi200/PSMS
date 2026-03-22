using System;
using System.ComponentModel.DataAnnotations;

namespace psms.HR.StaffLeaveRequests.Dto;

/// <summary>
/// DTO for updating a staff leave request.
/// All fields optional (null-skip pattern).
/// </summary>
public class UpdateStaffLeaveRequestDto
{
    public int? LeaveType { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [StringLength(1000)]
    public string Reason { get; set; }

    public Guid? SubstituteTeacherId { get; set; }

    [StringLength(2048)]
    public string SupportingDocumentUrl { get; set; }
}
