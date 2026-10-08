using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// Lightweight DTO for report lists.
/// </summary>
public class ReportListDto : EntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public Guid? TermId { get; set; }
    public Guid AcademicYearId { get; set; }
    public ReportType ReportType { get; set; }
    public decimal? OverallPercentage { get; set; }
    public CapsAchievementLevel? OverallAchievementLevel { get; set; }
    public int? ClassPosition { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime? GeneratedDate { get; set; }
    public DateTime? PublishedDate { get; set; }

    // Flattened
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
    public string ClassName { get; set; }
    public string TermName { get; set; }
    public string AcademicYearName { get; set; }

    // Computed
    public int SubjectCount { get; set; }

    /// <summary>
    /// Whether this learner's phase is reported in percentages at all.
    /// <para>
    /// RC-19, §17(4): Foundation Phase is reported on the 1–7 achievement
    /// scale, and the Protocol does not rank Foundation Phase learners against
    /// one another. The card and the printed PDF both honour that; the list did
    /// not, and showed a Grade R learner an overall percentage and a class
    /// position in two columns.
    /// </para>
    /// </summary>
    public bool ReportsPercentages { get; set; } = true;

    /// <summary>
    /// The live approval workflow for this report, when one is running.
    /// <para>
    /// RC-09: while this is set, the report is being approved through the engine
    /// and the direct Approve action is refused server-side. Callers use it to
    /// hide that action and link to the approval instead of offering a button
    /// that can only fail.
    /// </para>
    /// </summary>
    public Guid? ActiveWorkflowInstanceId { get; set; }

    /// <summary>
    /// Whether a PDF has been generated. The link itself is not exposed: it is
    /// minted per request and short-lived, so callers ask
    /// Report/GetReportPdfUrl when the user actually clicks download (RC-04).
    /// </summary>
    public bool HasPdf { get; set; }

    /// <summary>
    /// True when the signed-in user is the class teacher of this report's class
    /// — the one who signs the Class Teacher line. A teacher who teaches a
    /// subject in several classes only registers one of them, so without this
    /// the list gives them no way to tell which cards are theirs to sign.
    /// </summary>
    public bool IsMyRegisterClass { get; set; }
}
