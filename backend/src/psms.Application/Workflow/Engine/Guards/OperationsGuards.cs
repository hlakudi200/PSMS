using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Academic.Entities;
using psms.Domain.Activities.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Discipline.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.HR.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Workflow.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Engine.Guards;

/// <summary>Fee Waiver step 1: hardship and bursary requests must carry a supporting document.</summary>
public class FeeWaiverSupportingDocumentGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<FeeWaiver, Guid> _waivers;
    public FeeWaiverSupportingDocumentGuard(IRepository<FeeWaiver, Guid> waivers) { _waivers = waivers; }

    public string Key => "feewaiver.supporting-document-attached";
    public WorkflowEntityType EntityType => WorkflowEntityType.FeeWaiver;
    public string DisplayName => "Supporting document attached (hardship / bursary)";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var w = await _waivers.GetAll().Where(x => x.Id == entityId)
            .Select(x => new { x.WaiverType, x.SupportingDocumentUrl }).FirstOrDefaultAsync();
        if (w == null) return WorkflowGuardResult.Fail("Fee waiver not found.");
        var needsDocument = w.WaiverType == FeeWaiverType.FinancialHardship || w.WaiverType == FeeWaiverType.Bursary;
        if (needsDocument && string.IsNullOrWhiteSpace(w.SupportingDocumentUrl))
            return WorkflowGuardResult.Fail("A supporting document is required for hardship and bursary waivers.");
        return WorkflowGuardResult.Ok();
    }
}

/// <summary>Field Trip step 1: risk assessment, emergency plan and transport are recorded.</summary>
public class FieldTripSafetyCompleteGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<FieldTrip, Guid> _trips;
    public FieldTripSafetyCompleteGuard(IRepository<FieldTrip, Guid> trips) { _trips = trips; }

    public string Key => "fieldtrip.safety-complete";
    public WorkflowEntityType EntityType => WorkflowEntityType.FieldTrip;
    public string DisplayName => "Risk assessment, emergency plan and transport recorded";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var t = await _trips.GetAll().Where(x => x.Id == entityId)
            .Select(x => new { x.RiskAssessmentNotes, x.EmergencyPlan, x.TransportArrangement }).FirstOrDefaultAsync();
        if (t == null) return WorkflowGuardResult.Fail("Field trip not found.");
        var missing = new[]
        {
            string.IsNullOrWhiteSpace(t.RiskAssessmentNotes) ? "risk assessment" : null,
            string.IsNullOrWhiteSpace(t.EmergencyPlan) ? "emergency plan" : null,
            string.IsNullOrWhiteSpace(t.TransportArrangement) ? "transport arrangement" : null,
        }.Where(m => m != null).ToList();
        return missing.Count == 0
            ? WorkflowGuardResult.Ok()
            : WorkflowGuardResult.Fail($"Missing: {string.Join(", ", missing)}.");
    }
}

/// <summary>Field Trip step 1: at least one chaperone per <see cref="MaxLearnersPerChaperone"/> learners.</summary>
public class FieldTripChaperoneRatioGuard : IWorkflowStepGuard, ITransientDependency
{
    /// <summary>Default supervision ratio; a tenant setting can replace this in a later ticket.</summary>
    public const int MaxLearnersPerChaperone = 15;

    private readonly IRepository<FieldTrip, Guid> _trips;
    public FieldTripChaperoneRatioGuard(IRepository<FieldTrip, Guid> trips) { _trips = trips; }

    public string Key => "fieldtrip.chaperone-ratio";
    public WorkflowEntityType EntityType => WorkflowEntityType.FieldTrip;
    public string DisplayName => $"Chaperone ratio at least 1:{MaxLearnersPerChaperone}";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var t = await _trips.GetAll().Where(x => x.Id == entityId)
            .Select(x => new { x.NumberOfStudents, x.NumberOfChaperones }).FirstOrDefaultAsync();
        if (t == null) return WorkflowGuardResult.Fail("Field trip not found.");
        if (t.NumberOfChaperones <= 0) return WorkflowGuardResult.Fail("No chaperones have been assigned.");
        var required = (int)Math.Ceiling(t.NumberOfStudents / (double)MaxLearnersPerChaperone);
        return t.NumberOfChaperones >= required
            ? WorkflowGuardResult.Ok()
            : WorkflowGuardResult.Fail($"{t.NumberOfStudents} learners need at least {required} chaperone(s); {t.NumberOfChaperones} assigned.");
    }
}

