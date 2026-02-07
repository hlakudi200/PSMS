using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.StudentTransports.Dto;

/// <summary>
/// Full DTO for a student transport enrollment.
/// </summary>
public class StudentTransportDto : FullAuditedEntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid SchoolTransportId { get; set; }
    public Guid AcademicYearId { get; set; }
    public TransportDirection Direction { get; set; }
    public string PickupAddress { get; set; }
    public string DropoffAddress { get; set; }
    public TimeSpan? PickupTime { get; set; }
    public EnrollmentStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string EmergencyContactName { get; set; }
    public string EmergencyContactPhone { get; set; }
    public string Notes { get; set; }

    // Flattened
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
    public string RouteName { get; set; }
    public string AcademicYearName { get; set; }
}
