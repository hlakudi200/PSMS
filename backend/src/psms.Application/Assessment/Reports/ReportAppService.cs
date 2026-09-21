using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Assessment.Reports.Dto;
using psms.Assessment.Reports.Pdf;
using psms.Assessment.ReportSubjects.Dto;
using psms.Assessment.Shared;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AssessmentEntity = psms.Domain.Assessment.Entities.Assessment;

namespace psms.Assessment.Reports;

/// <summary>
/// Service for managing student reports (term and year-end report cards).
/// </summary>
[AbpAuthorize(PermissionNames.Assessment_ReportCards)]
public class ReportAppService : ApplicationService, IReportAppService
{
    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly IRepository<ReportSubject, Guid> _reportSubjectRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<Class, Guid> _classRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;
    private readonly IRepository<Term, Guid> _termRepository;
    private readonly IRepository<Mark, Guid> _markRepository;
    private readonly IRepository<ClassSubject, Guid> _classSubjectRepository;
    private readonly IRepository<AssessmentEntity, Guid> _assessmentRepository;
    private readonly IRepository<Attendance, Guid> _attendanceRepository;
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly psms.Academic.Students.ICurrentStudentResolver _currentStudent;

    public ReportAppService(
        IRepository<Report, Guid> reportRepository,
        IRepository<ReportSubject, Guid> reportSubjectRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<Class, Guid> classRepository,
        IRepository<AcademicYear, Guid> academicYearRepository,
        IRepository<Term, Guid> termRepository,
        IRepository<Mark, Guid> markRepository,
        IRepository<ClassSubject, Guid> classSubjectRepository,
        IRepository<AssessmentEntity, Guid> assessmentRepository,
        IRepository<Attendance, Guid> attendanceRepository,
        IBackgroundJobManager backgroundJobManager,
        psms.Academic.Students.ICurrentStudentResolver currentStudent)
    {
        _reportRepository = reportRepository;
        _reportSubjectRepository = reportSubjectRepository;
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _academicYearRepository = academicYearRepository;
        _termRepository = termRepository;
        _markRepository = markRepository;
        _classSubjectRepository = classSubjectRepository;
        _assessmentRepository = assessmentRepository;
        _attendanceRepository = attendanceRepository;
        _backgroundJobManager = backgroundJobManager;
        _currentStudent = currentStudent;
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_View)]
    public async Task<ReportDto> GetAsync(Guid id)
    {
        var report = await _reportRepository
            .GetAll()
            .Include(r => r.Student)
            .Include(r => r.Class)
            .Include(r => r.Term)
            .Include(r => r.AcademicYear)
            .Include(r => r.PromotedToGrade)
            .Include(r => r.SubjectReports).ThenInclude(sr => sr.Subject)
            .Include(r => r.SubjectReports).ThenInclude(sr => sr.Teacher)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        // LC-10: a student may only read their own report card.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != report.StudentId)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        var dto = ObjectMapper.Map<ReportDto>(report);
        dto.SubjectReports = ObjectMapper.Map<List<ReportSubjectDto>>(report.SubjectReports.OrderBy(sr => sr.Subject?.SubjectName).ToList());
        return dto;
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_View)]
    public async Task<PagedResultDto<ReportListDto>> GetAllAsync(GetReportsInput input)
    {
        // LC-10: a student-portal user only ever sees their own report cards.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();

        var query = _reportRepository
            .GetAll()
            .Include(r => r.Student)
            .Include(r => r.Class)
            .Include(r => r.Term)
            .Include(r => r.AcademicYear)
            .Include(r => r.SubjectReports)
            .Where(r => r.TenantId == AbpSession.TenantId)
            .WhereIf(selfId.HasValue, r => r.StudentId == selfId.Value)
            .WhereIf(input.StudentId.HasValue, r => r.StudentId == input.StudentId.Value)
            .WhereIf(input.ClassId.HasValue, r => r.ClassId == input.ClassId.Value)
            .WhereIf(input.TermId.HasValue, r => r.TermId == input.TermId.Value)
            .WhereIf(input.AcademicYearId.HasValue, r => r.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.ReportType.HasValue, r => r.ReportType == input.ReportType.Value)
            .WhereIf(input.Status.HasValue, r => r.Status == input.Status.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.StudentName),
                r => (r.Student.FirstName + " " + r.Student.LastName).ToLower()
                    .Contains(input.StudentName.Trim().ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                r => r.Student.FirstName.ToLower().Contains(input.Keyword.ToLower())
                    || r.Student.LastName.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "Student.LastName ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<ReportListDto>(
            totalCount,
            ObjectMapper.Map<List<ReportListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_View)]
    public async Task<ReportDto> GetByStudentTermAsync(Guid studentId, Guid termId, ReportType reportType)
    {
        // LC-10: a student may only read their own report card.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != studentId)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        var report = await _reportRepository
            .GetAll()
            .Include(r => r.Student)
            .Include(r => r.Class)
            .Include(r => r.Term)
            .Include(r => r.AcademicYear)
            .Include(r => r.PromotedToGrade)
            .Include(r => r.SubjectReports).ThenInclude(sr => sr.Subject)
            .Include(r => r.SubjectReports).ThenInclude(sr => sr.Teacher)
            .FirstOrDefaultAsync(r => r.TenantId == AbpSession.TenantId
                && r.StudentId == studentId
                && r.TermId == termId
                && r.ReportType == reportType);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        var dto = ObjectMapper.Map<ReportDto>(report);
        dto.SubjectReports = ObjectMapper.Map<List<ReportSubjectDto>>(report.SubjectReports.OrderBy(sr => sr.Subject?.SubjectName).ToList());
        return dto;
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Generate)]
    public async Task<ReportDto> GenerateAsync(GenerateReportDto input)
    {
        // Validate student exists
        var student = await _studentRepository.FirstOrDefaultAsync(s => s.Id == input.StudentId);
        if (student == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Student not found.");

        // Validate class exists
        var cls = await _classRepository.FirstOrDefaultAsync(c => c.Id == input.ClassId);
        if (cls == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Class not found.");

        // Validate student belongs to the specified class
        if (student.CurrentClassId != input.ClassId)
            throw new UserFriendlyException(AssessmentExceptionCodes.StudentNotInClass,
                "Student is not enrolled in the specified class.");

        // Validate academic year exists
        var academicYear = await _academicYearRepository.FirstOrDefaultAsync(ay => ay.Id == input.AcademicYearId);
        if (academicYear == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Academic year not found.");

        // Validate term if provided
        if (input.TermId.HasValue)
        {
            var term = await _termRepository.FirstOrDefaultAsync(t => t.Id == input.TermId.Value);
            if (term == null)
                throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Term not found.");
        }

        // Duplicate prevention
        var existing = await _reportRepository
            .GetAll()
            .Where(r => r.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(r => r.StudentId == input.StudentId
                && r.TermId == input.TermId
                && r.ReportType == input.ReportType);

        if (existing != null)
            throw new UserFriendlyException(AssessmentExceptionCodes.DuplicateReport,
                "A report already exists for this student, term, and report type.");

        // RE-001: Validate all marks for student/term are completed
        if (input.TermId.HasValue)
        {
            var incompleteMarks = await _markRepository
                .GetAll()
                .Where(m => m.TenantId == AbpSession.TenantId)
                .Where(m => m.StudentId == input.StudentId)
                .Where(m => m.Assessment.TermId == input.TermId.Value)
                .Where(m => m.Assessment.ClassSubject.ClassId == input.ClassId)
                .Where(m => m.Status != MarkStatus.Completed && m.Status != MarkStatus.Absent && m.Status != MarkStatus.Exempted)
                .AnyAsync();

            if (incompleteMarks)
                throw new UserFriendlyException(AssessmentExceptionCodes.IncompleteMarksForReport,
                    "Cannot generate report. There are incomplete marks for this student in the specified term.");
        }

        var report = await BuildReportAsync(
            input.StudentId,
            input.ClassId,
            input.AcademicYearId,
            input.ReportType,
            input.TermId,
            input.DaysPresent,
            input.DaysAbsent,
            input.DaysLate,
            input.TeacherComment);

        return await GetAsync(report.Id);
    }

    /// <summary>
    /// Creates one report card and its subject rows, pulling the marks. Shared by
    /// the single-student <see cref="GenerateAsync"/> and the bulk path (RC-01), so
    /// both produce identical reports — the callers differ only in how they
    /// validate and how they handle a failure.
    /// <para>
    /// The caller is responsible for validating the student, class, year and term,
    /// for duplicate prevention, and for the RE-001 completeness gate.
    /// </para>
    /// </summary>
    private async Task<Report> BuildReportAsync(
        Guid studentId,
        Guid classId,
        Guid academicYearId,
        ReportType reportType,
        Guid? termId,
        int daysPresent,
        int daysAbsent,
        int daysLate,
        string teacherComment)
    {
        var report = new Report(
            Guid.NewGuid(),
            AbpSession.TenantId,
            studentId,
            classId,
            academicYearId,
            reportType,
            termId)
        {
            DaysPresent = daysPresent,
            DaysAbsent = daysAbsent,
            DaysLate = daysLate,
            TeacherComment = teacherComment
        };

        await _reportRepository.InsertAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Auto-create ReportSubject entries for all class subjects
        var classSubjects = await _classSubjectRepository
            .GetAll()
            .Include(cs => cs.Subject)
            .Where(cs => cs.TenantId == AbpSession.TenantId)
            .Where(cs => cs.ClassId == classId && cs.IsActive)
            .ToListAsync();

        foreach (var classSubject in classSubjects)
        {
            var reportSubject = new ReportSubject(
                Guid.NewGuid(),
                report.Id,
                classSubject.SubjectId)
            {
                TeacherId = classSubject.TeacherId
            };

            // Aggregate mark percentages for this subject/term using weighted average
            if (termId.HasValue)
            {
                var subjectMarks = await _markRepository
                    .GetAll()
                    .Include(m => m.Assessment)
                    .Where(m => m.TenantId == AbpSession.TenantId)
                    .Where(m => m.StudentId == studentId)
                    .Where(m => m.Assessment.TermId == termId.Value)
                    .Where(m => m.Assessment.ClassSubject.SubjectId == classSubject.SubjectId)
                    .Where(m => m.Assessment.ClassSubject.ClassId == classId)
                    .Where(m => m.Status == MarkStatus.Completed && m.Percentage.HasValue)
                    .ToListAsync();

                if (subjectMarks.Any())
                {
                    decimal avgPercentage;
                    var totalWeight = subjectMarks.Sum(m => m.Assessment.Weight);

                    if (totalWeight > 0)
                    {
                        // Weighted average: sum(percentage * weight) / sum(weight)
                        avgPercentage = subjectMarks.Sum(m => m.Percentage.Value * m.Assessment.Weight) / totalWeight;
                    }
                    else
                    {
                        // Fallback to simple average when no weights are configured
                        avgPercentage = subjectMarks.Average(m => m.Percentage.Value);
                    }

                    reportSubject.RecordMarks(avgPercentage, null);
                }
            }

            await _reportSubjectRepository.InsertAsync(reportSubject);
        }

        // Calculate overall percentage from subject entries
        await CurrentUnitOfWork.SaveChangesAsync();

        // Reload and compute overall
        var savedSubjects = await _reportSubjectRepository
            .GetAll()
            .Where(rs => rs.ReportId == report.Id)
            .Where(rs => rs.FinalMark.HasValue)
            .ToListAsync();

        if (savedSubjects.Any())
        {
            report.OverallPercentage = savedSubjects.Average(rs => rs.FinalMark.Value);
            report.OverallAchievementLevel = CalculateAchievementLevel(report.OverallPercentage.Value);
        }

        // Tenant predicate is explicit here, as it is on every other query in this
        // service — relying on the ambient IMayHaveTenant filter alone made this
        // one count read differently from its neighbours.
        report.TotalStudentsInClass = await _studentRepository
            .CountAsync(s => s.TenantId == AbpSession.TenantId && s.CurrentClassId == classId);

        report.Generate();
        await _reportRepository.UpdateAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();

        return report;
    }

    /// <summary>
    /// RC-01. Shows what a bulk run would do without writing anything, so the
    /// actor can see who is blocked before committing to a class of forty.
    /// </summary>
    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Generate)]
    public Task<BulkGenerateReportsResultDto> PreviewBulkGenerateAsync(BulkGenerateReportsInput input)
    {
        return RunBulkGenerateAsync(input, previewOnly: true);
    }

    /// <summary>
    /// RC-01. Generates a report card for every active learner in a class.
    /// <para>
    /// One learner's problem does not fail the batch: an existing report is
    /// skipped, incomplete marks block just that learner, and an unexpected
    /// failure is recorded against that learner and the run continues. The
    /// caller gets a line per learner describing what happened.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Generate)]
    public Task<BulkGenerateReportsResultDto> BulkGenerateAsync(BulkGenerateReportsInput input)
    {
        return RunBulkGenerateAsync(input, previewOnly: false);
    }

    private async Task<BulkGenerateReportsResultDto> RunBulkGenerateAsync(
        BulkGenerateReportsInput input,
        bool previewOnly)
    {
        var cls = await _classRepository
            .FirstOrDefaultAsync(c => c.Id == input.ClassId && c.TenantId == AbpSession.TenantId);
        if (cls == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Class not found.");

        var academicYear = await _academicYearRepository
            .FirstOrDefaultAsync(ay => ay.Id == input.AcademicYearId && ay.TenantId == AbpSession.TenantId);
        if (academicYear == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Academic year not found.");

        Term term = null;
        if (input.TermId.HasValue)
        {
            term = await _termRepository
                .FirstOrDefaultAsync(t => t.Id == input.TermId.Value && t.TenantId == AbpSession.TenantId);
            if (term == null)
                throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Term not found.");
        }

        var subset = input.StudentIds != null && input.StudentIds.Count > 0
            ? input.StudentIds
            : null;

        var students = await _studentRepository
            .GetAll()
            .Where(s => s.TenantId == AbpSession.TenantId)
            .Where(s => s.CurrentClassId == input.ClassId && s.IsActive)
            .WhereIf(subset != null, s => subset.Contains(s.Id))
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();

        var result = new BulkGenerateReportsResultDto
        {
            ClassId = input.ClassId,
            ClassName = cls.ClassName,
            TermId = input.TermId,
            TermName = term?.TermName,
            IsPreview = previewOnly,
            TotalStudents = students.Count
        };

        if (students.Count == 0)
            return result;

        var studentIds = students.Select(s => s.Id).ToList();

        // Both lookups below are one query for the whole class rather than one
        // per learner — a class of forty would otherwise be eighty round trips.
        var existingReports = await _reportRepository
            .GetAll()
            .Where(r => r.TenantId == AbpSession.TenantId)
            .Where(r => r.TermId == input.TermId && r.ReportType == input.ReportType)
            .Where(r => studentIds.Contains(r.StudentId))
            .Select(r => new { r.Id, r.StudentId })
            .ToListAsync();

        var existingByStudent = existingReports
            .GroupBy(r => r.StudentId)
            .ToDictionary(g => g.Key, g => g.First().Id);

        // RE-001, evaluated per learner rather than for the batch: one learner
        // with an outstanding mark must not stop the other thirty-nine.
        var blockedStudentIds = new HashSet<Guid>();
        if (input.TermId.HasValue)
        {
            var incomplete = await _markRepository
                .GetAll()
                .Where(m => m.TenantId == AbpSession.TenantId)
                .Where(m => m.Assessment.TermId == input.TermId.Value)
                .Where(m => m.Assessment.ClassSubject.ClassId == input.ClassId)
                .Where(m => studentIds.Contains(m.StudentId))
                .Where(m => m.Status != MarkStatus.Completed
                    && m.Status != MarkStatus.Absent
                    && m.Status != MarkStatus.Exempted)
                .Select(m => m.StudentId)
                .Distinct()
                .ToListAsync();

            blockedStudentIds = new HashSet<Guid>(incomplete);
        }

        var attendanceByStudent = await ResolveAttendanceAsync(input, term, studentIds);

        foreach (var student in students)
        {
            var item = new BulkGenerateReportItemDto
            {
                StudentId = student.Id,
                StudentName = $"{student.FirstName} {student.LastName}".Trim(),
                AdmissionNumber = student.AdmissionNumber
            };

            if (existingByStudent.TryGetValue(student.Id, out var existingId))
            {
                item.Outcome = BulkGenerateOutcome.SkippedExisting;
                item.ReportId = existingId;
                item.Message = "A report already exists for this learner, term and report type.";
                result.SkippedCount++;
                result.Items.Add(item);
                continue;
            }

            if (blockedStudentIds.Contains(student.Id))
            {
                item.Outcome = BulkGenerateOutcome.Blocked;
                item.Message = "There are incomplete marks for this learner in the selected term.";
                result.BlockedCount++;
                result.Items.Add(item);
                continue;
            }

            if (previewOnly)
            {
                item.Outcome = BulkGenerateOutcome.Eligible;
                result.EligibleCount++;
                result.Items.Add(item);
                continue;
            }

            var days = attendanceByStudent.TryGetValue(student.Id, out var counted)
                ? counted
                : (input.DefaultDaysPresent, input.DefaultDaysAbsent, input.DefaultDaysLate);

            try
            {
                var report = await BuildReportAsync(
                    student.Id,
                    input.ClassId,
                    input.AcademicYearId,
                    input.ReportType,
                    input.TermId,
                    days.Item1,
                    days.Item2,
                    days.Item3,
                    teacherComment: null);

                item.Outcome = BulkGenerateOutcome.Generated;
                item.ReportId = report.Id;
                result.GeneratedCount++;
            }
            catch (UserFriendlyException ex)
            {
                item.Outcome = BulkGenerateOutcome.Failed;
                item.Message = ex.Message;
                result.FailedCount++;
            }

            result.Items.Add(item);
        }

        return result;
    }

    /// <summary>
    /// Reads each learner's attendance over the term's date range, so a class of
    /// report cards carries real attendance rather than one typed-in figure
    /// repeated forty times.
    /// <para>
    /// A late arrival still attended, so Late counts towards Present and is also
    /// reported on its own line — which is how the printed card reads it. Excused
    /// and SickLeave count as absent: the learner was not there, and the card has
    /// no separate line for the reason. Holiday is excluded from both totals — it
    /// is not a school day, so counting it either way would misstate the term.
    /// That keeps Present + Absent equal to the school days registered, which is
    /// what the PDF prints as the total.
    /// </para>
    /// Returns an empty map when there is no term to read a window from, or when
    /// the caller asked for the supplied defaults instead.
    /// </summary>
    private async Task<Dictionary<Guid, (int Present, int Absent, int Late)>> ResolveAttendanceAsync(
        BulkGenerateReportsInput input,
        Term term,
        List<Guid> studentIds)
    {
        var map = new Dictionary<Guid, (int, int, int)>();

        if (!input.UseAttendanceRecords || term == null)
            return map;

        var records = await _attendanceRepository
            .GetAll()
            .Where(a => a.TenantId == AbpSession.TenantId)
            .Where(a => a.ClassId == input.ClassId)
            .Where(a => studentIds.Contains(a.StudentId))
            .Where(a => a.AttendanceDate >= term.StartDate && a.AttendanceDate <= term.EndDate)
            .Where(a => a.Status != AttendanceStatus.Holiday)
            .Select(a => new { a.StudentId, a.Status })
            .ToListAsync();

        foreach (var group in records.GroupBy(r => r.StudentId))
        {
            var present = group.Count(r =>
                r.Status == AttendanceStatus.Present || r.Status == AttendanceStatus.Late);
            var absent = group.Count(r =>
                r.Status == AttendanceStatus.Absent
                || r.Status == AttendanceStatus.Excused
                || r.Status == AttendanceStatus.SickLeave);
            var late = group.Count(r => r.Status == AttendanceStatus.Late);
            map[group.Key] = (present, absent, late);
        }

        return map;
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Generate)]
    public async Task<ReportDto> SubmitForApprovalAsync(Guid id)
    {
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        try
        {
            report.SubmitForApproval();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(AssessmentExceptionCodes.InvalidReportStatusTransition,
                "Report must be in Generated status to submit for approval.");
        }

        await _reportRepository.UpdateAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Publish)]
    public async Task<ReportDto> ApproveAsync(Guid id)
    {
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        try
        {
            report.Approve(AbpSession.UserId.Value);
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(AssessmentExceptionCodes.InvalidReportStatusTransition,
                "Report must be in PendingApproval status to approve.");
        }

        await _reportRepository.UpdateAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Publish)]
    public async Task<ReportDto> PublishAsync(Guid id)
    {
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        try
        {
            report.Publish();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(AssessmentExceptionCodes.CannotPublishUnapproved,
                "Report must be approved before publishing.");
        }

        await _reportRepository.UpdateAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Generate)]
    public async Task<ReportDto> AddTeacherCommentAsync(Guid id, ReportCommentDto input)
    {
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        report.TeacherComment = input.Comment;
        await _reportRepository.UpdateAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Publish)]
    public async Task<ReportDto> AddPrincipalCommentAsync(Guid id, ReportCommentDto input)
    {
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        report.PrincipalComment = input.Comment;
        await _reportRepository.UpdateAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_View)]
    public async Task<ReportDto> AcknowledgeByParentAsync(Guid id, ReportCommentDto input)
    {
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        // LC-10: if a student-portal user reaches this, they may only act on
        // their own report (this is primarily a parent action).
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != report.StudentId)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        // Parent can only acknowledge a published report
        if (report.Status != ReportStatus.Published)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotPublished,
                "Report must be published before it can be acknowledged by a parent.");

        report.AcknowledgeByParent(input.Comment);
        await _reportRepository.UpdateAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Publish)]
    public async Task<ReportDto> RecordPromotionAsync(Guid id, PromotionDecision decision, Guid? promotedToGradeId)
    {
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        report.RecordPromotion(decision, promotedToGradeId);
        await _reportRepository.UpdateAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Generate)]
    public async Task DeleteAsync(Guid id)
    {
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        // Cannot delete published reports
        if (report.Status == ReportStatus.Published)
            throw new UserFriendlyException(AssessmentExceptionCodes.InvalidReportStatusTransition,
                "Cannot delete a published report.");

        // Delete all associated report subjects first
        var reportSubjects = await _reportSubjectRepository
            .GetAll()
            .Where(rs => rs.ReportId == id)
            .ToListAsync();

        foreach (var rs in reportSubjects)
        {
            await _reportSubjectRepository.DeleteAsync(rs);
        }

        await _reportRepository.DeleteAsync(report);
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Generate)]
    public async Task GenerateReportPdfAsync(Guid id)
    {
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        if (report.Status < ReportStatus.Generated)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotGeneratedForPdf,
                "Report must be generated before a PDF can be created.");

        await _backgroundJobManager.EnqueueAsync<GenerateReportPdfJob, GenerateReportPdfJobArgs>(
            new GenerateReportPdfJobArgs
            {
                ReportId = id,
                TenantId = AbpSession.TenantId,
                UserId = AbpSession.UserId.Value,
            });
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Generate)]
    public async Task<int> BulkGenerateReportPdfsAsync(BulkGenerateReportPdfsInput input)
    {
        var reportIds = await _reportRepository
            .GetAll()
            .Where(r => r.TenantId == AbpSession.TenantId)
            .Where(r => r.ClassId == input.ClassId)
            .WhereIf(input.TermId.HasValue, r => r.TermId == input.TermId.Value)
            .Where(r => r.Status >= ReportStatus.Generated)
            .Select(r => r.Id)
            .ToListAsync();

        if (reportIds.Count == 0)
            return 0;

        await _backgroundJobManager.EnqueueAsync<BulkGenerateReportPdfsJob, BulkGenerateReportPdfsJobArgs>(
            new BulkGenerateReportPdfsJobArgs
            {
                ReportIds = reportIds,
                TenantId = AbpSession.TenantId,
                UserId = AbpSession.UserId.Value,
            });

        return reportIds.Count;
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_View)]
    public async Task<string> GetReportPdfUrlAsync(Guid id)
    {
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        // LC-10: a student may only fetch their own report-card PDF.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != report.StudentId)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        return report.PdfUrl;
    }

    private static CapsAchievementLevel CalculateAchievementLevel(decimal percentage)
    {
        if (percentage >= 80) return CapsAchievementLevel.Level7;
        if (percentage >= 70) return CapsAchievementLevel.Level6;
        if (percentage >= 60) return CapsAchievementLevel.Level5;
        if (percentage >= 50) return CapsAchievementLevel.Level4;
        if (percentage >= 40) return CapsAchievementLevel.Level3;
        if (percentage >= 30) return CapsAchievementLevel.Level2;
        return CapsAchievementLevel.Level1;
    }
}
