using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.StudentAfterCares.Dto;

/// <summary>
/// Lightweight DTO for student after-care enrollment lists.
/// </summary>
public class StudentAfterCareListDto : EntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid AfterCareId { get; set; }
    public Guid AcademicYearId { get; set; }
    public EnrollmentStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string DaysEnrolled { get; set; }

    // Flattened
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
    public string AfterCareProgramName { get; set; }
    public string AcademicYearName { get; set; }
}
