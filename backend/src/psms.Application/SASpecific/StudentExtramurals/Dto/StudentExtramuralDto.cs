using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.StudentExtramurals.Dto;

/// <summary>
/// Full DTO for a student extramural enrollment.
/// </summary>
public class StudentExtramuralDto : FullAuditedEntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid ExtramuralActivityId { get; set; }
    public Guid AcademicYearId { get; set; }
    public int? TermNumber { get; set; }
    public EnrollmentStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string TeamAssignment { get; set; }
    public string PositionRole { get; set; }
    public string MedicalNotes { get; set; }
    public string EmergencyContactName { get; set; }
    public string EmergencyContactPhone { get; set; }
    public bool ConsentFormSigned { get; set; }
    public DateTime? ConsentFormDate { get; set; }
    public string Achievements { get; set; }
    public decimal? AttendanceRate { get; set; }
    public string Notes { get; set; }

    // Flattened
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
    public string ActivityName { get; set; }
    public string AcademicYearName { get; set; }
}
