using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.StudentExtramurals.Dto;

/// <summary>
/// Lightweight DTO for student extramural enrollment lists.
/// </summary>
public class StudentExtramuralListDto : EntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid ExtramuralActivityId { get; set; }
    public Guid AcademicYearId { get; set; }
    public EnrollmentStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool ConsentFormSigned { get; set; }
    public string TeamAssignment { get; set; }

    // Flattened
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
    public string ActivityName { get; set; }
    public string AcademicYearName { get; set; }
}
