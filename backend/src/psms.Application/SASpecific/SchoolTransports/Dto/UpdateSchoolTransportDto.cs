using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.SASpecific.SchoolTransports.Dto;

/// <summary>
/// Input DTO for updating a school transport route. All fields nullable (null-skip).
/// IsActive is NOT included — use Activate/Deactivate endpoints.
/// </summary>
public class UpdateSchoolTransportDto
{
    [StringLength(200)]
    public string RouteName { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [StringLength(20)]
    public string VehicleNumber { get; set; }

    public TransportType? TransportType { get; set; }

    [Range(1, int.MaxValue)]
    public int? Capacity { get; set; }

    [StringLength(100)]
    public string DriverName { get; set; }

    [StringLength(20)]
    public string DriverPhone { get; set; }

    [StringLength(2000)]
    public string AreasCovered { get; set; }

    public TimeSpan? MorningPickupTime { get; set; }

    public TimeSpan? AfternoonDepartureTime { get; set; }

    [Range(0, 9999999.99)]
    public decimal? MonthlyFee { get; set; }
}
