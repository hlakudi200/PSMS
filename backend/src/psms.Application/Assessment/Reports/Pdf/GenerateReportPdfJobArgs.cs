using System;
using System.Collections.Generic;

namespace psms.Assessment.Reports.Pdf;

/// <summary>
/// Arguments for the single report PDF generation background job.
/// </summary>
[Serializable]
public class GenerateReportPdfJobArgs
{
    public Guid ReportId { get; set; }
    public int? TenantId { get; set; }
    public long UserId { get; set; }
}

/// <summary>
/// Arguments for the bulk report PDF generation background job.
/// </summary>
[Serializable]
public class BulkGenerateReportPdfsJobArgs
{
    public List<Guid> ReportIds { get; set; }
    public int? TenantId { get; set; }
    public long UserId { get; set; }
}
