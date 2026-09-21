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
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly psms.Academic.Students.ICurrentStudentResolver _currentStudent;
    private readonly psms.Academic.Parents.ICurrentParentResolver _currentParent;

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
        IBackgroundJobManager backgroundJobManager,
        psms.Academic.Students.ICurrentStudentResolver currentStudent,
        psms.Academic.Parents.ICurrentParentResolver currentParent)
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
        _backgroundJobManager = backgroundJobManager;
        _currentStudent = currentStudent;
        _currentParent = currentParent;
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

        // Create report
        var report = new Report(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.StudentId,
            input.ClassId,
            input.AcademicYearId,
            input.ReportType,
            input.TermId)
        {
            DaysPresent = input.DaysPresent,
            DaysAbsent = input.DaysAbsent,
            DaysLate = input.DaysLate,
            TeacherComment = input.TeacherComment
        };

        await _reportRepository.InsertAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Auto-create ReportSubject entries for all class subjects
        var classSubjects = await _classSubjectRepository
            .GetAll()
            .Include(cs => cs.Subject)
            .Where(cs => cs.TenantId == AbpSession.TenantId)
            .Where(cs => cs.ClassId == input.ClassId && cs.IsActive)
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
            if (input.TermId.HasValue)
            {
                var subjectMarks = await _markRepository
                    .GetAll()
                    .Include(m => m.Assessment)
                    .Where(m => m.TenantId == AbpSession.TenantId)
                    .Where(m => m.StudentId == input.StudentId)
                    .Where(m => m.Assessment.TermId == input.TermId.Value)
                    .Where(m => m.Assessment.ClassSubject.SubjectId == classSubject.SubjectId)
                    .Where(m => m.Assessment.ClassSubject.ClassId == input.ClassId)
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

        report.TotalStudentsInClass = await _studentRepository
            .CountAsync(s => s.CurrentClassId == input.ClassId);

        report.Generate();
        await _reportRepository.UpdateAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(report.Id);
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

        // MOB-BE-04: a parent may only acknowledge their own children's reports.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(report.StudentId))
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

        // MOB-BE-04: a parent may only fetch their own children's report-card PDF.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(report.StudentId))
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
