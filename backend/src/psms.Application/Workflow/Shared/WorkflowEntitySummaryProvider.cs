using Abp.Dependency;
using Abp.Domain.Repositories;
using psms.Domain.Academic.Entities;
using psms.Domain.Activities.Entities;
using psms.Domain.Admissions.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Discipline.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.HR.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.WorkflowInstances.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Workflow.Shared;

/// <summary>
/// WF-09: builds a human-readable summary of the entity behind a workflow
/// instance so an approver sees WHAT they're approving (the report card, leave
/// request, expense, etc.) rather than a raw GUID. Reuses the same per-type
/// entity repositories as the bridge service; all reads are tenant-filtered.
/// Returns null when the type is unmapped or the entity no longer exists, so the
/// UI simply omits the summary card (the instance card already shows type + id).
/// </summary>
public class WorkflowEntitySummaryProvider : ITransientDependency
{
    private readonly IRepository<Application, Guid> _applicationRepository;
    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly IRepository<FeeWaiver, Guid> _feeWaiverRepository;
    private readonly IRepository<DisciplinaryCase, Guid> _disciplinaryCaseRepository;
    private readonly IRepository<StudentTransferRequest, Guid> _transferRepository;
    private readonly IRepository<StaffLeaveRequest, Guid> _leaveRepository;
    private readonly IRepository<FieldTrip, Guid> _fieldTripRepository;
    private readonly IRepository<ExpenseRequest, Guid> _expenseRepository;
    private readonly IRepository<Student, Guid> _studentRepository;

    public WorkflowEntitySummaryProvider(
        IRepository<Application, Guid> applicationRepository,
        IRepository<Report, Guid> reportRepository,
        IRepository<FeeWaiver, Guid> feeWaiverRepository,
        IRepository<DisciplinaryCase, Guid> disciplinaryCaseRepository,
        IRepository<StudentTransferRequest, Guid> transferRepository,
        IRepository<StaffLeaveRequest, Guid> leaveRepository,
        IRepository<FieldTrip, Guid> fieldTripRepository,
        IRepository<ExpenseRequest, Guid> expenseRepository,
        IRepository<Student, Guid> studentRepository)
    {
        _applicationRepository = applicationRepository;
        _reportRepository = reportRepository;
        _feeWaiverRepository = feeWaiverRepository;
        _disciplinaryCaseRepository = disciplinaryCaseRepository;
        _transferRepository = transferRepository;
        _leaveRepository = leaveRepository;
        _fieldTripRepository = fieldTripRepository;
        _expenseRepository = expenseRepository;
        _studentRepository = studentRepository;
    }

    public async Task<WorkflowEntitySummaryDto> GetSummaryAsync(WorkflowEntityType entityType, Guid entityId)
    {
        switch (entityType)
        {
            case WorkflowEntityType.Report: return await ReportSummary(entityId);
            case WorkflowEntityType.FeeWaiver: return await FeeWaiverSummary(entityId);
            case WorkflowEntityType.Disciplinary: return await DisciplinarySummary(entityId);
            case WorkflowEntityType.StudentTransfer: return await TransferSummary(entityId);
            case WorkflowEntityType.StaffLeave: return await LeaveSummary(entityId);
            case WorkflowEntityType.FieldTrip: return await FieldTripSummary(entityId);
            case WorkflowEntityType.ExpenseRequest: return await ExpenseSummary(entityId);
            case WorkflowEntityType.Application: return await ApplicationSummary(entityId);
            default: return null;
        }
    }

    // ── per-type projections ──────────────────────────────────────────────