/// <summary>Expense step 1: a quotation is attached for requests above <see cref="QuotationThreshold"/>.</summary>
public class ExpenseQuotationAttachedGuard : IWorkflowStepGuard, ITransientDependency
{
    /// <summary>Default threshold in ZAR; a tenant setting can replace this in a later ticket.</summary>
    public const decimal QuotationThreshold = 5000m;

    private readonly IRepository<ExpenseRequest, Guid> _expenses;
    public ExpenseQuotationAttachedGuard(IRepository<ExpenseRequest, Guid> expenses) { _expenses = expenses; }

    public string Key => "expense.quotation-attached";
    public WorkflowEntityType EntityType => WorkflowEntityType.ExpenseRequest;
    public string DisplayName => $"Quotation attached (above R{QuotationThreshold:N0})";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var e = await _expenses.GetAll().Where(x => x.Id == entityId)
            .Select(x => new { x.Amount, x.QuotationUrl }).FirstOrDefaultAsync();
        if (e == null) return WorkflowGuardResult.Fail("Expense request not found.");
        if (e.Amount >= QuotationThreshold && string.IsNullOrWhiteSpace(e.QuotationUrl))
            return WorkflowGuardResult.Fail($"Requests of R{QuotationThreshold:N0} or more need a quotation attached.");
        return WorkflowGuardResult.Ok();
    }
}

/// <summary>Staff Leave step 1: a substitute teacher is arranged for planned leave types.</summary>
public class LeaveSubstituteArrangedGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<StaffLeaveRequest, Guid> _leaves;
    public LeaveSubstituteArrangedGuard(IRepository<StaffLeaveRequest, Guid> leaves) { _leaves = leaves; }

    public string Key => "leave.substitute-arranged";
    public WorkflowEntityType EntityType => WorkflowEntityType.StaffLeave;
    public string DisplayName => "Substitute teacher arranged (planned leave)";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var l = await _leaves.GetAll().Where(x => x.Id == entityId)
            .Select(x => new { x.LeaveType, x.SubstituteTeacherId }).FirstOrDefaultAsync();
        if (l == null) return WorkflowGuardResult.Fail("Leave request not found.");
        var planned = l.LeaveType == StaffLeaveType.Annual || l.LeaveType == StaffLeaveType.Study
                      || l.LeaveType == StaffLeaveType.Unpaid || l.LeaveType == StaffLeaveType.Other;
        if (planned && !l.SubstituteTeacherId.HasValue)
            return WorkflowGuardResult.Fail("Planned leave needs a substitute teacher assigned.");
        return WorkflowGuardResult.Ok();
    }
}

/// <summary>Staff Leave step 1: a supporting document is attached where policy requires one.</summary>
public class LeaveDocumentAttachedGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<StaffLeaveRequest, Guid> _leaves;
    public LeaveDocumentAttachedGuard(IRepository<StaffLeaveRequest, Guid> leaves) { _leaves = leaves; }

    public string Key => "leave.document-attached";
    public WorkflowEntityType EntityType => WorkflowEntityType.StaffLeave;
    public string DisplayName => "Supporting document attached (sick > 2 days, maternity, paternity)";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var l = await _leaves.GetAll().Where(x => x.Id == entityId)
            .Select(x => new { x.LeaveType, x.StartDate, x.EndDate, x.SupportingDocumentUrl }).FirstOrDefaultAsync();
        if (l == null) return WorkflowGuardResult.Fail("Leave request not found.");
        var days = (l.EndDate.Date - l.StartDate.Date).TotalDays + 1;
        var needsDocument = l.LeaveType == StaffLeaveType.Maternity || l.LeaveType == StaffLeaveType.Paternity
                            || (l.LeaveType == StaffLeaveType.Sick && days > 2);
        if (needsDocument && string.IsNullOrWhiteSpace(l.SupportingDocumentUrl))
            return WorkflowGuardResult.Fail("This leave type and duration requires a supporting document.");
        return WorkflowGuardResult.Ok();
    }
}

