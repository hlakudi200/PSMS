using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;

namespace psms.SASpecific.SchoolTransports.Dto;

/// <summary>
/// Input DTO for querying school transport routes with filters.
/// </summary>
public class GetSchoolTransportsInput : PagedAndSortedResultRequestDto
{
    public TransportType? TransportType { get; set; }
    public bool? IsActive { get; set; }
    public string RouteName { get; set; }
    public string Keyword { get; set; }
}
