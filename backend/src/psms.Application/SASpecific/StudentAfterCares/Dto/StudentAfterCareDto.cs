using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.StudentAfterCares.Dto;

/// <summary>
/// Full DTO for a student after-care enrollment.
/// </summary>
public class StudentAfterCareDto : FullAuditedEntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid AfterCareId { get; set; }
    public Guid AcademicYearId { get; set; }
    public string DaysEnrolled { get; set; }
    public EnrollmentStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string DietaryRequirements { get; set; }
    public string MedicalNotes { get; set; }
    public string AuthorizedPickupPersons { get; set; }
    public TimeSpan? UsualPickupTime { get; set; }
    public string Notes { get; set; }

    // Flattened
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
    public string AfterCareProgramName { get; set; }
    public string AcademicYearName { get; set; }
}
