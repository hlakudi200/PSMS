using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.SASpecific.StudentTransports.Dto;

/// <summary>
/// Input DTO for updating a student transport enrollment. All fields nullable (null-skip).
/// Status is NOT included — use Suspend/Reactivate/Terminate endpoints.
/// </summary>
public class UpdateStudentTransportDto
{
    public TransportDirection? Direction { get; set; }

    [StringLength(500)]
    public string PickupAddress { get; set; }

    [StringLength(500)]
    public string DropoffAddress { get; set; }

    public TimeSpan? PickupTime { get; set; }

    [StringLength(100)]
    public string EmergencyContactName { get; set; }

    [StringLength(20)]
    public string EmergencyContactPhone { get; set; }

    [StringLength(1000)]
    public string Notes { get; set; }
}
