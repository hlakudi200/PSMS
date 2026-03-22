using Abp.Application.Services.Dto;
using System;

namespace psms.Discipline.DisciplinaryCases.Dto;

/// <summary>
/// Full DTO for a disciplinary case.
/// </summary>
public class DisciplinaryCaseDto : FullAuditedEntityDto<Guid>
{
    public string CaseNumber { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }
    public DateTime IncidentDate { get; set; }
    public string IncidentDescription { get; set; }
    public int IncidentCategory { get; set; }
    public int Severity { get; set; }
    public int Status { get; set; }
    public string Location { get; set; }
    public string WitnessNames { get; set; }
    public long ReportedByUserId { get; set; }
    public string ReportedByName { get; set; }
    public string InvestigationNotes { get; set; }
    public DateTime? HearingDate { get; set; }
    public string HearingNotes { get; set; }
    public int? Outcome { get; set; }
    public string OutcomeDescription { get; set; }
    public DateTime? SanctionStartDate { get; set; }
    public DateTime? SanctionEndDate { get; set; }
    public bool ParentNotified { get; set; }
    public DateTime? ParentNotifiedDate { get; set; }
    public string AppealNotes { get; set; }
    public long? ResolvedByUserId { get; set; }
    public DateTime? ResolvedDate { get; set; }
}
