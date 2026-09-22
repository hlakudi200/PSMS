using Abp.Application.Services.Dto;
using psms.Assessment.ReportSubjects.Dto;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// Full DTO for a student report.
/// </summary>
public class ReportDto : FullAuditedEntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public Guid? TermId { get; set; }
    public Guid AcademicYearId { get; set; }
    public ReportType ReportType { get; set; }
    public decimal? OverallPercentage { get; set; }
    public CapsAchievementLevel? OverallAchievementLevel { get; set; }
    public int? ClassPosition { get; set; }
    public int? TotalStudentsInClass { get; set; }
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public int DaysLate { get; set; }
    public string TeacherComment { get; set; }
    public string PrincipalComment { get; set; }
    public string ParentComment { get; set; }
    public DateTime? ParentAcknowledgedDate { get; set; }
    public PromotionDecision? PromotionDecision { get; set; }
    public Guid? PromotedToGradeId { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime? GeneratedDate { get; set; }
    public DateTime? PublishedDate { get; set; }
    public long? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }

    // Flattened
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
    public string ClassName { get; set; }
    public string TermName { get; set; }
    public string AcademicYearName { get; set; }
    public string PromotedToGradeName { get; set; }

    // Nested
    public List<ReportSubjectDto> SubjectReports { get; set; }

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
}
