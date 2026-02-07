using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// Input DTO for generating a student report.
/// </summary>
public class GenerateReportDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid ClassId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public ReportType ReportType { get; set; }

    public Guid? TermId { get; set; }

    [Range(0, 365)]
    public int DaysPresent { get; set; }

    [Range(0, 365)]
    public int DaysAbsent { get; set; }

    [Range(0, 365)]
    public int DaysLate { get; set; }

    [StringLength(2000)]
    public string TeacherComment { get; set; }
}
