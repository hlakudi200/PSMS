using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;

namespace psms.Academic.AcademicYears.Dto;

/// <summary>
/// Full DTO for AcademicYear entity.
/// </summary>
public class AcademicYearDto : FullAuditedEntityDto<Guid>
{
    public int Year { get; set; }
    public string YearName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }

    public int TermCount { get; set; }
    public int ClassCount { get; set; }

    public List<AcademicYearTermDto> Terms { get; set; }
}

/// <summary>
/// Nested term summary within AcademicYearDto.
/// </summary>
public class AcademicYearTermDto
{
    public Guid Id { get; set; }
    public int TermNumber { get; set; }
    public string TermName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }
}
