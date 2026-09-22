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
using psms.Domain.Assessment;
using psms.Domain.Assessment.Entities;
using psms.Domain.Assessment.Promotion;
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
    private readonly IRepository<AssessmentWeighting, Guid> _weightingRepository;
    private readonly IRepository<Grade, Guid> _gradeRepository;
    private readonly psms.Domain.Shared.Storage.IFileStorageService _fileStorage;
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly psms.Academic.Students.ICurrentStudentResolver _currentStudent;
    private readonly psms.Academic.Parents.ICurrentParentResolver _currentParent;
    private readonly psms.Workflow.Shared.WorkflowStarterService _workflowStarter;
    private readonly ReportCohortStatisticsService _cohortStatistics;

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
        IRepository<AssessmentWeighting, Guid> weightingRepository,
        IRepository<Grade, Guid> gradeRepository,
        psms.Domain.Shared.Storage.IFileStorageService fileStorage,
        IBackgroundJobManager backgroundJobManager,
        psms.Academic.Students.ICurrentStudentResolver currentStudent,
        psms.Academic.Parents.ICurrentParentResolver currentParent,
        psms.Workflow.Shared.WorkflowStarterService workflowStarter,
        ReportCohortStatisticsService cohortStatistics)
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
        _weightingRepository = weightingRepository;
        _gradeRepository = gradeRepository;
        _fileStorage = fileStorage;
        _backgroundJobManager = backgroundJobManager;
        _currentStudent = currentStudent;
        _currentParent = currentParent;
        _workflowStarter = workflowStarter;
        _cohortStatistics = cohortStatistics;
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

        // RC-05: which terms this report covers. A year-end card is the
        // composite of the whole year, not a term with no filter.
        var termIds = await ResolveTermScopeAsync(
            input.ReportType, input.TermId, input.AcademicYearId);

        if (termIds.Count == 0)
            throw new UserFriendlyException(AssessmentExceptionCodes.NoTermsInScope,
                "There are no terms in the academic year to build this report from.");

        // RE-001: every mark in scope must be complete. This used to be skipped
        // entirely for a year-end report, because it was written as "if a term
        // was given" and a year-end report has no single term.
        var incompleteMarks = await _markRepository
            .GetAll()
            .Where(m => m.TenantId == AbpSession.TenantId)
            .Where(m => m.StudentId == input.StudentId)
            .Where(m => termIds.Contains(m.Assessment.TermId))
            .Where(m => m.Assessment.ClassSubject.ClassId == input.ClassId)
            .Where(m => m.Status != MarkStatus.Completed && m.Status != MarkStatus.Absent && m.Status != MarkStatus.Exempted)
            .AnyAsync();

        if (incompleteMarks)
            throw new UserFriendlyException(AssessmentExceptionCodes.IncompleteMarksForReport,
                termIds.Count > 1
                    ? "Cannot generate report. There are incomplete marks for this student in the academic year."
                    : "Cannot generate report. There are incomplete marks for this student in the specified term.");

        var context = await LoadGenerationContextAsync(
            input.ClassId,
            termIds,
            new[] { input.StudentId },
            input.ReportType);

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

        // RC-06: this learner joining the cohort reorders everybody in it. This
        // runs in the same unit of work as the generation above, so a failure
        // rolls the new report back with it rather than leaving a card that is
        // ranked against nothing.
        await _cohortStatistics.RecalculateAsync(
            AbpSession.TenantId, input.ClassId, input.TermId, input.ReportType);

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

        /// <summary>The grade this class sits in, which decides the split.</summary>
        public SouthAfricanGradeLevel GradeLevel { get; set; }

        /// <summary>The school's School-Based Assessment percentage for that grade's band.</summary>
        public int SbaPercentage { get; set; }

        /// <summary>The school's examination percentage for that grade's band.</summary>
        public int ExamPercentage { get; set; }

        /// <summary>
        /// RC-15. Whether the examination for this grade is the external
        /// National Senior Certificate paper, in which case the school holds
        /// only the School-Based Assessment component.
        /// </summary>
        public bool ExaminationIsExternal { get; set; }

        /// <summary>
        /// Whether the final mark on this card is the promotion mark, and so a
        /// whole number under NPPPPR §31(3). True for a year-end report.
        /// </summary>
        public bool IsPromotionMark { get; set; }

        /// <summary>
        /// Whether the band's School-Based Assessment to examination split
        /// applies. It composes the <b>year</b> mark against the end-of-year
        /// examination, so it applies to a year-end card and to nothing else: a
        /// term card reports the term's own tasks (National Protocol §17(1)).
        /// </summary>
        public bool ComposesAgainstTheExamination { get; set; }

        /// <summary>
        /// Every completed mark in scope, per (learner, subject), carrying its
        /// weight and whether it was the examination. A missing key means that
        /// learner has no completed marks for that subject, which is how a
        /// subject with no marks stays blank on the card.
        /// </summary>
        public Dictionary<(Guid StudentId, Guid SubjectId), List<AssessmentContribution>> Contributions { get; set; }

        /// <summary>
        /// Whether this subject is assessed entirely by the school in this
        /// grade, whatever the band says - Life Orientation in Grades 10-12
        /// (NPPPPR §31(2)).
        /// </summary>
        public bool IsFullySchoolBased(ClassSubject classSubject) =>
            SubjectAssessmentRules.IsFullySchoolBased(
                classSubject.Subject?.SubjectName,
                classSubject.Subject?.SubjectCode,
                GradeLevel);
    }

    /// <summary>
    /// RC-05. The terms a report covers.
    /// <para>
    /// A term card reports its own term. A <b>year-end</b> card is the composite
    /// of every term in the academic year - National Protocol §17(1), "the
    /// promotion of a learner is based on the composite marks obtained in all
    /// four terms", and §25(5), "the end-of-year report card should indicate
    /// cumulative learner performance for the year". A <b>mid-year</b> card
    /// covers the terms that have finished by the middle of it.
    /// </para>
    /// <para>
    /// This is what was missing: both the completeness gate and the mark query
    /// were written as "if a term was given", and a year-end report has no
    /// single term, so it skipped the gate and aggregated nothing.
    /// </para>
    /// </summary>
    /// <summary>
    /// RE-002 / RC-05 / RC-12. Refuses to move a blank report card forward.
    /// <para>
    /// Two ways one is reached: a class with no class-subjects configured
    /// generates a card with no subject rows at all, and a report built over a
    /// scope with no marks generates rows that are all empty. Both used to
    /// reach Generated, and from there could be submitted, approved and
    /// published as a completely blank card.
    /// </para>
    /// <para>
    /// The gate is here rather than at generation on purpose: generating the
    /// shell before the marks exist is a legitimate thing to do, and RC-12 adds
    /// the screen for filling it in. What must not happen is a blank card
    /// reaching a parent.
    /// </para>
    /// </summary>
    private async Task AssertNotBlankAsync(Report report)
    {
        var subjects = await _reportSubjectRepository
            .GetAll()
            .Where(rs => rs.ReportId == report.Id)
            .Select(rs => new { rs.FinalMark })
            .ToListAsync();

        if (subjects.Count == 0)
            throw new UserFriendlyException(AssessmentExceptionCodes.BlankReportCard,
                "This report card has no subjects on it. Check that the class has its subjects configured, "
                + "then generate it again.");

        if (subjects.All(rs => rs.FinalMark == null))
            throw new UserFriendlyException(AssessmentExceptionCodes.BlankReportCard,
                "This report card has no marks in any subject. Capture the marks before sending it on.");
    }

    private async Task<List<Guid>> ResolveTermScopeAsync(
        ReportType reportType,
        Guid? termId,
        Guid academicYearId)
    {
        var terms = await _termRepository
            .GetAll()
            .Where(t => t.TenantId == AbpSession.TenantId)
            .Where(t => t.AcademicYearId == academicYearId)
            .OrderBy(t => t.TermNumber)
            .Select(t => new { t.Id, t.TermNumber })
            .ToListAsync();

        if (IsYearScoped(reportType))
        {
            // A term supplied for a year-scoped report is ignored rather than
            // honoured: a year-end card built from one term would still be
            // stamped with a whole-number promotion mark, which would be a
            // promotion decision made on a quarter of the year.
            return reportType == ReportType.MidYear
                ? terms
                    .Where(t => t.TermNumber == SouthAfricanTermNumber.Term1
                        || t.TermNumber == SouthAfricanTermNumber.Term2)
                    .Select(t => t.Id)
                    .ToList()
                : terms.Select(t => t.Id).ToList();
        }

        // Everything else reports one term, and has to name it. The screen
        // enforces this; the API did not, so a Term 3 card posted without a term
        // was quietly built from the whole year.
        if (!termId.HasValue)
            throw new UserFriendlyException(AssessmentExceptionCodes.TermRequiredForReport,
                "A term report needs the term it covers.");

        if (terms.All(t => t.Id != termId.Value))
            throw new UserFriendlyException(AssessmentExceptionCodes.TermNotInAcademicYear,
                "That term does not belong to the selected academic year.");

        return new List<Guid> { termId.Value };
    }

    /// <summary>
    /// Whether this kind of report covers a span of terms rather than one.
    /// </summary>
    private static bool IsYearScoped(ReportType reportType) =>
        reportType == ReportType.YearEnd || reportType == ReportType.MidYear;

    /// <summary>
    /// The school's split for a grade band, falling back to the national
    /// default when the tenant has no row for it - the same behaviour the
    /// settings screen shows, so a school that has never opened it still gets
    /// marks composed the way policy says.
    /// </summary>
    private async Task<(int Sba, int Exam)> ResolveWeightingAsync(AssessmentWeightingBand band)
    {
        var row = await _weightingRepository
            .GetAll()
            .Where(w => w.TenantId == AbpSession.TenantId && w.Band == band)
            .FirstOrDefaultAsync();

        if (row != null && AssessmentWeighting.IsValidSplit(row.SbaPercentage, row.ExamPercentage))
            return (row.SbaPercentage, row.ExamPercentage);

        return (AssessmentWeightingDefaults.SbaFor(band), AssessmentWeightingDefaults.ExamFor(band));
    }

    private async Task<ReportGenerationContext> LoadGenerationContextAsync(
        Guid classId,
        IReadOnlyCollection<Guid> termIds,
        IReadOnlyCollection<Guid> studentIds,
        ReportType reportType)
    {
        var classSubjects = await _classSubjectRepository
            .GetAll()
            .Include(cs => cs.Subject)
            .Where(cs => cs.TenantId == AbpSession.TenantId)
            .Where(cs => cs.ClassId == classId && cs.IsActive)
            .ToListAsync();

        // Active only, to match who a run actually generates for. The count is
        // a starting figure: RC-06's cohort pass restates it as the number of
        // report cards actually ranked, so a printed "3 of 30" is consistent.
        var totalStudentsInClass = await _studentRepository
            .CountAsync(s => s.TenantId == AbpSession.TenantId
                && s.CurrentClassId == classId
                && s.IsActive);

        // The grade decides the band, and the band decides the split. A class
        // whose grade cannot be resolved — deleted, or never set — must stop the
        // run: defaulting would silently pick the Foundation band, whose split
        // is 100:0, and drop every examination mark off the card.
        var gradeLevel = await _classRepository
            .GetAll()
            .Where(c => c.Id == classId && c.TenantId == AbpSession.TenantId)
            .Select(c => (SouthAfricanGradeLevel?)c.Grade.GradeLevel)
            .FirstOrDefaultAsync();

        if (!gradeLevel.HasValue)
            throw new UserFriendlyException(AssessmentExceptionCodes.ClassGradeNotResolved,
                "This class is not linked to a grade, so there is no way to tell how its marks "
                + "should be weighted. Set the class's grade and try again.");

        var band = AssessmentWeightingDefaults.BandFor(gradeLevel.Value);
        var (sba, exam) = await ResolveWeightingAsync(band);

        var contributions = new Dictionary<(Guid, Guid), List<AssessmentContribution>>();

        if (termIds.Count > 0 && studentIds.Count > 0 && classSubjects.Count > 0)
        {
            var ids = studentIds.ToList();
            var terms = termIds.ToList();
            var subjectIds = classSubjects.Select(cs => cs.SubjectId).Distinct().ToList();

            var marks = await _markRepository
                .GetAll()
                .Where(m => m.TenantId == AbpSession.TenantId)
                .Where(m => ids.Contains(m.StudentId))
                .Where(m => terms.Contains(m.Assessment.TermId))
                .Where(m => m.Assessment.ClassSubject.ClassId == classId)
                .Where(m => subjectIds.Contains(m.Assessment.ClassSubject.SubjectId))
                .Where(m => m.Status == MarkStatus.Completed && m.Percentage.HasValue)
                .Select(m => new
                {
                    m.StudentId,
                    m.Assessment.ClassSubject.SubjectId,
                    Percentage = m.Percentage.Value,
                    m.Assessment.Weight,
                    m.Assessment.AssessmentType
                })
                .ToListAsync();

            foreach (var group in marks.GroupBy(m => (m.StudentId, m.SubjectId)))
            {
                contributions[group.Key] = group
                    .Select(m => new AssessmentContribution(
                        m.Percentage,
                        m.Weight,
                        m.AssessmentType == AcademicAssessmentType.Exam))
                    .ToList();
            }
        }

        return new ReportGenerationContext
        {
            ClassSubjects = classSubjects,
            TotalStudentsInClass = totalStudentsInClass,
            GradeLevel = gradeLevel.Value,
            SbaPercentage = sba,
            ExamPercentage = exam,
            // RC-15: Grade 12's examination is the external NSC paper. The
            // school's own trial paper is not it, and must not be blended in at
            // 75% to make a number that reads like an NSC result.
            ExaminationIsExternal = band == AssessmentWeightingBand.Grade12,
            IsPromotionMark = reportType == ReportType.YearEnd,
            ComposesAgainstTheExamination = reportType == ReportType.YearEnd,
            Contributions = contributions
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
        var subjectFinalMarks = new List<decimal?>();

        foreach (var classSubject in context.ClassSubjects)
        {
            var reportSubject = new ReportSubject(
                Guid.NewGuid(),
                report.Id,
                classSubject.SubjectId)
            {
                TeacherId = classSubject.TeacherId
            };

            if (context.Contributions.TryGetValue((studentId, classSubject.SubjectId), out var marks))
            {
                // RC-05: a term card reports "the total mark obtained in all
                // tasks completed in a term" (National Protocol §17(1)). The
                // band's SBA:examination split is about the end-of-year
                // examination, so it composes the year mark and only that —
                // applying it in term 1 would re-weight a class test as though
                // it were the final paper.
                SubjectMarkResult aggregate;

                if (context.ComposesAgainstTheExamination)
                {
                    // RC-15: with Life Orientation and Grade 12 handled as
                    // policy requires.
                    var fullySchoolBased = context.IsFullySchoolBased(classSubject);

                    aggregate = SubjectMarkAggregator.Aggregate(
                        marks,
                        fullySchoolBased ? 100 : context.SbaPercentage,
                        fullySchoolBased ? 0 : context.ExamPercentage,
                        examinationIsExternal: context.ExaminationIsExternal && !fullySchoolBased);
                }
                else
                {
                    aggregate = SubjectMarkAggregator.AggregateTerm(marks);
                }

                reportSubject.RecordAggregate(aggregate, context.IsPromotionMark);
                subjectFinalMarks.Add(reportSubject.FinalMark);
            }

            await _reportSubjectRepository.InsertAsync(reportSubject);
        }

        // RC-07: the overall comes from Report.RecalculateOverall, the one
        // definition of it. It is computed from what was just written rather
        // than re-read, so there is one round trip instead of a save-then-reload
        // for every learner.
        report.RecalculateOverall(subjectFinalMarks);

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

        // RC-05: the terms this run covers. A year-end run has no TermId, and
        // both the gate below and the mark query used to be skipped entirely
        // because of it.
        var termIds = await ResolveTermScopeAsync(
            input.ReportType, input.TermId, input.AcademicYearId);

        if (termIds.Count == 0)
        {
            // A preview exists to show what would block a run, so it reports
            // this rather than failing — a principal looking at an academic year
            // with no terms configured should see why, not an error dialog.
            if (!previewOnly)
                throw new UserFriendlyException(AssessmentExceptionCodes.NoTermsInScope,
                    "There are no terms in the academic year to build these reports from.");

            foreach (var blocked in students)
            {
                result.Items.Add(new BulkGenerateReportItemDto
                {
                    StudentId = blocked.Id,
                    StudentName = $"{blocked.FirstName} {blocked.LastName}".Trim(),
                    AdmissionNumber = blocked.AdmissionNumber,
                    Outcome = BulkGenerateOutcome.Blocked,
                    Message = "There are no terms in the academic year to build this report from."
                });
                result.BlockedCount++;
            }

            return result;
        }

        // RE-001, evaluated per learner rather than for the batch: one learner
        // with an outstanding mark must not stop the other thirty-nine.
        var incomplete = await _markRepository
            .GetAll()
            .Where(m => m.TenantId == AbpSession.TenantId)
            .Where(m => termIds.Contains(m.Assessment.TermId))
            .Where(m => m.Assessment.ClassSubject.ClassId == input.ClassId)
            .Where(m => studentIds.Contains(m.StudentId))
            .Where(m => m.Status != MarkStatus.Completed
                && m.Status != MarkStatus.Absent
                && m.Status != MarkStatus.Exempted)
            .Select(m => m.StudentId)
            .Distinct()
            .ToListAsync();

        var blockedStudentIds = new HashSet<Guid>(incomplete);

        var readingRegister = input.UseAttendanceRecords && term != null;
        var attendanceByStudent = await ResolveAttendanceAsync(input, term, studentIds);

        // Loaded once for the whole run: the class subjects, the headcount, the
        // grade's weighting, and every learner's marks in a single query.
        var context = previewOnly
            ? null
            : await LoadGenerationContextAsync(input.ClassId, termIds, studentIds, input.ReportType);

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

        // RC-06: class position and the per-subject class figures only exist
        // once the cohort does, so they are stamped on in one pass here rather
        // than guessed at per learner inside the loop above.
        //
        // Contained, unlike the other callers: every learner above has already
        // committed in its own unit of work, and a class of report cards that
        // were genuinely created must not come back as a failed request because
        // the ranking pass fell over afterwards.
        if (result.GeneratedCount > 0)
        {
            await _cohortStatistics.TryRecalculateAsync(
                AbpSession.TenantId, input.ClassId, input.TermId, input.ReportType);
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

        await AssertNotBlankAsync(report);

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

        await AssertNotBlankAsync(report);

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
        return await RecordPromotionDecisionAsync(new RecordPromotionDto
        {
            ReportId = id,
            Decision = decision,
            PromotedToGradeId = promotedToGradeId
        });
    }

    /// <summary>
    /// RC-16. Records the promotion decision on a year-end report card.
    /// <para>
    /// The decision itself stays a person's: NPPPPR §(2b) puts a retention
    /// behind a meeting of subject staff and then a meeting with the parent, and
    /// progression — moving a learner on despite not meeting the requirements,
    /// to keep them from spending more than four years in a phase — is a
    /// judgement no rule can reach. What is checked here is everything
    /// structural: the right kind of card, a card still open to being changed,
    /// and a destination grade that exists and is the one the learner would
    /// actually move into.
    /// </para>
    /// <para>
    /// Use <see cref="GetPromotionAdviceAsync"/> first: it says whether the
    /// national requirements are met and exactly which are not.
    /// </para>
    /// </summary>
    // The same permission the older RecordPromotion endpoint has always
    // required. A this-call does not pass through the authorization interceptor,
    // so a laxer attribute here would have made the stricter one bypassable by
    // calling the other endpoint.
    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Publish)]
    public async Task<ReportDto> RecordPromotionDecisionAsync(RecordPromotionDto input)
    {
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == input.ReportId && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        // RE-003 and National Protocol §25(3): a published card is a legal
        // document that must carry no corrections. The promotion decision is
        // recorded before it is issued, not after.
        if (report.Status == ReportStatus.Published)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotEditable,
                "This report card has been published. A published report card cannot be changed.");

        if (!report.CarriesPromotionDecision())
            throw new UserFriendlyException(AssessmentExceptionCodes.PromotionNotOnThisReport,
                "A promotion decision belongs on the year-end report card, which is the one that "
                + "carries the composite marks for the year.");

        if (!Enum.IsDefined(typeof(PromotionDecision), input.Decision))
            throw new UserFriendlyException(AssessmentExceptionCodes.InvalidReportStatusTransition,
                "That is not a promotion decision this system recognises.");

        Guid? destinationGradeId = null;

        if (input.Decision != PromotionDecision.Retained)
        {
            if (!input.PromotedToGradeId.HasValue)
                throw new UserFriendlyException(AssessmentExceptionCodes.PromotionGradeRequired,
                    "Say which grade the learner moves into.");

            var options = await BuildGradeOptionsAsync(report.ClassId);
            var chosen = options.FirstOrDefault(g => g.Id == input.PromotedToGradeId.Value);

            if (chosen == null)
                throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound,
                    "That grade does not exist at this school.");

            // Every decision that moves a learner moves them into the next
            // grade — a conditional promotion and a progression are both
            // movements to the following grade, differing in what the school
            // undertakes to do once the learner is there, not in where they go.
            if (!chosen.IsNextGrade)
                throw new UserFriendlyException(AssessmentExceptionCodes.PromotionGradeNotNext,
                    $"A learner who moves goes into the next grade. {chosen.GradeName} is not it.");

            destinationGradeId = chosen.Id;
        }

        // A reason the caller did not send is a reason they did not change.
        // Blanking it on every save wiped the justification NPPPPR §(2b)(c)
        // requires to be printed, the moment anyone re-opened the screen and
        // pressed save.
        report.RecordPromotion(
            input.Decision,
            destinationGradeId,
            input.Reason ?? report.PromotionReason);

        await _reportRepository.UpdateAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(input.ReportId);
    }

    /// <summary>
    /// RC-16. What the national promotion requirements make of this learner's
    /// year — whether they are met, and clause by clause which are not.
    /// <para>
    /// Advice only. The rules never recommend progression, because moving a
    /// learner on despite not meeting the requirements is a judgement made in a
    /// meeting (NPPPPR §(2b)), not something a rule can reach.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Assessment_ReportCards_View)]
    public async Task<PromotionAdviceDto> GetPromotionAdviceAsync(Guid id)
    {
        var report = await _reportRepository
            .GetAll()
            .Include(r => r.SubjectReports).ThenInclude(sr => sr.Subject)
            .Include(r => r.PromotedToGrade)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        // The Student and Parent roles both hold ReportCards.View, and this
        // response carries subject names with exact percentages. Without these
        // two checks — the same ones GetAsync carries — any learner or parent
        // could read any classmate's marks and promotion outcome by report id.
        //
        // LC-10: a student may only read their own report card.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != report.StudentId)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        // MOB-BE-04: a parent may only read their own children's report cards.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(report.StudentId))
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        // Projected as a nullable level, not an anonymous type: Grade is
        // soft-deletable, so EF emits a LEFT JOIN and a missing grade would
        // materialise as the CLR default — SouthAfricanGradeLevel.GradeR — and a
        // Grade 11 learner would be evaluated against Grade R's two clauses and
        // advised "promoted". The same nullable cast is used everywhere else the
        // grade is read, for exactly this reason.
        var grade = await _classRepository
            .GetAll()
            .Where(c => c.Id == report.ClassId && c.TenantId == AbpSession.TenantId)
            .Select(c => new
            {
                GradeLevel = (SouthAfricanGradeLevel?)c.Grade.GradeLevel,
                GradeName = c.Grade.GradeName
            })
            .FirstOrDefaultAsync();

        var advice = new PromotionAdviceDto
        {
            ReportId = report.Id,
            GradeLevel = grade?.GradeLevel ?? SouthAfricanGradeLevel.Grade1,
            PromotionReason = report.PromotionReason,
            GradeName = grade?.GradeName,
            Recorded = report.PromotionDecision,
            PromotedToGradeId = report.PromotedToGradeId,
            PromotedToGradeName = report.PromotedToGrade?.GradeName,
            GradeOptions = await BuildGradeOptionsAsync(report.ClassId)
        };

        if (grade?.GradeLevel == null)
        {
            advice.IsEvaluable = false;
            advice.NotEvaluableReason =
                "This class is not linked to a grade, so there are no requirements to check against.";
            return advice;
        }

        if (!report.CarriesPromotionDecision())
        {
            advice.IsEvaluable = false;
            advice.NotEvaluableReason =
                "Promotion is decided on the year-end report card, which carries the composite marks "
                + "for the whole year. This is a term report.";
            return advice;
        }

        var subjects = report.SubjectReports
            .Select(sr => new PromotionSubject(
                sr.Subject?.SubjectName ?? "Unknown subject",
                SubjectRoleResolver.Resolve(sr.Subject?.SubjectName, sr.Subject?.SubjectCode),
                sr.FinalMark,
                // We do not track SBA completeness per subject, so a subject
                // that carries a final mark is taken to have had its
                // School-Based Assessment done. That is a proxy, and it is
                // stricter than §21(1)'s proviso, which asks only that the
                // uncounted ninth subject's SBA be complete: a learner with one
                // unmarked subject will be reported as not satisfying it. The
                // advice says which clause is short, so the reader can judge.
                schoolBasedAssessmentComplete: sr.FinalMark.HasValue))
            .ToList();

        var evaluation = PromotionRules.Evaluate(grade.GradeLevel.Value, subjects);

        advice.IsEvaluable = true;
        advice.MeetsRequirements = evaluation.MeetsRequirements;
        advice.Recommended = evaluation.Recommended;
        advice.Requirements = evaluation.Requirements
            .Select(r => new PromotionRequirementDto
            {
                Clause = r.Clause,
                Description = r.Description,
                IsMet = r.IsMet,
                Detail = r.Detail
            })
            .ToList();

        return advice;
    }

    /// <summary>
    /// The grades a learner in this class could be moved into: the one they are
    /// in, and the one immediately after it.
    /// </summary>
    private async Task<List<PromotionGradeOptionDto>> BuildGradeOptionsAsync(Guid classId)
    {
        var current = await _classRepository
            .GetAll()
            .Where(c => c.Id == classId && c.TenantId == AbpSession.TenantId)
            .Select(c => (SouthAfricanGradeLevel?)c.Grade.GradeLevel)
            .FirstOrDefaultAsync();

        var grades = await _gradeRepository
            .GetAll()
            .Where(g => g.TenantId == AbpSession.TenantId && g.IsActive)
            .OrderBy(g => g.GradeLevel)
            .Select(g => new { g.Id, g.GradeName, g.GradeLevel })
            .ToListAsync();

        return grades
            .Select(g => new PromotionGradeOptionDto
            {
                Id = g.Id,
                GradeName = g.GradeName,
                GradeLevel = g.GradeLevel,
                IsCurrentGrade = current.HasValue && g.GradeLevel == current.Value,
                IsNextGrade = current.HasValue && (int)g.GradeLevel == (int)current.Value + 1
            })
            .ToList();
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

        // RC-06: the cohort is one learner smaller. Without this, every
        // remaining card keeps a position and a class average computed over the
        // deleted learner until somebody happens to edit an unrelated mark. The
        // flush is what makes the soft delete visible to the pass.
        await CurrentUnitOfWork.SaveChangesAsync();
        await _cohortStatistics.RecalculateAsync(
            report.TenantId, report.ClassId, report.TermId, report.ReportType);
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
}
