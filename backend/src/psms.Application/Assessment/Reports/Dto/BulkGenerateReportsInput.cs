using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// Input for generating report cards for every active learner in a class.
/// <para>
/// RC-01. The single-student <see cref="GenerateReportDto"/> takes the attendance
/// days by hand, which does not scale: one typed-in figure spread across a whole
/// class would put the same attendance on every report card. So the bulk path
/// reads attendance from the register instead, and only falls back to the
/// supplied defaults when there is no term to read a window from.
/// </para>
/// </summary>
public class BulkGenerateReportsInput
{
    [Required]
    public Guid ClassId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public ReportType ReportType { get; set; }

    /// <summary>Null for a year-end or mid-year report.</summary>
    public Guid? TermId { get; set; }

    /// <summary>
    /// Restrict the run to these learners. Empty or null means every active
    /// learner in the class — which is the usual end-of-term case. Supplying a
    /// subset is how the caller regenerates for one learner, or acts on just
    /// the learners a preview showed as eligible.
    /// </summary>
    public List<Guid> StudentIds { get; set; }

    /// <summary>
    /// Read each learner's attendance from the register over the term's date
    /// range rather than using the default days below. Ignored when
    /// <see cref="TermId"/> is null, because there is no window to read.
    /// </summary>
    public bool UseAttendanceRecords { get; set; } = true;

    [Range(0, 365)]
    public int DefaultDaysPresent { get; set; }

    [Range(0, 365)]
    public int DefaultDaysAbsent { get; set; }

    [Range(0, 365)]
    public int DefaultDaysLate { get; set; }
}
