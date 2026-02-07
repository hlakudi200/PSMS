using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.SASpecific.ExtramuralActivities.Dto;

/// <summary>
/// Input DTO for updating an extramural activity. All fields nullable (null-skip).
/// IsActive/IsRegistrationOpen are NOT included — use dedicated endpoints.
/// </summary>
public class UpdateExtramuralActivityDto
{
    [StringLength(200)]
    public string ActivityName { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    public ExtramuralCategory? Category { get; set; }

    public ExtramuralType? ActivityType { get; set; }

    [StringLength(200)]
    public string Venue { get; set; }

    public DayOfWeek? DayOfWeek { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public SchoolSeason? Season { get; set; }

    public int? TermNumber { get; set; }

    public int? MinGrade { get; set; }

    public int? MaxGrade { get; set; }

    public Gender? GenderRestriction { get; set; }

    [StringLength(100)]
    public string CoachName { get; set; }

    [StringLength(20)]
    public string CoachPhone { get; set; }

    [Range(1, int.MaxValue)]
    public int? MaxCapacity { get; set; }

    [StringLength(1000)]
    public string Requirements { get; set; }

    [Range(0, 9999999.99)]
    public decimal? FeePerTerm { get; set; }
}
