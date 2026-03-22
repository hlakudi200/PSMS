using Abp.Application.Services.Dto;
using System;

namespace psms.Discipline.DisciplinaryCases.Dto;

/// <summary>
/// Lightweight list DTO for disciplinary cases.
/// </summary>
public class DisciplinaryCaseListDto : EntityDto<Guid>
{
    public string CaseNumber { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public DateTime IncidentDate { get; set; }
    public int IncidentCategory { get; set; }
    public int Severity { get; set; }
    public int Status { get; set; }
    public string ReportedByName { get; set; }
    public DateTime CreationTime { get; set; }
}
