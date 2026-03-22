using Abp.Application.Services.Dto;
using System;

namespace psms.Activities.FieldTrips.Dto;

/// <summary>
/// Input DTO for querying field trips.
/// </summary>
public class GetFieldTripsInput : PagedAndSortedResultRequestDto
{
    public Guid? AcademicYearId { get; set; }
    public int? Status { get; set; }
    public Guid? OrganizingTeacherId { get; set; }
    public string Search { get; set; }
}
