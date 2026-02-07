using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.ExtramuralActivities.Dto;

/// <summary>
/// Full DTO for an extramural activity.
/// </summary>
public class ExtramuralActivityDto : FullAuditedEntityDto<Guid>
{
    public Guid AcademicYearId { get; set; }
    public string ActivityName { get; set; }
    public string Description { get; set; }
    public ExtramuralCategory Category { get; set; }
    public ExtramuralType ActivityType { get; set; }
    public string Venue { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public SchoolSeason? Season { get; set; }
    public int? TermNumber { get; set; }
    public int? MinGrade { get; set; }
    public int? MaxGrade { get; set; }
    public Gender? GenderRestriction { get; set; }
    public string CoachName { get; set; }
    public string CoachPhone { get; set; }
    public int? MaxCapacity { get; set; }
    public int CurrentEnrollment { get; set; }
    public string Requirements { get; set; }
    public decimal FeePerTerm { get; set; }
    public bool IsActive { get; set; }
    public bool IsRegistrationOpen { get; set; }

    // Computed
    public int EnrollmentCount { get; set; }
}
