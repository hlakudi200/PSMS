using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.ExtramuralActivities.Dto;

/// <summary>
/// Lightweight DTO for extramural activity lists.
/// </summary>
public class ExtramuralActivityListDto : EntityDto<Guid>
{
    public Guid AcademicYearId { get; set; }
    public string ActivityName { get; set; }
    public ExtramuralCategory Category { get; set; }
    public ExtramuralType ActivityType { get; set; }
    public string Venue { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public SchoolSeason? Season { get; set; }
    public int? MaxCapacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public string CoachName { get; set; }
    public decimal FeePerTerm { get; set; }
    public bool IsActive { get; set; }
    public bool IsRegistrationOpen { get; set; }

    // Computed
    public int EnrollmentCount { get; set; }
}
