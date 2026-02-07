using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.SASpecific.AfterCares.Dto;

/// <summary>
/// Input DTO for creating an after-care program.
/// </summary>
public class CreateAfterCareDto
{
    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    [StringLength(200)]
    public string ProgramName { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    [Required]
    public AfterCareType AfterCareType { get; set; }

    [StringLength(200)]
    public string Location { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public string DaysAvailable { get; set; }

    [Range(0, int.MaxValue)]
    public int Capacity { get; set; }

    [StringLength(100)]
    public string SupervisorName { get; set; }

    [StringLength(20)]
    public string ContactPhone { get; set; }

    [StringLength(2000)]
    public string ActivitiesIncluded { get; set; }

    public bool IncludesMeals { get; set; }

    public bool IncludesHomeworkSupervision { get; set; }

    [Required]
    [Range(0, 9999999.99)]
    public decimal MonthlyFee { get; set; }
}
