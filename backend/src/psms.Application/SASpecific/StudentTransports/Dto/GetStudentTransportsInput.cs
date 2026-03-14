using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.StudentTransports.Dto;

/// <summary>
/// Input DTO for querying student transport enrollments with filters.
/// </summary>
public class GetStudentTransportsInput : PagedAndSortedResultRequestDto
{
    public Guid? StudentId { get; set; }
    public Guid? SchoolTransportId { get; set; }
    public Guid? AcademicYearId { get; set; }
    public EnrollmentStatus? Status { get; set; }
    public TransportDirection? Direction { get; set; }
    public string Keyword { get; set; }
}
