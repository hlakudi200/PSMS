using System;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// One learner's line in a bulk generation result.
/// </summary>
public class BulkGenerateReportItemDto
{
    public Guid StudentId { get; set; }

    public string StudentName { get; set; }

    public string AdmissionNumber { get; set; }

    public BulkGenerateOutcome Outcome { get; set; }

    /// <summary>Why, when the outcome is Skipped, Blocked or Failed. Null otherwise.</summary>
    public string Message { get; set; }

    /// <summary>Set when a report was generated, or when one already existed.</summary>
    public Guid? ReportId { get; set; }
}
