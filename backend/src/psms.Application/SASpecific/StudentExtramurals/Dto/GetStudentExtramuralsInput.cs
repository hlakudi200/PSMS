using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.StudentExtramurals.Dto;

/// <summary>
/// Input DTO for querying student extramural enrollments with filters.
/// </summary>
public class GetStudentExtramuralsInput : PagedAndSortedResultRequestDto
{
    public Guid? StudentId { get; set; }
    public Guid? ExtramuralActivityId { get; set; }
    public Guid? AcademicYearId { get; set; }
    public EnrollmentStatus? Status { get; set; }
}
