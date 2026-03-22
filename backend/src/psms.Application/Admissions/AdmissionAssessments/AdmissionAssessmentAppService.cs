using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.AdmissionAssessments.Dto;
using psms.Admissions.Shared;
using psms.Authorization;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Admissions.AdmissionAssessments;

/// <summary>
/// Service for managing admission assessments.
/// Implements ADM-014 to ADM-016.
/// </summary>
[AbpAuthorize(PermissionNames.Admissions_Assessments)]
public class AdmissionAssessmentAppService : ApplicationService, IAdmissionAssessmentAppService
{
    private readonly IRepository<AdmissionAssessment, Guid> _assessmentRepository;
    private readonly IRepository<Application, Guid> _applicationRepository;

    private const int MinNoticeDaysRequired = 7; // ADM-015: 7 days minimum notice

    public AdmissionAssessmentAppService(
        IRepository<AdmissionAssessment, Guid> assessmentRepository,
        IRepository<Application, Guid> applicationRepository)
    {
        _assessmentRepository = assessmentRepository;
        _applicationRepository = applicationRepository;
    }

    [AbpAuthorize(PermissionNames.Admissions_Assessments_View)]
    public async Task<AdmissionAssessmentDto> GetAsync(Guid id)
    {
        var assessment = await _assessmentRepository
            .GetAll()
            .Include(a => a.Application)
                .ThenInclude(app => app.AppliedGrade)
            .Include(a => a.AssessedGrade)
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (assessment == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.AssessmentNotFound, "Assessment not found.");

        return MapToDto(assessment);
    }

    [AbpAuthorize(PermissionNames.Admissions_Assessments_View)]
    public async Task<AdmissionAssessmentDto> GetByApplicationAsync(Guid applicationId)
    {
        var assessment = await _assessmentRepository
            .GetAll()
            .Include(a => a.Application)
                .ThenInclude(app => app.AppliedGrade)
            .Include(a => a.AssessedGrade)
            .FirstOrDefaultAsync(a => a.ApplicationId == applicationId && a.TenantId == AbpSession.TenantId);

        if (assessment == null)
            return null;

        return MapToDto(assessment);
    }

    [AbpAuthorize(PermissionNames.Admissions_Assessments_View)]
    public async Task<PagedResultDto<AdmissionAssessmentDto>> GetAllAsync(GetAdmissionAssessmentsInput input)
    {
        var query = _assessmentRepository
            .GetAll()
            .Include(a => a.Application)
                .ThenInclude(app => app.AppliedGrade)
            .Include(a => a.AssessedGrade)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                a => a.Application.ProspectiveStudentFirstName.ToLower().Contains(input.Keyword.ToLower())
                    || a.Application.ProspectiveStudentLastName.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var assessments = await query
            .OrderBy(input.Sorting ?? "ScheduledDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<AdmissionAssessmentDto>(
            totalCount,
            assessments.Select(MapToDto).ToList());
    }

    [AbpAuthorize(PermissionNames.Admissions_Assessments_Schedule)]
    public async Task<AdmissionAssessmentDto> ScheduleAsync(ScheduleAssessmentDto input)
    {
        var application = await _applicationRepository.GetAsync(input.ApplicationId);

        if (application.Status != ApplicationStatus.UnderReview)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "Application must be under review to schedule an assessment.");

        // Check if assessment already exists
        var existingAssessment = await _assessmentRepository
            .FirstOrDefaultAsync(a => a.ApplicationId == input.ApplicationId);

        if (existingAssessment != null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.AssessmentAlreadyScheduled,
                "An assessment has already been scheduled for this application.");

        // Validate minimum notice (ADM-015: 7 days)
        if (input.ScheduledDate < DateTime.UtcNow.AddDays(MinNoticeDaysRequired))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InsufficientAssessmentNotice,
                $"Assessment must be scheduled at least {MinNoticeDaysRequired} days in advance.");

        var assessment = new AdmissionAssessment(
            Guid.NewGuid(),
            input.ApplicationId,
            input.Type,
            input.ScheduledDate,
            input.AssessedGradeId,
            input.AssessorUserId)
        {
            TenantId = AbpSession.TenantId,
            Subjects = input.Subjects,
            MaxScore = input.MaxScore
        };

        await _assessmentRepository.InsertAsync(assessment);

        // Update application status
        application.ScheduleAssessment();
        await _applicationRepository.UpdateAsync(application);

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(assessment.Id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Assessments_RecordResults)]
    public async Task<AdmissionAssessmentDto> RecordResultsAsync(Guid id, RecordAssessmentResultsDto input)
    {
        var assessment = await _assessmentRepository.GetAsync(id);

        if (assessment.CompletedDate.HasValue)
            throw new UserFriendlyException(AdmissionsExceptionCodes.AssessmentAlreadyCompleted,
                "Assessment results have already been recorded.");

        // Record results using entity method
        assessment.RecordResults(input.TotalScore, input.MaxScore, input.PassPercentage, input.Feedback);

        await _assessmentRepository.UpdateAsync(assessment);

        // Update application status
        var application = await _applicationRepository.GetAsync(assessment.ApplicationId);
        application.CompleteAssessment();
        await _applicationRepository.UpdateAsync(application);

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Assessments_Cancel)]
    public async Task CancelAsync(Guid id, string reason)
    {
        var assessment = await _assessmentRepository.GetAsync(id);

        if (assessment.CompletedDate.HasValue)
            throw new UserFriendlyException(AdmissionsExceptionCodes.CannotCancelAssessment,
                "Cannot cancel a completed assessment.");

        // Revert application status back to UnderReview
        var application = await _applicationRepository.GetAsync(assessment.ApplicationId);
        if (application.Status == ApplicationStatus.AssessmentScheduled)
        {
            application.RevertToUnderReview();
            await _applicationRepository.UpdateAsync(application);
        }

        // Delete the assessment
        await _assessmentRepository.DeleteAsync(id);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    #region Private Methods

    private AdmissionAssessmentDto MapToDto(AdmissionAssessment assessment)
    {
        var dto = ObjectMapper.Map<AdmissionAssessmentDto>(assessment);

        // Map additional properties
        dto.ApplicationNumber = assessment.Application?.ApplicationNumber;
        dto.ApplicantName = assessment.Application?.GetProspectiveStudentFullName();
        dto.GradeName = assessment.AssessedGrade?.GradeName;

        return dto;
    }

    #endregion
}
