using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Financial.StudentFees.Dto;

/// <summary>
/// Input DTO for querying student fees with filters.
/// </summary>
public class GetStudentFeesInput : PagedAndSortedResultRequestDto
{
    public Guid? StudentId { get; set; }
    public Guid? FeeStructureId { get; set; }
    public Guid? GradeId { get; set; }
    public Guid? AcademicYearId { get; set; }
    public FeeStatus? Status { get; set; }
    public string StudentName { get; set; }
}
