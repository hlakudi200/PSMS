using System;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// Input for bulk PDF generation — generates PDFs for all reports matching class and term.
/// </summary>
public class BulkGenerateReportPdfsInput
{
    public Guid ClassId { get; set; }
    public Guid? TermId { get; set; }
}
