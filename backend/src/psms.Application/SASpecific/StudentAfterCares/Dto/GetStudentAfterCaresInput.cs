using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.StudentAfterCares.Dto;

/// <summary>
/// Input DTO for querying student after-care enrollments with filters.
/// </summary>
public class GetStudentAfterCaresInput : PagedAndSortedResultRequestDto
{
    public Guid? StudentId { get; set; }
    public Guid? AfterCareId { get; set; }
    public Guid? AcademicYearId { get; set; }
    public EnrollmentStatus? Status { get; set; }
}
