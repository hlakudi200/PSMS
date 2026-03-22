using Abp.Application.Services.Dto;
using System;

namespace psms.Discipline.DisciplinaryCases.Dto;

/// <summary>
/// Input DTO for querying disciplinary cases with filters.
/// </summary>
public class GetDisciplinaryCasesInput : PagedAndSortedResultRequestDto
{
    public Guid? StudentId { get; set; }
    public Guid? AcademicYearId { get; set; }
    public int? IncidentCategory { get; set; }
    public int? Severity { get; set; }
    public int? Status { get; set; }
    public string Search { get; set; }
}
