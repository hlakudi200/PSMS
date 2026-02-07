using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.ExtramuralActivities.Dto;

/// <summary>
/// Input DTO for querying extramural activities with filters.
/// </summary>
public class GetExtramuralActivitiesInput : PagedAndSortedResultRequestDto
{
    public Guid? AcademicYearId { get; set; }
    public ExtramuralCategory? Category { get; set; }
    public ExtramuralType? ActivityType { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsRegistrationOpen { get; set; }
    public string ActivityName { get; set; }
}
