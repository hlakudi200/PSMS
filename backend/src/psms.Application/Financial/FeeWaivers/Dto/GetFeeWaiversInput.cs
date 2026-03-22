using Abp.Application.Services.Dto;
using System;

namespace psms.Financial.FeeWaivers.Dto;

/// <summary>
/// Input DTO for querying fee waivers with filters.
/// </summary>
public class GetFeeWaiversInput : PagedAndSortedResultRequestDto
{
    public Guid? StudentId { get; set; }
    public Guid? AcademicYearId { get; set; }
    public int? WaiverType { get; set; }
    public int? Status { get; set; }
    public string Search { get; set; }
}
