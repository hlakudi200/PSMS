using System;
using System.Collections.Generic;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// What happened to one learner in a bulk generation run, or what would happen
/// to them on a preview.
/// </summary>
public enum BulkGenerateOutcome
{
    /// <summary>Preview only: this learner would get a report card.</summary>
    Eligible = 1,

    /// <summary>A report card was created.</summary>
    Generated = 2,

    /// <summary>A report already exists for this learner, term and type. Left alone.</summary>
    SkippedExisting = 3,

    /// <summary>A business rule blocks this learner — incomplete marks, for example.</summary>
    Blocked = 4,

    /// <summary>Generation was attempted and threw.</summary>
    Failed = 5,
}

/// <summary>One learner's line in a bulk generation result.</summary>
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
