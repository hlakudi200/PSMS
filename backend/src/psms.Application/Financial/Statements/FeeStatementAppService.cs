using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Timing;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Parents;
using psms.Academic.Students;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Shared.Storage;
using psms.Financial.Shared;
using psms.Financial.Statements.Dto;
using psms.Shared.Pdf;

namespace psms.Financial.Statements;

/// <summary>
/// MOB-BE-01 (#215). Fee statements, gated on <c>Financial.Statements</c>.
/// <para>
/// A parent holds the statement permissions but none of the fee-structure ones,
/// and must not be given them: <c>Financial.FeeStructures.View</c> also governs
/// fee administration. So this service reads fees and payments itself and hands
/// back only the statement, scoped to the caller:
/// <list type="bullet">
/// <item>a parent sees their own children — one of them, or all together;</item>
/// <item>a student (should one ever hold the permission) sees only themselves;</item>
/// <item>staff name the learner.</item>
/// </list>
/// A learner outside that scope gets the same "not found" as one that does not
/// exist, so the response does not confirm who is enrolled.
/// </para>
/// </summary>
[AbpAuthorize(PermissionNames.Financial_Statements)]
public class FeeStatementAppService : ApplicationService, IFeeStatementAppService
{
    private const int PdfLinkLifetimeSeconds = 300;

    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<Parent, Guid> _parentRepository;
    private readonly IRepository<StudentFee, Guid> _studentFeeRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;
    private readonly IRepository<Term, Guid> _termRepository;
    private readonly ICurrentParentResolver _currentParent;
    private readonly ICurrentStudentResolver _currentStudent;
    private readonly SchoolLetterheadLoader _letterheadLoader;
    private readonly IFileStorageService _fileStorage;

    public FeeStatementAppService(
        IRepository<Student, Guid> studentRepository,
        IRepository<Parent, Guid> parentRepository,
        IRepository<StudentFee, Guid> studentFeeRepository,
        IRepository<Payment, Guid> paymentRepository,
        IRepository<AcademicYear, Guid> academicYearRepository,
        IRepository<Term, Guid> termRepository,
        ICurrentParentResolver currentParent,
        ICurrentStudentResolver currentStudent,
        SchoolLetterheadLoader letterheadLoader,
        IFileStorageService fileStorage)
    {
        _studentRepository = studentRepository;
        _parentRepository = parentRepository;
        _studentFeeRepository = studentFeeRepository;
        _paymentRepository = paymentRepository;
        _academicYearRepository = academicYearRepository;
        _termRepository = termRepository;
        _currentParent = currentParent;
        _currentStudent = currentStudent;
        _letterheadLoader = letterheadLoader;
        _fileStorage = fileStorage;
    }

    [AbpAuthorize(PermissionNames.Financial_Statements_View)]
    public async Task<FeeStatementDto> GetStatementAsync(GetFeeStatementInput input)
    {
        var scope = await ResolveScopeAsync(input.StudentId);
        var period = await ResolvePeriodAsync(input);
        return await BuildAsync(scope, period);
    }

    [AbpAuthorize(PermissionNames.Financial_Statements_Generate)]
    public async Task<string> GenerateStatementPdfAsync(GetFeeStatementInput input)
    {
        var scope = await ResolveScopeAsync(input.StudentId);
        var period = await ResolvePeriodAsync(input);
        var statement = await BuildAsync(scope, period);

        string addressedTo = null;
        if (scope.ParentId.HasValue)
        {
            var parent = await _parentRepository.FirstOrDefaultAsync(scope.ParentId.Value);
            addressedTo = parent?.GetFullName();
        }

        var letterhead = await _letterheadLoader.LoadAsync(AbpSession.TenantId, includeStamp: false);
        var pdf = FeeStatementPdfGenerator.Generate(statement, letterhead, addressedTo);

        // One place per statement, so generating it again replaces the file
        // rather than piling up copies. Built on request, never served stale.
        var objectKey = await _fileStorage.UploadAsync(
            StoragePath(scope, statement), pdf, "application/pdf");

        // Private file, short-lived link — the same rule as report cards (RC-04).
        return await _fileStorage.CreateSignedDownloadUrlAsync(
            _fileStorage.DefaultBucketName, objectKey, PdfLinkLifetimeSeconds);
    }

