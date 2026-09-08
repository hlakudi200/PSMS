using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Academic.PrincipalDashboard.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Activities.Entities;
using psms.Domain.Admissions.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Discipline.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.HR.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Academic.PrincipalDashboard;

/// <summary>
/// Server-side aggregation for the Principal / Vice Principal dashboard.
///
/// Why a dedicated endpoint: the dashboard previously pulled up to 1000 reports
/// and 500 classes to the browser just to compute per-grade averages, and had
/// no view of the approval queue at all. Everything here is a handful of
/// GROUP BY / COUNT queries.
///
/// Gated on its own management-dashboard permission: the payload crosses
/// finance, HR, discipline and admissions, so a role that merely has
/// school-wide student visibility (HOD, Finance) must not receive it.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Dashboard_View)]
public class PrincipalDashboardAppService : ApplicationService, IPrincipalDashboardAppService
{
    /// <summary>CAPS pass mark used for the per-grade pass rate.</summary>
    private const decimal PassMark = 50m;

    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<Teacher, Guid> _teacherRepository;
    private readonly IRepository<Class, Guid> _classRepository;
    private readonly IRepository<Grade, Guid> _gradeRepository;
    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly IRepository<Application, Guid> _applicationRepository;
    private readonly IRepository<DisciplinaryCase, Guid> _disciplinaryCaseRepository;
    private readonly IRepository<StaffLeaveRequest, Guid> _leaveRepository;
    private readonly IRepository<FieldTrip, Guid> _fieldTripRepository;
    private readonly IRepository<ExpenseRequest, Guid> _expenseRepository;
    private readonly IRepository<FeeWaiver, Guid> _feeWaiverRepository;
    private readonly IRepository<StudentTransferRequest, Guid> _transferRepository;
    private readonly IRepository<StudentFee, Guid> _studentFeeRepository;

    public PrincipalDashboardAppService(
        IRepository<Student, Guid> studentRepository,
        IRepository<Teacher, Guid> teacherRepository,
        IRepository<Class, Guid> classRepository,
        IRepository<Grade, Guid> gradeRepository,
        IRepository<Report, Guid> reportRepository,
        IRepository<Application, Guid> applicationRepository,
        IRepository<DisciplinaryCase, Guid> disciplinaryCaseRepository,
        IRepository<StaffLeaveRequest, Guid> leaveRepository,
        IRepository<FieldTrip, Guid> fieldTripRepository,
        IRepository<ExpenseRequest, Guid> expenseRepository,
        IRepository<FeeWaiver, Guid> feeWaiverRepository,
        IRepository<StudentTransferRequest, Guid> transferRepository,
        IRepository<StudentFee, Guid> studentFeeRepository)
    {
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _classRepository = classRepository;
        _gradeRepository = gradeRepository;
        _reportRepository = reportRepository;
        _applicationRepository = applicationRepository;
        _disciplinaryCaseRepository = disciplinaryCaseRepository;
        _leaveRepository = leaveRepository;
        _fieldTripRepository = fieldTripRepository;
        _expenseRepository = expenseRepository;
        _feeWaiverRepository = feeWaiverRepository;
        _transferRepository = transferRepository;
        _studentFeeRepository = studentFeeRepository;
    }

