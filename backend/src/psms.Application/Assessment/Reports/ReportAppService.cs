using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
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
using psms.Domain.Workflow.Enums;
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
    /// <summary>
    /// How long a report-card download link stays valid. Long enough to click
    /// and save, short enough that a forwarded link is worthless.
    /// </summary>
    private const int PdfLinkLifetimeSeconds = 300;

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
    private readonly psms.Domain.Shared.Storage.IFileStorageService _fileStorage;
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly psms.Academic.Students.ICurrentStudentResolver _currentStudent;
    private readonly psms.Academic.Parents.ICurrentParentResolver _currentParent;
    private readonly psms.Workflow.Shared.WorkflowStarterService _workflowStarter;

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
        psms.Domain.Shared.Storage.IFileStorageService fileStorage,
        IBackgroundJobManager backgroundJobManager,
        psms.Academic.Students.ICurrentStudentResolver currentStudent,
        psms.Academic.Parents.ICurrentParentResolver currentParent,
        psms.Workflow.Shared.WorkflowStarterService workflowStarter)
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
        _fileStorage = fileStorage;
        _backgroundJobManager = backgroundJobManager;
        _currentStudent = currentStudent;
        _currentParent = currentParent;
        _workflowStarter = workflowStarter;
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

        // MOB-BE-04: a parent may only read their own children's report cards.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(report.StudentId))
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        var dto = ObjectMapper.Map<ReportDto>(report);
        dto.SubjectReports = ObjectMapper.Map<List<ReportSubjectDto>>(report.SubjectReports.OrderBy(sr => sr.Subject?.SubjectName).ToList());

        // RC-09: see ReportListDto.ActiveWorkflowInstanceId.
        var live = await _workflowStarter.GetActiveInstanceIdsAsync(
            AbpSession.TenantId, WorkflowEntityType.Report, new[] { dto.Id });
        if (live.TryGetValue(dto.Id, out var wfId))
            dto.ActiveWorkflowInstanceId = wfId;

        return dto;
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_View)]
    public async Task<PagedResultDto<ReportListDto>> GetAllAsync(GetReportsInput input)
    {
        // LC-10: a student-portal user only ever sees their own report cards.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        // MOB-BE-04: a parent only ever sees their own children's report cards.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();

        var query = _reportRepository
            .GetAll()
            .Include(r => r.Student)
            .Include(r => r.Class)
            .Include(r => r.Term)
            .Include(r => r.AcademicYear)
            .Include(r => r.SubjectReports)
            .Where(r => r.TenantId == AbpSession.TenantId)
            .WhereIf(selfId.HasValue, r => r.StudentId == selfId.Value)
            .WhereIf(childIds != null, r => childIds.Contains(r.StudentId))
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

        var dtos = ObjectMapper.Map<List<ReportListDto>>(items);

        // RC-09: mark the rows whose approval the workflow has taken over, so the
        // list can hide the direct Approve action instead of offering a button the
        // server will refuse. One query for the page, not one per row.
        var liveWorkflows = await _workflowStarter.GetActiveInstanceIdsAsync(
            AbpSession.TenantId,
            WorkflowEntityType.Report,
            dtos.Select(d => d.Id).ToList());

        foreach (var dto in dtos)
        {
            if (liveWorkflows.TryGetValue(dto.Id, out var instanceId))
                dto.ActiveWorkflowInstanceId = instanceId;
        }

        return new PagedResultDto<ReportListDto>(totalCount, dtos);
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_View)]
    public async Task<ReportDto> GetByStudentTermAsync(Guid studentId, Guid termId, ReportType reportType)
    {
        // LC-10: a student may only read their own report card.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != studentId)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        // MOB-BE-04: a parent may only read their own children's report cards.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(studentId))
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

        // RC-09: see ReportListDto.ActiveWorkflowInstanceId.
        var live = await _workflowStarter.GetActiveInstanceIdsAsync(
            AbpSession.TenantId, WorkflowEntityType.Report, new[] { dto.Id });
        if (live.TryGetValue(dto.Id, out var wfId))
            dto.ActiveWorkflowInstanceId = wfId;

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

        var context = await LoadGenerationContextAsync(
            input.ClassId,
            input.TermId,
            new[] { input.StudentId });

        var report = await BuildReportAsync(
            context,
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
    /// Everything a generation run needs that is the same for every learner in
    /// the class, loaded once up front.
    /// <para>
    /// This exists because the per-learner work used to re-query it: the class
    /// subjects, the class headcount, and one mark query per subject per
    /// learner. For a class of forty with eight subjects that was several
    /// hundred round trips, all inside one open write transaction.
    /// </para>
    /// </summary>
    private sealed class ReportGenerationContext
    {
        public List<ClassSubject> ClassSubjects { get; set; }

        public int TotalStudentsInClass { get; set; }

        /// <summary>
        /// The weighted subject average per (learner, subject). A missing key
        /// means that learner has no completed marks for that subject, which is
        /// how a subject with no marks stays blank on the card.
        /// </summary>
        public Dictionary<(Guid StudentId, Guid SubjectId), decimal> SubjectAverages { get; set; }
    }

    private async Task<ReportGenerationContext> LoadGenerationContextAsync(
        Guid classId,
        Guid? termId,
        IReadOnlyCollection<Guid> studentIds)
    {
        var classSubjects = await _classSubjectRepository
            .GetAll()
            .Include(cs => cs.Subject)
            .Where(cs => cs.TenantId == AbpSession.TenantId)
            .Where(cs => cs.ClassId == classId && cs.IsActive)
            .ToListAsync();

        var totalStudentsInClass = await _studentRepository
            .CountAsync(s => s.TenantId == AbpSession.TenantId && s.CurrentClassId == classId);

        var averages = new Dictionary<(Guid, Guid), decimal>();

        if (termId.HasValue && studentIds.Count > 0 && classSubjects.Count > 0)
        {
            var ids = studentIds.ToList();
            var subjectIds = classSubjects.Select(cs => cs.SubjectId).Distinct().ToList();

            var marks = await _markRepository
                .GetAll()
                .Where(m => m.TenantId == AbpSession.TenantId)
                .Where(m => ids.Contains(m.StudentId))
                .Where(m => m.Assessment.TermId == termId.Value)
                .Where(m => m.Assessment.ClassSubject.ClassId == classId)
                .Where(m => subjectIds.Contains(m.Assessment.ClassSubject.SubjectId))
                .Where(m => m.Status == MarkStatus.Completed && m.Percentage.HasValue)
                .Select(m => new
                {
                    m.StudentId,
                    m.Assessment.ClassSubject.SubjectId,
                    Percentage = m.Percentage.Value,
                    m.Assessment.Weight
                })
                .ToListAsync();

            foreach (var group in marks.GroupBy(m => (m.StudentId, m.SubjectId)))
            {
                var totalWeight = group.Sum(m => m.Weight);

                averages[group.Key] = totalWeight > 0
                    // Weighted average: sum(percentage * weight) / sum(weight)
                    ? group.Sum(m => m.Percentage * m.Weight) / totalWeight
                    // Fallback to a simple average when no weights are configured
                    : group.Average(m => m.Percentage);
            }
        }

        return new ReportGenerationContext
        {
            ClassSubjects = classSubjects,
            TotalStudentsInClass = totalStudentsInClass,
            SubjectAverages = averages
        };
    }

    /// <summary>
    /// Creates one report card and its subject rows. Shared by the
    /// single-student <see cref="GenerateAsync"/> and the bulk path (RC-01), so
    /// both produce identical reports — the callers differ only in how they
    /// validate and how they handle a failure.
    /// <para>
    /// The caller is responsible for validating the student, class, year and
    /// term, for duplicate prevention, and for the RE-001 completeness gate.
    /// </para>
    /// </summary>
    private async Task<Report> BuildReportAsync(
        ReportGenerationContext context,
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

        // Flush the parent before inserting its subject rows. EF Core would
        // normally order these itself from the FK graph, but this runs without
        // test coverage and a wrong order fails the whole run, so the round trip
        // is worth it. It is one per learner, against the several hundred this
        // method used to cost.
        await CurrentUnitOfWork.SaveChangesAsync();

        // One ReportSubject per active class subject, carrying the mark average
        // already computed for this learner.
        decimal? overallTotal = null;
        var markedSubjects = 0;

        foreach (var classSubject in context.ClassSubjects)
        {
            var reportSubject = new ReportSubject(
                Guid.NewGuid(),
                report.Id,
                classSubject.SubjectId)
            {
                TeacherId = classSubject.TeacherId
            };

            if (context.SubjectAverages.TryGetValue((studentId, classSubject.SubjectId), out var average))
            {
                reportSubject.RecordMarks(average, null);

                if (reportSubject.FinalMark.HasValue)
                {
                    overallTotal = (overallTotal ?? 0m) + reportSubject.FinalMark.Value;
                    markedSubjects++;
                }
            }

            await _reportSubjectRepository.InsertAsync(reportSubject);
        }

        // The overall is the mean of the subject final marks. It is computed from
        // what was just written rather than re-read, so there is one round trip
        // instead of a save-then-reload for every learner.
        if (markedSubjects > 0)
        {
            report.OverallPercentage = overallTotal.Value / markedSubjects;
            report.OverallAchievementLevel = CalculateAchievementLevel(report.OverallPercentage.Value);
        }

        report.TotalStudentsInClass = context.TotalStudentsInClass;

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
    /// skipped, incomplete marks block just that learner, and a failure is
    /// recorded against that learner while the run carries on.
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
            ? input.StudentIds.Distinct().ToList()
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

        // A requested learner who is not an active member of this class would
        // otherwise vanish from the result with no explanation — the caller asked
        // for N and silently got fewer.
        if (subset != null)
        {
            var found = students.Select(s => s.Id).ToHashSet();

            foreach (var missing in subset.Where(id => !found.Contains(id)))
            {
                result.Items.Add(new BulkGenerateReportItemDto
                {
                    StudentId = missing,
                    StudentName = "Unknown learner",
                    Outcome = BulkGenerateOutcome.Failed,
                    Message = "Not an active learner in the selected class."
                });
                result.FailedCount++;
                result.TotalStudents++;
            }
        }

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

        var readingRegister = input.UseAttendanceRecords && term != null;
        var attendanceByStudent = await ResolveAttendanceAsync(input, term, studentIds);

        // Loaded once for the whole run: the class subjects, the headcount, and
        // every learner's subject averages in a single mark query.
        var context = previewOnly
            ? null
            : await LoadGenerationContextAsync(input.ClassId, input.TermId, studentIds);

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

            (int Present, int Absent, int Late) days;
            string note = null;

            if (attendanceByStudent.TryGetValue(student.Id, out var counted))
            {
                days = counted;
            }
            else if (readingRegister)
            {
                // The register was consulted and holds nothing for this learner.
                // Zero is the honest figure — stamping the class-wide defaults on
                // their card would invent attendance they never had.
                days = (0, 0, 0);
                note = "No attendance records for this term.";
            }
            else
            {
                days = (input.DefaultDaysPresent, input.DefaultDaysAbsent, input.DefaultDaysLate);
            }

            try
            {
                // Each learner commits on its own. Sharing the ambient transaction
                // meant that one unique-constraint collision — two principals
                // pressing Generate at once, or a double click — rolled back every
                // card already generated in the run and left the connection in a
                // failed transaction, so nothing after it could succeed either.
                using (var uow = UnitOfWorkManager.Begin(new UnitOfWorkOptions
                {
                    Scope = System.Transactions.TransactionScopeOption.RequiresNew
                }))
                {
                    var report = await BuildReportAsync(
                        context,
                        student.Id,
                        input.ClassId,
                        input.AcademicYearId,
                        input.ReportType,
                        input.TermId,
                        days.Present,
                        days.Absent,
                        days.Late,
                        teacherComment: null);

                    await uow.CompleteAsync();

                    item.Outcome = BulkGenerateOutcome.Generated;
                    item.ReportId = report.Id;
                    item.Message = note;
                    result.GeneratedCount++;
                }
            }
            catch (Exception ex)
            {
                // Anything from a rule violation to a collision with a concurrent
                // run. Record it against this learner and keep going — carrying on
                // is the whole point of the batch.
                item.Outcome = BulkGenerateOutcome.Failed;
                item.Message = ex is UserFriendlyException friendly
                    ? friendly.Message
                    : "Generation failed for this learner.";
                result.FailedCount++;

                Logger.Error(
                    $"Bulk report generation failed for student {student.Id} in class {input.ClassId}.",
                    ex);
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
    /// This counts DAYS, not register rows. A school that takes a register per
    /// subject writes several rows for one learner on one day, and counting rows
    /// would print "days present: 320" for a forty-day term.
    /// </para>
    /// <para>
    /// A late arrival still attended, so a day with a Late row counts as present
    /// and is also reported on its own line — which is how the printed card reads
    /// it. A day with neither a Present nor a Late row counts as absent, whatever
    /// the reason: the card has no separate line for excused or sick leave.
    /// Holiday rows are dropped before counting, since a holiday is not a school
    /// day. So present + absent equals the school days registered, which is what
    /// the PDF prints as the total.
    /// </para>
    /// Returns an empty map when there is no term to read a window from, or when
    /// the caller asked for the supplied defaults instead.
    /// </summary>
    private async Task<Dictionary<Guid, (int Present, int Absent, int Late)>> ResolveAttendanceAsync(
        BulkGenerateReportsInput input,
        Term term,
        List<Guid> studentIds)
    {
        var map = new Dictionary<Guid, (int Present, int Absent, int Late)>();

        if (!input.UseAttendanceRecords || term == null)
            return map;

        // Registers are captured with a time component; the term boundaries are
        // midnight. Comparing them raw drops the last day of term. Every other
        // attendance query in the codebase normalises the same way.
        var start = term.StartDate.Date;
        var end = term.EndDate.Date;

        var records = await _attendanceRepository
            .GetAll()
            .Where(a => a.TenantId == AbpSession.TenantId)
            .Where(a => a.ClassId == input.ClassId)
            .Where(a => studentIds.Contains(a.StudentId))
            .Where(a => a.AttendanceDate.Date >= start && a.AttendanceDate.Date <= end)
            .Where(a => a.Status != AttendanceStatus.Holiday)
            .Select(a => new { a.StudentId, a.AttendanceDate, a.Status })
            .ToListAsync();

        foreach (var perStudent in records.GroupBy(r => r.StudentId))
        {
            var present = 0;
            var absent = 0;
            var late = 0;

            foreach (var perDay in perStudent.GroupBy(r => r.AttendanceDate.Date))
            {
                var wasLate = perDay.Any(r => r.Status == AttendanceStatus.Late);
                var attended = wasLate || perDay.Any(r => r.Status == AttendanceStatus.Present);

                if (attended)
                {
                    present++;
                }
                else
                {
                    absent++;
                }

                if (wasLate)
                {
                    late++;
                }
            }

            map[perStudent.Key] = (present, absent, late);
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

        // RC-09: submitting is what puts the report in front of a reviewer, so it
        // is what starts the approval workflow. Approval happens ONLY through the
        // engine — there is no direct-approve endpoint — so a submit that cannot
        // start a workflow would strand the report in PendingApproval with nobody
        // able to act on it. Fail loudly instead; the throw rolls the status
        // change back with the unit of work.
        var started = await _workflowStarter.TryStartWorkflowAsync(
            AbpSession.TenantId,
            WorkflowEntityType.Report,
            id,
            AbpSession.UserId.Value,
            AbpSession.UserId.Value.ToString());

        if (!started && !await _workflowStarter.HasActiveInstanceAsync(
                AbpSession.TenantId, WorkflowEntityType.Report, id))
        {
            throw new UserFriendlyException(
                AssessmentExceptionCodes.NoApprovalWorkflowConfigured,
                "This school has no active report approval workflow, so there is nobody to review this report. "
                + "Seed or activate the Report Approval workflow first.");
        }

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

        // RC-03: publishing is the moment a parent can see the report, and a
        // report card with nothing to download is not much of a report card.
        // Nothing else produced the PDF — not generate, not approve — so a
        // report could reach Published with PdfUrl null. Enqueue it here if it
        // has not been produced, and let a failure be a background-job failure
        // rather than a blocked publish.
        if (!report.HasPdf())
        {
            try
            {
                await _backgroundJobManager.EnqueueAsync<GenerateReportPdfJob, GenerateReportPdfJobArgs>(
                    new GenerateReportPdfJobArgs
                    {
                        ReportId = report.Id,
                        TenantId = AbpSession.TenantId,
                        UserId = AbpSession.UserId ?? 0
                    });
            }
            catch (Exception ex)
            {
                Logger.Warn($"Could not enqueue the report PDF for {report.Id} on publish: {ex.Message}");
            }
        }

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

        // MOB-BE-04: a parent may only acknowledge their own children's reports.
        // RC-11: and only a parent may acknowledge at all. The field records
        // that the PARENT saw the report; a teacher or principal ticking it on
        // their behalf makes the record say something that did not happen.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.NotTheParent,
                "Only a parent linked to this learner can acknowledge their report.");
        if (!childIds.Contains(report.StudentId))
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

    /// <summary>
    /// A short-lived signed link to the report's PDF.
    /// <para>
    /// RC-11: gated on Download, not View. The permission was declared, seeded
    /// to six roles and checked nowhere, which made it read like a control that
    /// did not exist.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Download)]
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

        // MOB-BE-04: a parent may only fetch their own children's report-card PDF.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(report.StudentId))
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        if (!report.HasPdf())
            throw new UserFriendlyException(AssessmentExceptionCodes.PdfNotGenerated,
                "No PDF has been generated for this report yet.");

        // RC-04: mint a short-lived signed URL rather than handing back a
        // durable link. The file itself is private, so the URL is the only way
        // in and it expires.
        var objectKey = report.ResolvePdfObjectKey(_fileStorage.DefaultBucketName);
        if (string.IsNullOrWhiteSpace(objectKey))
            throw new UserFriendlyException(AssessmentExceptionCodes.PdfNotGenerated,
                "No PDF has been generated for this report yet.");

        return await _fileStorage.CreateSignedDownloadUrlAsync(
            _fileStorage.DefaultBucketName, objectKey, PdfLinkLifetimeSeconds);
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