    [AbpAuthorize(PermissionNames.Financial_Statements_View)]
    public async Task<List<FeeStatementYearDto>> GetPeriodsAsync()
    {
        var years = await _academicYearRepository
            .GetAll()
            .Include(y => y.Terms)
            .Where(y => y.TenantId == AbpSession.TenantId)
            .OrderByDescending(y => y.StartDate)
            .ToListAsync();

        return years.Select(y => new FeeStatementYearDto
        {
            AcademicYearId = y.Id,
            YearName = y.YearName,
            StartDate = y.StartDate,
            EndDate = y.EndDate,
            IsCurrent = y.IsCurrent,
            Terms = (y.Terms ?? new List<Term>())
                .Where(t => !t.IsDeleted)
                .OrderBy(t => t.StartDate)
                .Select(t => new FeeStatementTermDto
                {
                    TermId = t.Id,
                    TermName = t.TermName,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    IsCurrent = t.IsCurrent,
                })
                .ToList(),
        }).ToList();
    }

    private record StatementScope(List<Guid> StudentIds, Guid? ParentId);

    private async Task<StatementScope> ResolveScopeAsync(Guid? studentId)
    {
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue)
        {
            if (studentId.HasValue && studentId.Value != selfId.Value)
                throw StudentNotFound();
            return new StatementScope(new List<Guid> { selfId.Value }, null);
        }

        // MOB-BE-04: a parent is scoped to their own children.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null)
        {
            var parentId = await _currentParent.GetCurrentParentIdAsync();
            if (!studentId.HasValue)
                return new StatementScope(childIds, parentId);
            if (!childIds.Contains(studentId.Value))
                throw StudentNotFound();
            return new StatementScope(new List<Guid> { studentId.Value }, parentId);
        }

        if (!studentId.HasValue)
            throw new UserFriendlyException(FinancialExceptionCodes.StatementStudentRequired,
                "Choose the learner whose statement you want.");

        var exists = await _studentRepository.GetAll()
            .AnyAsync(s => s.Id == studentId.Value && s.TenantId == AbpSession.TenantId);
        if (!exists)
            throw StudentNotFound();