    public async Task<PrincipalDashboardSummaryDto> GetSummaryAsync(Guid? academicYearId = null)
    {
        var tenantId = AbpSession.TenantId;
        var summary = new PrincipalDashboardSummaryDto { PassMarkPercentage = PassMark };

        // --- Headline counts ---
        summary.TotalStudents = await _studentRepository.GetAll()
            .Where(s => s.TenantId == tenantId)
            .CountAsync(s => s.IsActive);
        summary.TotalTeachers = await _teacherRepository.GetAll()
            .Where(t => t.TenantId == tenantId)
            .CountAsync(t => t.IsActive);

        var activeClassQuery = _classRepository.GetAll().Where(c => c.TenantId == tenantId && c.IsActive);
        if (academicYearId.HasValue)
            activeClassQuery = activeClassQuery.Where(c => c.AcademicYearId == academicYearId.Value);
        summary.TotalClasses = await activeClassQuery.CountAsync();

        // --- Needs attention ---
        summary.PendingAdmissions = await _applicationRepository.GetAll()
            .Where(a => a.TenantId == tenantId)
            .CountAsync(a =>
                a.Status == ApplicationStatus.Submitted
                || a.Status == ApplicationStatus.UnderReview
                || a.Status == ApplicationStatus.DocumentsRequired
                || a.Status == ApplicationStatus.InterviewScheduled
                || a.Status == ApplicationStatus.AssessmentScheduled
                || a.Status == ApplicationStatus.UnderConsideration);

        summary.OpenDisciplinaryCases = await _disciplinaryCaseRepository.GetAll()
            .Where(c => c.TenantId == tenantId)
            .CountAsync(c =>
                c.Status == DisciplinaryStatus.Reported
                || c.Status == DisciplinaryStatus.UnderInvestigation
                || c.Status == DisciplinaryStatus.HearingScheduled
                || c.Status == DisciplinaryStatus.HearingCompleted
                || c.Status == DisciplinaryStatus.Appealed);

        summary.PendingLeaveRequests = await _leaveRepository.GetAll()
            .Where(l => l.TenantId == tenantId)
            .CountAsync(l => l.Status == StaffLeaveStatus.Submitted || l.Status == StaffLeaveStatus.HODApproved);

        summary.PendingFieldTrips = await _fieldTripRepository.GetAll()
            .Where(f => f.TenantId == tenantId)
            .CountAsync(f => f.Status == FieldTripStatus.Submitted || f.Status == FieldTripStatus.UnderReview);

        summary.PendingExpenses = await _expenseRepository.GetAll()
            .Where(e => e.TenantId == tenantId)
            .CountAsync(e => e.Status == ExpenseStatus.Submitted || e.Status == ExpenseStatus.UnderReview);

        summary.PendingFeeWaivers = await _feeWaiverRepository.GetAll()
            .Where(w => w.TenantId == tenantId)
            .CountAsync(w => w.Status == FeeWaiverStatus.Submitted || w.Status == FeeWaiverStatus.UnderReview);

        summary.PendingTransfers = await _transferRepository.GetAll()
            .Where(t => t.TenantId == tenantId)
            .CountAsync(t => t.Status == TransferStatus.Submitted || t.Status == TransferStatus.UnderReview);

        // --- Finance ---
        // Same formula as StudentFee.GetOutstandingBalance(): discounts never
        // reduce AmountDue, so they must be subtracted here too.
        var outstandingFees = _studentFeeRepository.GetAll().Where(f =>
            f.TenantId == tenantId
            && (f.Status == FeeStatus.Pending
                || f.Status == FeeStatus.PartiallyPaid
                || f.Status == FeeStatus.Overdue));
        summary.OutstandingFeesTotal = await outstandingFees
            .SumAsync(f => (decimal?)(f.AmountDue - f.DiscountAmount - f.AmountPaid)) ?? 0m;
        summary.OverdueFeesCount = await _studentFeeRepository.GetAll()
            .Where(f => f.TenantId == tenantId)
            .CountAsync(f => f.Status == FeeStatus.Overdue);

        // --- Per-grade performance ---
        var grades = await _gradeRepository.GetAll()
            .Where(g => g.TenantId == tenantId && g.IsActive)
            .OrderBy(g => g.GradeLevel)
            .Select(g => new { g.Id, g.GradeName, g.GradeLevel })
            .ToListAsync();

        // Students are grouped on their own CurrentGradeId (kept in step with
        // CurrentClassId by StudentAppService), so learners in a deactivated or
        // prior-year class still count toward their grade and the rows sum to
        // TotalStudents.
        var studentsPerGrade = (await _studentRepository.GetAll()
                .Where(s => s.TenantId == tenantId && s.IsActive)
                .GroupBy(s => s.CurrentGradeId)
                .Select(g => new { GradeId = g.Key, Count = g.Count() })
                .ToListAsync())
            .ToDictionary(x => x.GradeId, x => x.Count);

        // Reports carry ClassId only; map through EVERY class (not just active /
        // in-year ones) so a report on a since-deactivated class still rolls up.
        // Year scoping is applied to the report itself.
        var classToGrade = (await _classRepository.GetAll()
                .Where(c => c.TenantId == tenantId)
                .Select(c => new { c.Id, c.GradeId })
                .ToListAsync())
            .ToDictionary(c => c.Id, c => c.GradeId);

        var reportQuery = _reportRepository.GetAll()
            .Where(r => r.TenantId == tenantId && r.OverallPercentage != null);
        if (academicYearId.HasValue)
            reportQuery = reportQuery.Where(r => r.AcademicYearId == academicYearId.Value);
        var reportsPerClass = await reportQuery
            .GroupBy(r => r.ClassId)
            .Select(g => new
            {
                ClassId = g.Key,
                Count = g.Count(),
                Sum = g.Sum(r => r.OverallPercentage.Value),
                Passed = g.Count(r => r.OverallPercentage.Value >= PassMark),
            })
            .ToListAsync();

        var reportsPerGrade = reportsPerClass
            .Where(x => classToGrade.ContainsKey(x.ClassId))
            .GroupBy(x => classToGrade[x.ClassId])
            .ToDictionary(
                g => g.Key,
                g => new { Count = g.Sum(x => x.Count), Sum = g.Sum(x => x.Sum), Passed = g.Sum(x => x.Passed) });

        summary.GradePerformance = grades.Select(g =>
        {
            reportsPerGrade.TryGetValue(g.Id, out var rep);
            studentsPerGrade.TryGetValue(g.Id, out var studentCount);
            return new GradePerformanceDto
            {
                GradeId = g.Id,
                GradeName = g.GradeName,
                GradeLevel = (int)g.GradeLevel,
                StudentCount = studentCount,
                ReportCount = rep?.Count ?? 0,
                AveragePercentage = rep != null && rep.Count > 0 ? Math.Round(rep.Sum / rep.Count, 1) : null,
                PassRate = rep != null && rep.Count > 0 ? Math.Round(rep.Passed * 100m / rep.Count, 0) : null,
            };
        }).ToList();

        return summary;
    }
}