/// <summary>Student Transfer step 1: schools, effective date and (for transfers in) the previous report are captured.</summary>
public class TransferDetailsCompleteGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<StudentTransferRequest, Guid> _transfers;
    public TransferDetailsCompleteGuard(IRepository<StudentTransferRequest, Guid> transfers) { _transfers = transfers; }

    public string Key => "transfer.details-complete";
    public WorkflowEntityType EntityType => WorkflowEntityType.StudentTransfer;
    public string DisplayName => "Transfer details complete (schools, date, previous report)";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var t = await _transfers.GetAll().Where(x => x.Id == entityId)
            .Select(x => new { x.TransferType, x.FromSchoolName, x.ToSchoolName, x.EffectiveDate, x.PreviousReportUrl }).FirstOrDefaultAsync();
        if (t == null) return WorkflowGuardResult.Fail("Transfer request not found.");
        var missing = new[]
        {
            string.IsNullOrWhiteSpace(t.FromSchoolName) ? "from school" : null,
            string.IsNullOrWhiteSpace(t.ToSchoolName) ? "to school" : null,
            !t.EffectiveDate.HasValue ? "effective date" : null,
            t.TransferType == TransferType.TransferIn && string.IsNullOrWhiteSpace(t.PreviousReportUrl) ? "previous school report" : null,
        }.Where(m => m != null).ToList();
        return missing.Count == 0
            ? WorkflowGuardResult.Ok()
            : WorkflowGuardResult.Fail($"Missing: {string.Join(", ", missing)}.");
    }
}

/// <summary>Disciplinary step 1: a hearing date has been scheduled (or the case was resolved without one).</summary>
public class DisciplineHearingScheduledGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<DisciplinaryCase, Guid> _cases;
    public DisciplineHearingScheduledGuard(IRepository<DisciplinaryCase, Guid> cases) { _cases = cases; }

    public string Key => "discipline.hearing-scheduled";
    public WorkflowEntityType EntityType => WorkflowEntityType.Disciplinary;
    public string DisplayName => "Hearing scheduled";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var status = await _cases.GetAll().Where(x => x.Id == entityId).Select(x => (DisciplinaryStatus?)x.Status).FirstOrDefaultAsync();
        if (status == null) return WorkflowGuardResult.Fail("Disciplinary case not found.");
        return status is DisciplinaryStatus.HearingScheduled or DisciplinaryStatus.HearingCompleted or DisciplinaryStatus.Resolved
            ? WorkflowGuardResult.Ok()
            : WorkflowGuardResult.Fail("A hearing date has not been scheduled.");
    }
}

/// <summary>Disciplinary step 2: the hearing outcome has been recorded.</summary>
public class DisciplineOutcomeRecordedGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<DisciplinaryCase, Guid> _cases;
    public DisciplineOutcomeRecordedGuard(IRepository<DisciplinaryCase, Guid> cases) { _cases = cases; }

    public string Key => "discipline.outcome-recorded";
    public WorkflowEntityType EntityType => WorkflowEntityType.Disciplinary;
    public string DisplayName => "Hearing outcome recorded";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var c = await _cases.GetAll().Where(x => x.Id == entityId).Select(x => new { x.Status, x.Outcome }).FirstOrDefaultAsync();
        if (c == null) return WorkflowGuardResult.Fail("Disciplinary case not found.");
        return c.Outcome.HasValue && (c.Status == DisciplinaryStatus.HearingCompleted || c.Status == DisciplinaryStatus.Resolved)
            ? WorkflowGuardResult.Ok()
            : WorkflowGuardResult.Fail("The hearing outcome has not been recorded.");
    }
}

/// <summary>Report Approval step 1: every subject on the report has a final mark and a teacher comment.</summary>
public class ReportSubjectsCompleteGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<ReportSubject, Guid> _subjects;
    public ReportSubjectsCompleteGuard(IRepository<ReportSubject, Guid> subjects) { _subjects = subjects; }

    public string Key => "report.subjects-complete";
    public WorkflowEntityType EntityType => WorkflowEntityType.Report;
    public string DisplayName => "All subjects have a final mark and teacher comment";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var subjects = await _subjects.GetAll().Where(s => s.ReportId == entityId)
            .Select(s => new { s.FinalMark, s.TeacherComment }).ToListAsync();
        if (subjects.Count == 0) return WorkflowGuardResult.Fail("The report has no subjects.");
        var noMark = subjects.Count(s => !s.FinalMark.HasValue);
        var noComment = subjects.Count(s => string.IsNullOrWhiteSpace(s.TeacherComment));
        if (noMark == 0 && noComment == 0) return WorkflowGuardResult.Ok();
        return WorkflowGuardResult.Fail(
            $"{(noMark > 0 ? $"{noMark} subject(s) without a final mark" : "")}{(noMark > 0 && noComment > 0 ? "; " : "")}{(noComment > 0 ? $"{noComment} subject(s) without a teacher comment" : "")}.");
    }
}
