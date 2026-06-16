using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Assessment.ReportSubjects.Dto;
using psms.Assessment.Shared;
using psms.Authorization;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Assessment.ReportSubjects;

/// <summary>
/// Service for managing report subject entries.
/// TENANT SAFETY: ReportSubject has no IMayHaveTenant — filter via Report.TenantId.
/// </summary>
[AbpAuthorize(PermissionNames.Assessment_ReportCards)]
public class ReportSubjectAppService : ApplicationService, IReportSubjectAppService
{
    private readonly IRepository<ReportSubject, Guid> _reportSubjectRepository;
    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly psms.Academic.Students.ICurrentStudentResolver _currentStudent;

    public ReportSubjectAppService(
        IRepository<ReportSubject, Guid> reportSubjectRepository,
        IRepository<Report, Guid> reportRepository,
        psms.Academic.Students.ICurrentStudentResolver currentStudent)
    {
        _reportSubjectRepository = reportSubjectRepository;
        _reportRepository = reportRepository;
        _currentStudent = currentStudent;
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_View)]
    public async Task<ReportSubjectDto> GetAsync(Guid id)
    {
        // Tenant isolation via Report parent
        var reportSubject = await _reportSubjectRepository
            .GetAll()
            .Include(rs => rs.Report)
            .Include(rs => rs.Subject)
            .Include(rs => rs.Teacher)
            .Where(rs => rs.Report.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(rs => rs.Id == id);

        if (reportSubject == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportSubjectNotFound,
                "Report subject entry not found.");

        // LC-10: a student may only read their own report's subject entries
        // (scope via the parent Report's StudentId).
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != reportSubject.Report.StudentId)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportSubjectNotFound,
                "Report subject entry not found.");

        return ObjectMapper.Map<ReportSubjectDto>(reportSubject);
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_View)]
    public async Task<ListResultDto<ReportSubjectDto>> GetByReportAsync(Guid reportId)
    {
        // Validate report exists and belongs to tenant
        var report = await _reportRepository
            .GetAll()
            .FirstOrDefaultAsync(r => r.Id == reportId && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        // LC-10: a student may only read their own report's subject entries.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != report.StudentId)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        var items = await _reportSubjectRepository
            .GetAll()
            .Include(rs => rs.Report)
            .Include(rs => rs.Subject)
            .Include(rs => rs.Teacher)
            .Where(rs => rs.Report.TenantId == AbpSession.TenantId)
            .Where(rs => rs.ReportId == reportId)
            .OrderBy(rs => rs.Subject.SubjectName)
            .ToListAsync();

        return new ListResultDto<ReportSubjectDto>(
            ObjectMapper.Map<List<ReportSubjectDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Generate)]
    public async Task<ReportSubjectDto> RecordMarksAsync(RecordReportSubjectMarksDto input)
    {
        var reportSubject = await _reportSubjectRepository
            .GetAll()
            .Include(rs => rs.Report)
            .Where(rs => rs.Report.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(rs => rs.Id == input.ReportSubjectId);

        if (reportSubject == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportSubjectNotFound,
                "Report subject entry not found.");

        // Cannot modify subjects on Published or Approved reports
        if (reportSubject.Report.Status == ReportStatus.Published || reportSubject.Report.Status == ReportStatus.Approved)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotEditable,
                "Cannot modify report subjects on an approved or published report.");

        reportSubject.RecordMarks(input.TermMark, input.ExamMark, input.TermWeight, input.ExamWeight);

        if (input.TeacherComment != null)
            reportSubject.TeacherComment = input.TeacherComment;

        await _reportSubjectRepository.UpdateAsync(reportSubject);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Recalculate report overall percentage
        await RecalculateReportOverall(reportSubject.ReportId);

        return await GetAsync(reportSubject.Id);
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Generate)]
    public async Task<ListResultDto<ReportSubjectDto>> BulkRecordMarksAsync(BulkRecordReportSubjectMarksDto input)
    {
        // Validate report exists and belongs to tenant
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == input.ReportId && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        // Cannot modify subjects on Published or Approved reports
        if (report.Status == ReportStatus.Published || report.Status == ReportStatus.Approved)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotEditable,
                "Cannot modify report subjects on an approved or published report.");

        var updatedIds = new List<Guid>();

        foreach (var subjectMark in input.SubjectMarks)
        {
            var reportSubject = await _reportSubjectRepository
                .GetAll()
                .Include(rs => rs.Report)
                .Where(rs => rs.Report.TenantId == AbpSession.TenantId)
                .FirstOrDefaultAsync(rs => rs.Id == subjectMark.ReportSubjectId);

            if (reportSubject == null)
                throw new UserFriendlyException(AssessmentExceptionCodes.ReportSubjectNotFound,
                    $"Report subject entry {subjectMark.ReportSubjectId} not found.");

            // Prevent cross-report manipulation
            if (reportSubject.ReportId != input.ReportId)
                throw new UserFriendlyException(AssessmentExceptionCodes.ReportSubjectNotFound,
                    $"Report subject entry {subjectMark.ReportSubjectId} does not belong to report {input.ReportId}.");

            reportSubject.RecordMarks(subjectMark.TermMark, subjectMark.ExamMark, subjectMark.TermWeight, subjectMark.ExamWeight);

            if (subjectMark.TeacherComment != null)
                reportSubject.TeacherComment = subjectMark.TeacherComment;

            await _reportSubjectRepository.UpdateAsync(reportSubject);
            updatedIds.Add(reportSubject.Id);
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        // Recalculate report overall
        await RecalculateReportOverall(input.ReportId);

        // Return updated entries
        var updated = await _reportSubjectRepository
            .GetAll()
            .Include(rs => rs.Report)
            .Include(rs => rs.Subject)
            .Include(rs => rs.Teacher)
            .Where(rs => rs.Report.TenantId == AbpSession.TenantId)
            .Where(rs => updatedIds.Contains(rs.Id))
            .OrderBy(rs => rs.Subject.SubjectName)
            .ToListAsync();

        return new ListResultDto<ReportSubjectDto>(
            ObjectMapper.Map<List<ReportSubjectDto>>(updated));
    }

    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Generate)]
    public async Task<ReportSubjectDto> AddTeacherCommentAsync(Guid id, string comment)
    {
        var reportSubject = await _reportSubjectRepository
            .GetAll()
            .Include(rs => rs.Report)
            .Where(rs => rs.Report.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(rs => rs.Id == id);

        if (reportSubject == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportSubjectNotFound,
                "Report subject entry not found.");

        reportSubject.TeacherComment = comment;
        await _reportSubjectRepository.UpdateAsync(reportSubject);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    private async Task RecalculateReportOverall(Guid reportId)
    {
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == reportId && r.TenantId == AbpSession.TenantId);

        if (report == null) return;

        var subjectsWithMarks = await _reportSubjectRepository
            .GetAll()
            .Where(rs => rs.ReportId == reportId && rs.FinalMark.HasValue)
            .ToListAsync();

        if (subjectsWithMarks.Any())
        {
            report.OverallPercentage = subjectsWithMarks.Average(rs => rs.FinalMark.Value);
            report.OverallAchievementLevel = CalculateAchievementLevel(report.OverallPercentage.Value);
        }
        else
        {
            report.OverallPercentage = null;
            report.OverallAchievementLevel = null;
        }

        await _reportRepository.UpdateAsync(report);
        await CurrentUnitOfWork.SaveChangesAsync();
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
