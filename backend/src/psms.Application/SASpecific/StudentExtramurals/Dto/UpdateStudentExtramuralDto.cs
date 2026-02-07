using System.ComponentModel.DataAnnotations;

namespace psms.SASpecific.StudentExtramurals.Dto;

/// <summary>
/// Input DTO for updating a student extramural enrollment. All fields nullable (null-skip).
/// Status is NOT included — use Suspend/Reactivate/Terminate endpoints.
/// </summary>
public class UpdateStudentExtramuralDto
{
    public int? TermNumber { get; set; }

    public string TeamAssignment { get; set; }

    public string PositionRole { get; set; }

    [StringLength(1000)]
    public string MedicalNotes { get; set; }

    [StringLength(100)]
    public string EmergencyContactName { get; set; }

    [StringLength(20)]
    public string EmergencyContactPhone { get; set; }

    [Range(0, 100)]
    public decimal? AttendanceRate { get; set; }

    [StringLength(1000)]
    public string Notes { get; set; }
}
