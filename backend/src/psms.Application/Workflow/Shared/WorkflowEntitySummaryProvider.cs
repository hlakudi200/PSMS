using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Academic.Entities;
using psms.Domain.Activities.Entities;
using psms.Domain.Admissions.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Discipline.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.HR.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Workflow.Enums;
using psms.Workflow.WorkflowInstances.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Shared;

/// <summary>
/// WF-09/WF-18: builds the full record a workflow instance is about, so an
/// approver has enough evidence to decide inline (deep-linking to a record page
/// isn't viable — most entity types have no detail page). Comprehensive,
/// grouped fields per WorkflowEntityType plus key child-record tables (report
/// subject marks; application documents + guardians). All reads are
/// tenant-filtered; returns null for an unmapped type or a missing entity.
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

    private async Task<WorkflowEntitySummaryDto> ApplicationSummary(Guid id)
    {
        // Single round-trip: the application + its grade + parents + documents.
        var a = await _applicationRepository
            .GetAll()
            .Include(x => x.AppliedGrade)
            .Include(x => x.ApplicantParents)
            .Include(x => x.ApplicationDocuments)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return null;
        var dto = Doc($"Application {a.ApplicationNumber}".Trim(),
            $"{a.ProspectiveStudentFirstName} {a.ProspectiveStudentLastName}".Trim());

        var applicant = Sec("Applicant");
        F(applicant, "Full Name", Join(a.ProspectiveStudentFirstName, a.ProspectiveStudentMiddleName, a.ProspectiveStudentLastName));
        F(applicant, "Date of Birth", Date(a.DateOfBirth));
        F(applicant, "Gender", a.Gender.ToString());
        F(applicant, "ID Number", a.IdNumber);
        F(applicant, "Passport", a.PassportNumber);
        F(applicant, "SA Citizen", Bool(a.IsSACitizen));
        Add(dto, applicant);

        var academic = Sec("Academic");
        F(academic, "Applied Grade", a.AppliedGrade?.GradeName);
        F(academic, "Previous School", a.PreviousSchool);
        Add(dto, academic);

        var status = Sec("Status");
        F(status, "Status", a.Status.ToString());
        F(status, "Applied", Date(a.ApplicationDate));
        F(status, "Submitted", Date(a.SubmissionDate));
        F(status, "Decision", a.DecisionReason);
        F(status, "Decision Date", Date(a.DecisionDate));
        Add(dto, status);

        var parents = (a.ApplicantParents ?? new List<ApplicantParent>())
            .OrderByDescending(p => p.IsPrimaryContact).ToList();
        if (parents.Any())
        {
            var t = new WorkflowEntitySummaryTableDto
            {
                Heading = "Parents / Guardians",
                Columns = new List<string> { "Name", "Relationship", "Contact", "Primary" },
            };
            foreach (var p in parents)
                t.Rows.Add(new List<string>
                {
                    Join(p.FirstName, p.LastName),
                    Relationship(p.Relationship),
                    p.Email ?? p.PhoneNumber ?? "—",
                    Bool(p.IsPrimaryContact),
                });
            dto.Tables.Add(t);
        }

        var docs = (a.ApplicationDocuments ?? new List<ApplicationDocument>())
            .OrderBy(d => d.DocumentName).ToList();
        if (docs.Any())
        {
            var t = new WorkflowEntitySummaryTableDto
            {
                Heading = "Documents",
                Columns = new List<string> { "Document", "Required", "Verified", "Uploaded" },
            };
            foreach (var d in docs)
                t.Rows.Add(new List<string>
                {
                    d.DocumentName ?? d.FileName ?? "—",
                    Bool(d.IsRequired),
                    Bool(d.IsVerified),
                    Date(d.UploadedDate),
                });
            dto.Tables.Add(t);
        }

        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> ReportSummary(Guid id)
    {
        // Single round-trip: the report + its student + subject marks (+ subject names).
        var r = await _reportRepository
            .GetAll()
            .Include(x => x.Student)
            .Include(x => x.SubjectReports).ThenInclude(rs => rs.Subject)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (r == null) return null;
        var dto = Doc("Report Card", FormatStudent(r.Student) ?? r.StudentId.ToString());

        var result = Sec("Result");
        F(result, "Status", r.Status.ToString());
        F(result, "Overall", r.OverallPercentage.HasValue ? $"{r.OverallPercentage.Value:N1}%" : null);
        F(result, "Class Position", r.ClassPosition.HasValue
            ? (r.TotalStudentsInClass > 0 ? $"{r.ClassPosition} / {r.TotalStudentsInClass}" : $"{r.ClassPosition}")
            : null);
        F(result, "Generated", Date(r.GeneratedDate));
        F(result, "Published", Date(r.PublishedDate));
        Add(dto, result);

        var attendance = Sec("Attendance");
        F(attendance, "Present", r.DaysPresent.ToString());
        F(attendance, "Absent", r.DaysAbsent.ToString());
        F(attendance, "Late", r.DaysLate.ToString());
        Add(dto, attendance);

        var comments = Sec("Comments");
        F(comments, "Teacher", r.TeacherComment);
        F(comments, "Principal", r.PrincipalComment);
        Add(dto, comments);

        var subjects = (r.SubjectReports ?? new List<ReportSubject>()).ToList();
        if (subjects.Any())
        {
            var t = new WorkflowEntitySummaryTableDto
            {
                Heading = "Subject Marks",
                Columns = new List<string> { "Subject", "Term", "Exam", "Final", "Position" },
            };
            foreach (var s in subjects.OrderBy(s => s.Subject?.SubjectName ?? ""))
                t.Rows.Add(new List<string>
                {
                    s.Subject?.SubjectName ?? "—",
                    Num(s.TermMark),
                    Num(s.ExamMark),
                    Num(s.FinalMark),
                    s.SubjectPosition?.ToString() ?? "—",
                });
            dto.Tables.Add(t);
        }

        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> FeeWaiverSummary(Guid id)
    {
        var w = await _feeWaiverRepository.FirstOrDefaultAsync(id);
        if (w == null) return null;
        var dto = Doc("Fee Waiver Request", await StudentName(w.StudentId));
        var s = Sec("Request");
        F(s, "Status", w.Status.ToString());
        F(s, "Requested Amount", Money(w.RequestedAmount));
        F(s, "Approved Amount", Money(w.ApprovedAmount));
        F(s, "Reason", w.Reason);
        F(s, "Review Notes", w.ReviewNotes);
        Add(dto, s);
        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> DisciplinarySummary(Guid id)
    {
        var c = await _disciplinaryCaseRepository.FirstOrDefaultAsync(id);
        if (c == null) return null;
        var dto = Doc($"Disciplinary Case {c.CaseNumber}".Trim(), await StudentName(c.StudentId));

        var incident = Sec("Incident");
        F(incident, "Status", c.Status.ToString());
        F(incident, "Incident Date", Date(c.IncidentDate));
        F(incident, "Location", c.Location);
        F(incident, "Reported By", c.ReportedByName);
        F(incident, "Witnesses", c.WitnessNames);
        F(incident, "Description", c.IncidentDescription);
        Add(dto, incident);

        var process = Sec("Investigation & Hearing");
        F(process, "Investigation Notes", c.InvestigationNotes);
        F(process, "Hearing Date", Date(c.HearingDate));
        F(process, "Hearing Notes", c.HearingNotes);
        F(process, "Outcome", c.OutcomeDescription);
        Add(dto, process);

        var sanction = Sec("Sanction");
        F(sanction, "Starts", Date(c.SanctionStartDate));
        F(sanction, "Ends", Date(c.SanctionEndDate));
        F(sanction, "Parent Notified", c.ParentNotified ? Date(c.ParentNotifiedDate) ?? "Yes" : "No");
        Add(dto, sanction);

        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> TransferSummary(Guid id)
    {
        var t = await _transferRepository.FirstOrDefaultAsync(id);
        if (t == null) return null;
        var dto = Doc($"Transfer {t.TransferNumber}".Trim(), await StudentName(t.StudentId));
        var s = Sec("Transfer");
        F(s, "Status", t.Status.ToString());
        F(s, "From", t.FromSchoolName);
        F(s, "To", t.ToSchoolName);
        F(s, "Requested", Date(t.RequestedDate));
        F(s, "Effective", Date(t.EffectiveDate));
        F(s, "Reason", t.Reason);
        F(s, "Notes", t.Notes);
        Add(dto, s);
        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> LeaveSummary(Guid id)
    {
        var l = await _leaveRepository.FirstOrDefaultAsync(id);
        if (l == null) return null;
        var dto = Doc($"Leave Request {l.LeaveNumber}".Trim(), l.UserName);
        var s = Sec("Leave");
        F(s, "Status", l.Status.ToString());
        F(s, "Period", $"{Date(l.StartDate)} → {Date(l.EndDate)}");
        F(s, "Reason", l.Reason);
        F(s, "Substitute", l.SubstituteTeacherName);
        F(s, "Rejection Reason", l.RejectionReason);
        Add(dto, s);
        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> FieldTripSummary(Guid id)
    {
        var t = await _fieldTripRepository.FirstOrDefaultAsync(id);
        if (t == null) return null;
        var dto = Doc($"Field Trip: {t.TripName}".Trim(), t.Destination);

        var trip = Sec("Trip");
        F(trip, "Status", t.Status.ToString());
        F(trip, "Destination", t.Destination);
        F(trip, "Trip Date", Date(t.TripDate));
        F(trip, "Return", Date(t.ReturnDate));
        Add(dto, trip);

        var logistics = Sec("Logistics");
        F(logistics, "Students", t.NumberOfStudents.ToString());
        F(logistics, "Chaperones", t.NumberOfChaperones.ToString());
        F(logistics, "Transport", t.TransportArrangement);
        F(logistics, "Estimated Cost", Money(t.EstimatedCost));
        F(logistics, "Approved Budget", Money(t.ApprovedBudget));
        Add(dto, logistics);

        var safety = Sec("Safety & Details");
        F(safety, "Risk Assessment", t.RiskAssessmentNotes);
        F(safety, "Emergency Plan", t.EmergencyPlan);
        F(safety, "Description", t.Description);
        Add(dto, safety);

        return dto;
    }

    private async Task<WorkflowEntitySummaryDto> ExpenseSummary(Guid id)
    {
        var e = await _expenseRepository.FirstOrDefaultAsync(id);
        if (e == null) return null;
        var who = string.IsNullOrWhiteSpace(e.Department) ? e.RequestedByName : $"{e.RequestedByName} · {e.Department}";
        var dto = Doc($"Expense {e.RequestNumber}".Trim(), who);
        var currency = string.IsNullOrWhiteSpace(e.Currency) ? "R" : e.Currency;

        var request = Sec("Request");
        F(request, "Status", e.Status.ToString());
        F(request, "Amount", $"{currency} {e.Amount:N2}");
        F(request, "Approved Amount", e.ApprovedAmount.HasValue ? $"{currency} {e.ApprovedAmount.Value:N2}" : null);
        F(request, "Vendor", e.Vendor);
        F(request, "Required By", Date(e.RequiredByDate));
        Add(dto, request);

        var detail = Sec("Detail");
        F(detail, "Description", e.Description);
        F(detail, "Payment Reference", e.PaymentReference);
        Add(dto, detail);

        return dto;
    }

    // ── helpers ───────────────────────────────────────────────────────────

    private static WorkflowEntitySummaryDto Doc(string title, string subtitle) =>
        new WorkflowEntitySummaryDto { Title = title, Subtitle = subtitle };

    private static WorkflowEntitySummarySectionDto Sec(string heading) =>
        new WorkflowEntitySummarySectionDto { Heading = heading };

    private static void F(WorkflowEntitySummarySectionDto section, string label, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            section.Fields.Add(new WorkflowEntitySummaryFieldDto(label, value));
    }

    private static void Add(WorkflowEntitySummaryDto dto, WorkflowEntitySummarySectionDto section)
    {
        if (section.Fields.Count > 0)
            dto.Sections.Add(section);
    }

    private async Task<string> StudentName(Guid studentId)
    {
        var s = await _studentRepository.FirstOrDefaultAsync(studentId);
        return FormatStudent(s) ?? studentId.ToString();
    }

    private static string FormatStudent(Student s)
    {
        if (s == null) return null;
        var name = $"{s.FirstName} {s.LastName}".Trim();
        return string.IsNullOrWhiteSpace(s.AdmissionNumber) ? name : $"{name} ({s.AdmissionNumber})";
    }

    private static string Join(params string[] parts) =>
        string.Join(" ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));

    // Show the enum name, not a raw int, for relationship values not in the enum.
    private static string Relationship(RelationshipType r) =>
        Enum.IsDefined(typeof(RelationshipType), r) ? r.ToString() : "—";

    private static string Bool(bool v) => v ? "Yes" : "No";
    private static string Money(decimal? v) => v.HasValue ? $"R {v.Value:N2}" : null;
    private static string Num(decimal? v) => v.HasValue ? $"{v.Value:N1}" : "—";
    private static string Date(DateTime? d) => d?.ToString("yyyy-MM-dd");
    private static string Date(DateTime d) => d.ToString("yyyy-MM-dd");
}