        return new StatementScope(new List<Guid> { studentId.Value }, null);
    }

    private static UserFriendlyException StudentNotFound() =>
        new(FinancialExceptionCodes.StudentNotFound, "Student not found.");

    private async Task<StatementPeriod> ResolvePeriodAsync(GetFeeStatementInput input)
    {
        var hasRange = input.FromDate.HasValue || input.ToDate.HasValue;
        var chosen = (input.TermId.HasValue ? 1 : 0) + (input.AcademicYearId.HasValue ? 1 : 0) + (hasRange ? 1 : 0);
        if (chosen > 1)
            throw new UserFriendlyException(FinancialExceptionCodes.StatementPeriodInvalid,
                "Choose a term, an academic year or a date range — only one of them.");

        if (input.TermId.HasValue)
        {
            var term = await _termRepository.GetAll()
                .Include(t => t.AcademicYear)
                .FirstOrDefaultAsync(t => t.Id == input.TermId.Value && t.TenantId == AbpSession.TenantId);
            if (term == null)
                throw new UserFriendlyException(FinancialExceptionCodes.TermNotFound, "Term not found.");

            var label = string.Join(" · ", new[] { term.TermName, term.AcademicYear?.YearName }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
            return new StatementPeriod(term.StartDate, term.EndDate, label);
        }

        if (input.AcademicYearId.HasValue)
        {
            var year = await _academicYearRepository
                .FirstOrDefaultAsync(y => y.Id == input.AcademicYearId.Value && y.TenantId == AbpSession.TenantId);
            if (year == null)
                throw new UserFriendlyException(FinancialExceptionCodes.AcademicYearNotFound, "Academic year not found.");

            return new StatementPeriod(year.StartDate, year.EndDate, year.YearName);
        }

        if (hasRange)
        {
            if (!input.FromDate.HasValue || !input.ToDate.HasValue)
                throw new UserFriendlyException(FinancialExceptionCodes.StatementPeriodInvalid,
                    "A date range needs both a start and an end date.");
            if (input.FromDate.Value.Date > input.ToDate.Value.Date)
                throw new UserFriendlyException(FinancialExceptionCodes.StatementPeriodInvalid,
                    "The start date must be on or before the end date.");

            var from = input.FromDate.Value.Date;
            var to = input.ToDate.Value.Date;
            return new StatementPeriod(from, to, $"{Day(from)} – {Day(to)}");
        }

        // Nothing chosen: the current academic year, or this calendar year to
        // date for a school that has not marked one as current.
        var current = await _academicYearRepository
            .FirstOrDefaultAsync(y => y.IsCurrent && y.TenantId == AbpSession.TenantId);
        if (current != null)
            return new StatementPeriod(current.StartDate, current.EndDate, current.YearName);

        var today = Clock.Now.Date;
        return new StatementPeriod(new DateTime(today.Year, 1, 1), today,
            today.Year.ToString(CultureInfo.InvariantCulture));
    }

    private async Task<FeeStatementDto> BuildAsync(StatementScope scope, StatementPeriod period)
    {
        var ids = scope.StudentIds;
        var tenantId = AbpSession.TenantId;

        var students = await _studentRepository.GetAll()
            .Where(s => ids.Contains(s.Id) && s.TenantId == tenantId)
            .Select(s => new StatementStudent(
                s.Id,
                s.FirstName + " " + s.LastName,
                s.AdmissionNumber,
                s.CurrentGrade != null ? s.CurrentGrade.GradeName : null))
            .ToListAsync();

        // Every fee, not just the period's: the opening balance is everything
        // that came before.
        var fees = await _studentFeeRepository.GetAll()
            .Where(f => ids.Contains(f.StudentId) && f.TenantId == tenantId)
            .Select(f => new StatementFee(
                f.Id,
                f.StudentId,
                f.FeeStructure != null ? f.FeeStructure.FeeName : "Fee",
                f.CreationTime,
                f.DueDate,
                f.AmountDue,
                f.DiscountAmount,
                f.Status,
                // Waiving only flips the status, so the last change is the
                // closest record of when it happened.
                f.Status == FeeStatus.Waived ? f.LastModificationTime : null,
                f.FeeStructure != null ? f.FeeStructure.Currency : null))
            .ToListAsync();

        var paymentRows = await _paymentRepository.GetAll()
            .Include(p => p.PaymentAllocations)
            .Where(p => ids.Contains(p.StudentId) && p.TenantId == tenantId
                && (p.Status == PaymentStatus.Completed || p.Status == PaymentStatus.Refunded))
            .ToListAsync();

        var payments = paymentRows.Select(p => new StatementPayment(
                p.Id,
                p.StudentId,
                p.PaymentDate,
                p.Amount,
                p.ReceiptNumber,
                p.Status,
                // Refunding likewise only flips the status.
                p.Status == PaymentStatus.Refunded ? p.LastModificationTime : null,
                p.PaymentAllocations
                    .Select(a => new StatementAllocation(a.StudentFeeId, a.Amount, a.AllocatedDate))
                    .ToList()))
            .ToList();

        return FeeStatementBuilder.Build(period, Clock.Now, students, fees, payments);
    }

    private string StoragePath(StatementScope scope, FeeStatementDto statement)
    {
        var who = scope.ParentId.HasValue
            ? $"parent-{scope.ParentId.Value}" + (scope.StudentIds.Count == 1 ? $"/{scope.StudentIds[0]}" : "/family")
            : $"student-{scope.StudentIds[0]}";

        return $"statements/{AbpSession.TenantId ?? 0}/{who}/"
            + $"{statement.PeriodStart:yyyyMMdd}-{statement.PeriodEnd:yyyyMMdd}.pdf";
    }

    private static string Day(DateTime date) => date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
}