    private async Task<WorkflowEntitySummaryDto> ReportSummary(Guid id)
    {
        var r = await _reportRepository.FirstOrDefaultAsync(id);
        if (r == null) return null;
        var dto = New("Report Card", await StudentName(r.StudentId));
        Add(dto, "Status", r.Status.ToString());
        Add(dto, "Overall", r.OverallPercentage.HasValue ? $"{r.OverallPercentage.Value:N1}%" : null);
        Add(dto, "Class Position", r.ClassPosition.HasValue
            ? (r.TotalStudentsInClass > 0 ? $"{r.ClassPosition} / {r.TotalStudentsInClass}" : $"{r.ClassPosition}")
            : null);
        Add(dto, "Attendance", $"{r.DaysPresent} present · {r.DaysAbsent} absent · {r.DaysLate} late");
        Add(dto, "Generated", Date(r.GeneratedDate));
        Add(dto, "Teacher Comment", Truncate(r.TeacherComment, 240));
        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> FeeWaiverSummary(Guid id)
    {
        var w = await _feeWaiverRepository.FirstOrDefaultAsync(id);
        if (w == null) return null;
        var dto = New("Fee Waiver Request", await StudentName(w.StudentId));
        Add(dto, "Status", w.Status.ToString());
        Add(dto, "Requested Amount", Money(w.RequestedAmount));
        Add(dto, "Reason", Truncate(w.Reason, 240));
        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> DisciplinarySummary(Guid id)
    {
        var c = await _disciplinaryCaseRepository.FirstOrDefaultAsync(id);
        if (c == null) return null;
        var dto = New($"Disciplinary Case {c.CaseNumber}".Trim(), await StudentName(c.StudentId));
        Add(dto, "Status", c.Status.ToString());
        Add(dto, "Incident Date", Date(c.IncidentDate));
        Add(dto, "Location", c.Location);
        Add(dto, "Reported By", c.ReportedByName);
        Add(dto, "Incident", Truncate(c.IncidentDescription, 300));
        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> TransferSummary(Guid id)
    {
        var t = await _transferRepository.FirstOrDefaultAsync(id);
        if (t == null) return null;
        var dto = New($"Transfer {t.TransferNumber}".Trim(), await StudentName(t.StudentId));
        Add(dto, "Status", t.Status.ToString());
        Add(dto, "From", t.FromSchoolName);
        Add(dto, "To", t.ToSchoolName);
        Add(dto, "Requested", Date(t.RequestedDate));
        Add(dto, "Reason", Truncate(t.Reason, 240));
        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> LeaveSummary(Guid id)
    {
        var l = await _leaveRepository.FirstOrDefaultAsync(id);
        if (l == null) return null;
        var dto = New($"Leave Request {l.LeaveNumber}".Trim(), l.UserName);
        Add(dto, "Status", l.Status.ToString());
        Add(dto, "Period", $"{Date(l.StartDate)} → {Date(l.EndDate)}");
        Add(dto, "Substitute", l.SubstituteTeacherName);
        Add(dto, "Reason", Truncate(l.Reason, 240));
        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> FieldTripSummary(Guid id)
    {
        var t = await _fieldTripRepository.FirstOrDefaultAsync(id);
        if (t == null) return null;
        var dto = New($"Field Trip: {t.TripName}".Trim(), t.Destination);
        Add(dto, "Status", t.Status.ToString());
        Add(dto, "Trip Date", Date(t.TripDate));
        Add(dto, "Students", t.NumberOfStudents.ToString());
        Add(dto, "Chaperones", t.NumberOfChaperones.ToString());
        Add(dto, "Estimated Cost", Money(t.EstimatedCost));
        Add(dto, "Details", Truncate(t.Description, 240));
        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> ExpenseSummary(Guid id)
    {
        var e = await _expenseRepository.FirstOrDefaultAsync(id);
        if (e == null) return null;
        var who = string.IsNullOrWhiteSpace(e.Department) ? e.RequestedByName : $"{e.RequestedByName} · {e.Department}";
        var dto = New($"Expense {e.RequestNumber}".Trim(), who);
        var currency = string.IsNullOrWhiteSpace(e.Currency) ? "R" : e.Currency;
        Add(dto, "Status", e.Status.ToString());
        Add(dto, "Amount", $"{currency} {e.Amount:N2}");
        Add(dto, "Vendor", e.Vendor);
        Add(dto, "Required By", Date(e.RequiredByDate));
        Add(dto, "Description", Truncate(e.Description, 240));
        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> ApplicationSummary(Guid id)
    {
        var a = await _applicationRepository.FirstOrDefaultAsync(id);
        if (a == null) return null;
        var name = $"{a.ProspectiveStudentFirstName} {a.ProspectiveStudentLastName}".Trim();
        var dto = New($"Application {a.ApplicationNumber}".Trim(), name);
        Add(dto, "Status", a.Status.ToString());
        Add(dto, "Applied", Date(a.ApplicationDate));
        Add(dto, "Date of Birth", Date(a.DateOfBirth));
        Add(dto, "Previous School", a.PreviousSchool);
        Add(dto, "Submitted", Date(a.SubmissionDate));
        return dto;
    }

    // ── helpers ───────────────────────────────────────────────────────────

    private static WorkflowEntitySummaryDto New(string title, string subtitle) =>
        new WorkflowEntitySummaryDto { Title = title, Subtitle = subtitle, Fields = new List<WorkflowEntitySummaryFieldDto>() };

    private static void Add(WorkflowEntitySummaryDto dto, string label, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            dto.Fields.Add(new WorkflowEntitySummaryFieldDto(label, value));
    }

    private async Task<string> StudentName(Guid studentId)
    {
        var s = await _studentRepository.FirstOrDefaultAsync(studentId);
        if (s == null) return studentId.ToString();
        var name = $"{s.FirstName} {s.LastName}".Trim();
        return string.IsNullOrWhiteSpace(s.AdmissionNumber) ? name : $"{name} ({s.AdmissionNumber})";
    }

    private static string Money(decimal? v) => v.HasValue ? $"R {v.Value:N2}" : null;
    private static string Date(DateTime? d) => d?.ToString("yyyy-MM-dd");
    private static string Date(DateTime d) => d.ToString("yyyy-MM-dd");

    private static string Truncate(string s, int max) =>
        string.IsNullOrWhiteSpace(s) ? null : (s.Length <= max ? s : s.Substring(0, max) + "…");
}
