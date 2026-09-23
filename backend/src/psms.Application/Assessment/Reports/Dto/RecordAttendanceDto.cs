using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// RC-17. Input for correcting the attendance on a report card.
/// <para>
/// RE-002 reconciles days present and days absent against the days the
/// learner's attendance is measured over. Nothing could change any of them
/// after generation, so a card generated before the register was captured had
/// no way to be put right.
/// </para>
/// </summary>
public class RecordAttendanceDto
{
    [Required]
    public Guid ReportId { get; set; }

    [Range(0, 400)]
    public int DaysPresent { get; set; }

    [Range(0, 400)]
    public int DaysAbsent { get; set; }

    [Range(0, 400)]
    public int DaysLate { get; set; }

    /// <summary>
    /// The school days the two figures are measured over. Left out, it is taken
    /// to be present plus absent — which is what it must be when nobody is
    /// reading a register.
    /// </summary>
    [Range(0, 400)]
    public int? DaysInTerm { get; set; }
}
