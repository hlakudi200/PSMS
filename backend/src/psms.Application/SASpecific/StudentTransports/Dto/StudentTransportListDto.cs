using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.StudentTransports.Dto;

/// <summary>
/// Lightweight DTO for student transport enrollment lists.
/// </summary>
public class StudentTransportListDto : EntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid SchoolTransportId { get; set; }
    public Guid AcademicYearId { get; set; }
    public TransportDirection Direction { get; set; }
    public EnrollmentStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string PickupAddress { get; set; }

    // Flattened
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
    public string RouteName { get; set; }
    public string AcademicYearName { get; set; }
}
