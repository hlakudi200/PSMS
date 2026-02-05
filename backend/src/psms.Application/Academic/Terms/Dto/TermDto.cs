using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Academic.Terms.Dto;

/// <summary>
/// Full DTO for Term entity.
/// </summary>
public class TermDto : FullAuditedEntityDto<Guid>
{
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }
    public SouthAfricanTermNumber TermNumber { get; set; }
    public string TermName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsCurrent { get; set; }

    public int TotalDays => (EndDate - StartDate).Days;
}
