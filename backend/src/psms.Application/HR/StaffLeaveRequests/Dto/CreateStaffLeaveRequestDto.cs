using System;
using System.ComponentModel.DataAnnotations;

namespace psms.HR.StaffLeaveRequests.Dto;

/// <summary>
/// DTO for creating a staff leave request.
/// </summary>
public class CreateStaffLeaveRequestDto
{
    [Required]
    public int LeaveType { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [StringLength(1000)]
    public string Reason { get; set; }

    public Guid? SubstituteTeacherId { get; set; }

    [StringLength(2048)]
    public string SupportingDocumentUrl { get; set; }
}
