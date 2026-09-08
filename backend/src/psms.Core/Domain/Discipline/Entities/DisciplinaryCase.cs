using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Discipline.Entities;

[Table("DisciplinaryCases")]
public class DisciplinaryCase : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
{
    public int? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string CaseNumber { get; set; }

    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public DateTime IncidentDate { get; set; }

    [Required]
    [StringLength(2000)]
    public string IncidentDescription { get; set; }

    [Required]
    public DisciplinaryCategory IncidentCategory { get; set; }

    [Required]
    public DisciplinarySeverity Severity { get; set; }

    [Required]
    public DisciplinaryStatus Status { get; set; }

    [StringLength(200)]
    public string Location { get; set; }

    [StringLength(500)]
    public string WitnessNames { get; set; }

    [Required]
    public long ReportedByUserId { get; set; }

    [StringLength(256)]
    public string ReportedByName { get; set; }

    [StringLength(2000)]
    public string InvestigationNotes { get; set; }

    public DateTime? HearingDate { get; set; }

    [StringLength(2000)]
    public string HearingNotes { get; set; }

    public DisciplinaryOutcome? Outcome { get; set; }

    [StringLength(1000)]
    public string OutcomeDescription { get; set; }

    public DateTime? SanctionStartDate { get; set; }
    public DateTime? SanctionEndDate { get; set; }

    public bool ParentNotified { get; set; }
    public DateTime? ParentNotifiedDate { get; set; }

    [StringLength(1000)]
    public string AppealNotes { get; set; }

    public long? ResolvedByUserId { get; set; }
    public DateTime? ResolvedDate { get; set; }

    [ForeignKey(nameof(StudentId))]
    public virtual Student Student { get; set; }

    [ForeignKey(nameof(AcademicYearId))]
    public virtual AcademicYear AcademicYear { get; set; }

    protected DisciplinaryCase() { }

    public DisciplinaryCase(Guid id, int? tenantId, string caseNumber, Guid studentId,
        Guid academicYearId, DateTime incidentDate, string incidentDescription,
        DisciplinaryCategory category, DisciplinarySeverity severity,
        long reportedByUserId, string reportedByName)
    {
        Id = id;
        TenantId = tenantId;
        CaseNumber = caseNumber;
        StudentId = studentId;
        AcademicYearId = academicYearId;
        IncidentDate = incidentDate;
        IncidentDescription = incidentDescription;
        IncidentCategory = category;
        Severity = severity;
        ReportedByUserId = reportedByUserId;
        ReportedByName = reportedByName;
        Status = DisciplinaryStatus.Draft;
        ParentNotified = false;
    }

    public void Submit()
    {
        if (Status != DisciplinaryStatus.Draft)
            throw new InvalidOperationException("Only draft cases can be submitted.");
        Status = DisciplinaryStatus.Reported;
    }

    /// <summary>
    /// WF-31: the approval workflow was cancelled or recalled before a decision —
    /// return the case to the reporter as a draft. Cases past investigation keep
    /// their evidence and stay where they are.
    /// </summary>
    public void ReopenAsDraft()
    {
        if (Status != DisciplinaryStatus.Reported && Status != DisciplinaryStatus.UnderInvestigation)
            throw new InvalidOperationException("Only reported or under-investigation cases can be reopened.");
        Status = DisciplinaryStatus.Draft;
    }

    public void StartInvestigation()
    {
        if (Status != DisciplinaryStatus.Reported)
            throw new InvalidOperationException("Case must be reported to start investigation.");
        Status = DisciplinaryStatus.UnderInvestigation;
    }

    public void ScheduleHearing(DateTime hearingDate)
    {
        if (Status != DisciplinaryStatus.UnderInvestigation)
            throw new InvalidOperationException("Investigation must be in progress to schedule hearing.");
        HearingDate = hearingDate;
        Status = DisciplinaryStatus.HearingScheduled;
    }

    public void RecordOutcome(DisciplinaryOutcome outcome, string description, long userId)
    {
        if (Status != DisciplinaryStatus.HearingScheduled)
            throw new InvalidOperationException("Hearing must be scheduled to record outcome.");
        Outcome = outcome;
        OutcomeDescription = description;
        ResolvedByUserId = userId;
        Status = DisciplinaryStatus.HearingCompleted;
    }

    public void NotifyParent()
    {
        ParentNotified = true;
        ParentNotifiedDate = DateTime.UtcNow;
    }

    public void Resolve(long userId)
    {
        if (Status != DisciplinaryStatus.HearingCompleted)
            throw new InvalidOperationException("Hearing must be completed to resolve case.");
        ResolvedByUserId = userId;
        ResolvedDate = DateTime.UtcNow;
        Status = DisciplinaryStatus.Resolved;
    }

    public void Appeal(string notes)
    {
        if (Status != DisciplinaryStatus.Resolved)
            throw new InvalidOperationException("Only resolved cases can be appealed.");
        AppealNotes = notes;
        Status = DisciplinaryStatus.Appealed;
    }

    public void Cancel()
    {
        if (Status == DisciplinaryStatus.Resolved || Status == DisciplinaryStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel a resolved or already cancelled case.");
        Status = DisciplinaryStatus.Cancelled;
    }
}
