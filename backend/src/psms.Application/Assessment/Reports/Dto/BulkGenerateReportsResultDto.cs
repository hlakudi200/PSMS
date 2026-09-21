using System;
using System.Collections.Generic;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// The result of a bulk generation run (or a preview of one).
/// <para>
/// A batch never fails as a whole because one learner is blocked — the caller
/// gets a line per learner and decides what to do about the ones that did not
/// go through.
/// </para>
/// </summary>
public class BulkGenerateReportsResultDto
{
    public Guid ClassId { get; set; }

    public string ClassName { get; set; }

    public Guid? TermId { get; set; }

    public string TermName { get; set; }

    /// <summary>True when this was a preview and nothing was written.</summary>
    public bool IsPreview { get; set; }

    public int TotalStudents { get; set; }

    public int EligibleCount { get; set; }

    public int GeneratedCount { get; set; }

    public int SkippedCount { get; set; }

    public int BlockedCount { get; set; }

    public int FailedCount { get; set; }

    public List<BulkGenerateReportItemDto> Items { get; set; } = new List<BulkGenerateReportItemDto>();
}
