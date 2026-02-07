using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.SASpecific.AfterCares.Dto;

/// <summary>
/// Input DTO for updating an after-care program. All fields nullable (null-skip).
/// IsActive is NOT included — use Activate/Deactivate endpoints.
/// </summary>
public class UpdateAfterCareDto
{
    [StringLength(200)]
    public string ProgramName { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    public AfterCareType? AfterCareType { get; set; }

    [StringLength(200)]
    public string Location { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public string DaysAvailable { get; set; }

    [Range(0, int.MaxValue)]
    public int? Capacity { get; set; }

    [StringLength(100)]
    public string SupervisorName { get; set; }

    [StringLength(20)]
    public string ContactPhone { get; set; }

    [StringLength(2000)]
    public string ActivitiesIncluded { get; set; }

    public bool? IncludesMeals { get; set; }

    public bool? IncludesHomeworkSupervision { get; set; }

    [Range(0, 9999999.99)]
    public decimal? MonthlyFee { get; set; }
}
