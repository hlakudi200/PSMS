using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.AcademicYears.Dto;

/// <summary>
/// Lightweight DTO for academic year lists.
/// </summary>
public class AcademicYearListDto : EntityDto<Guid>
{
    public int Year { get; set; }
    public string YearName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public int TermCount { get; set; }
    public int ClassCount { get; set; }
}
