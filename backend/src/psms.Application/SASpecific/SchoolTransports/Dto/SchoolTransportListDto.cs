using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.SchoolTransports.Dto;

/// <summary>
/// Lightweight DTO for school transport route lists.
/// </summary>
public class SchoolTransportListDto : EntityDto<Guid>
{
    public string RouteName { get; set; }
    public string VehicleNumber { get; set; }
    public TransportType TransportType { get; set; }
    public int Capacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public string DriverName { get; set; }
    public decimal MonthlyFee { get; set; }
    public bool IsActive { get; set; }

    // Computed
    public int EnrollmentCount { get; set; }
}
