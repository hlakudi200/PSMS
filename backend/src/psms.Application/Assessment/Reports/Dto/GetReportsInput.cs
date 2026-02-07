using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// Input DTO for querying reports with optional filters.
/// </summary>
public class GetReportsInput : PagedAndSortedResultRequestDto
{
    public Guid? StudentId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? TermId { get; set; }
    public Guid? AcademicYearId { get; set; }
    public ReportType? ReportType { get; set; }
    public ReportStatus? Status { get; set; }
    public string StudentName { get; set; }
}
