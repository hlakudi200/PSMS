using System;
using psms.Domain.Assessment.Entities;

namespace psms.Assessment.Reports.Pdf;

/// <summary>
/// Where a report card's PDF is kept.
/// <para>
/// One card has one place, so rebuilding it replaces the file rather than
/// leaving an older copy behind under a second name. Both callers that produce
/// a PDF — the background job and the rebuild on download — have to agree on
/// that path, which is why it is worked out here and not in either of them.
/// </para>
/// </summary>
public static class ReportPdfStorage
{
    public static string PathFor(Report report, string admissionNumber) =>
        PathFor(report.TenantId, report.AcademicYearId, report.TermId, report.Id, admissionNumber);

    public static string PathFor(
        int? tenantId, Guid academicYearId, Guid? termId, Guid reportId, string admissionNumber)
    {
        var learner = string.IsNullOrWhiteSpace(admissionNumber) ? "unknown" : admissionNumber;

        return $"reports/{tenantId ?? 0}/{academicYearId}/{termId ?? Guid.Empty}/{learner}_{reportId}.pdf";
    }
}
