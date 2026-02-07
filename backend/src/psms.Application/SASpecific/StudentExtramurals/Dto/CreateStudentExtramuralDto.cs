using System;
using System.ComponentModel.DataAnnotations;

namespace psms.SASpecific.StudentExtramurals.Dto;

/// <summary>
/// Input DTO for creating a student extramural enrollment.
/// </summary>
public class CreateStudentExtramuralDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid ExtramuralActivityId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public int? TermNumber { get; set; }

    public string TeamAssignment { get; set; }

    public string PositionRole { get; set; }

    [StringLength(1000)]
    public string MedicalNotes { get; set; }

    [StringLength(100)]
    public string EmergencyContactName { get; set; }

    [StringLength(20)]
    public string EmergencyContactPhone { get; set; }

    [StringLength(1000)]
    public string Notes { get; set; }
}
