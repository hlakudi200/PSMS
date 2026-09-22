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
    public string PdfUrl { get; set; }

    // Flattened
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
    public string ClassName { get; set; }
    public string TermName { get; set; }
    public string AcademicYearName { get; set; }

    // Computed
    public int SubjectCount { get; set; }

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
}
