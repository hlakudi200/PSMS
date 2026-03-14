using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.SASpecific.AfterCares.Dto;

/// <summary>
/// Input DTO for querying after-care programs with filters.
/// </summary>
public class GetAfterCaresInput : PagedAndSortedResultRequestDto
{
    public Guid? AcademicYearId { get; set; }
    public AfterCareType? AfterCareType { get; set; }
    public bool? IsActive { get; set; }
    public string ProgramName { get; set; }
    public string Keyword { get; set; }
}
