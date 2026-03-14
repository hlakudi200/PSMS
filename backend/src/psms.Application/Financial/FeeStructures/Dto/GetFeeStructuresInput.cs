using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Financial.FeeStructures.Dto;

/// <summary>
/// Input DTO for querying fee structures with filters.
/// </summary>
public class GetFeeStructuresInput : PagedAndSortedResultRequestDto
{
    public Guid? GradeId { get; set; }
    public Guid? AcademicYearId { get; set; }
    public SouthAfricanFeeType? FeeType { get; set; }
    public bool? IsActive { get; set; }
    public string FeeName { get; set; }
    public string Keyword { get; set; }
}
