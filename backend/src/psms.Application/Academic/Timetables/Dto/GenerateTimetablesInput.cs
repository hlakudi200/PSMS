using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Timetables.Dto;

public class GenerateTimetablesInput
{
    /// <summary>Generate timetables for every active class in this academic year.</summary>
    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public DateTime EffectiveDate { get; set; }

    /// <summary>Days of the week to schedule periods on. Defaults to Monday–Friday.</summary>
    public List<DayOfWeek> WorkingDays { get; set; } = new()
    {
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday
    };

    [Range(1, 12)]
    public int PeriodsPerDay { get; set; } = 8;

    public TimeSpan PeriodStartTime { get; set; } = new TimeSpan(8, 0, 0);

    [Range(15, 120)]
    public int PeriodDurationMinutes { get; set; } = 40;

    /// <summary>
    /// Period numbers after which a break is inserted (e.g. [3, 6] for
    /// periods 1-3, a break, periods 4-6, a break, then the remaining
    /// periods). Empty/null = no breaks, periods run back-to-back.
    /// </summary>
    public List<int> BreakAfterPeriods { get; set; } = new();

    [Range(0, 120)]
    public int BreakDurationMinutes { get; set; } = 30;
}
