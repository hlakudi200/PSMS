using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.SchoolTransports.Dto;

/// <summary>
/// Full DTO for a school transport route.
/// </summary>
public class SchoolTransportDto : FullAuditedEntityDto<Guid>
{
    public string RouteName { get; set; }
    public string Description { get; set; }
    public string VehicleNumber { get; set; }
    public TransportType TransportType { get; set; }
    public int Capacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public string DriverName { get; set; }
    public string DriverPhone { get; set; }
    public string AreasCovered { get; set; }
    public TimeSpan? MorningPickupTime { get; set; }
    public TimeSpan? AfternoonDepartureTime { get; set; }
    public decimal MonthlyFee { get; set; }
    public bool IsActive { get; set; }

    // Computed
    public int EnrollmentCount { get; set; }
}
