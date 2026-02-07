using System;
using System.ComponentModel.DataAnnotations;

namespace psms.SASpecific.StudentAfterCares.Dto;

/// <summary>
/// Input DTO for updating a student after-care enrollment. All fields nullable (null-skip).
/// Status is NOT included — use Suspend/Reactivate/Terminate endpoints.
/// </summary>
public class UpdateStudentAfterCareDto
{
    public string DaysEnrolled { get; set; }

    [StringLength(500)]
    public string DietaryRequirements { get; set; }

    [StringLength(1000)]
    public string MedicalNotes { get; set; }

    [StringLength(2000)]
    public string AuthorizedPickupPersons { get; set; }

    public TimeSpan? UsualPickupTime { get; set; }

    [StringLength(1000)]
    public string Notes { get; set; }
}
