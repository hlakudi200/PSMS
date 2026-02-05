using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.AdmissionSettings;
using psms.Admissions.Applications.Dto;
using psms.Admissions.Shared;
using psms.Authorization;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Admissions.Applications;

/// <summary>
/// Service for managing admission applications.
/// Implements ADM-001 to ADM-005.
/// </summary>
[AbpAuthorize(PermissionNames.Admissions_Applications)]
public class ApplicationAppService : ApplicationService, IApplicationAppService
{
    private readonly IRepository<Application, Guid> _applicationRepository;
    private readonly IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> _settingsRepository;
    private readonly IRepository<ApplicantParent, Guid> _parentRepository;

    public ApplicationAppService(
        IRepository<Application, Guid> applicationRepository,
        IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> settingsRepository,
        IRepository<ApplicantParent, Guid> parentRepository)
    {
        _applicationRepository = applicationRepository;
        _settingsRepository = settingsRepository;
        _parentRepository = parentRepository;
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    public async Task<ApplicationDto> GetAsync(Guid id)
    {
        var application = await GetApplicationWithDetailsAsync(id);

        if (application == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound, "Application not found.");

        return ObjectMapper.Map<ApplicationDto>(application);
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    public async Task<ApplicationDto> GetByApplicationNumberAsync(string applicationNumber)
    {
        var application = await _applicationRepository
            .GetAll()
            .Include(a => a.AppliedGrade)
            .Include(a => a.AcademicYear)
            .Include(a => a.ApplicantParents)
            .Include(a => a.ApplicationDocuments)
            .Include(a => a.ApplicationFee)
            .Include(a => a.AdmissionInterview)
            .Include(a => a.AdmissionAssessment)
            .Include(a => a.Waitlist)
            .FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);

        if (application == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidApplicationNumber, "Application not found with this number.");

        return ObjectMapper.Map<ApplicationDto>(application);
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_ViewAll)]
    public async Task<PagedResultDto<ApplicationListDto>> GetAllAsync(GetApplicationsInput input)
    {
        var query = _applicationRepository
            .GetAll()
            .Include(a => a.AppliedGrade)
            .Include(a => a.AcademicYear)
            .Include(a => a.ApplicantParents)
            .Include(a => a.ApplicationDocuments)
            .Include(a => a.ApplicationFee)
            .Include(a => a.AdmissionInterview)
            .Include(a => a.AdmissionAssessment)
            .Include(a => a.Waitlist)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ApplicationNumber),
                a => a.ApplicationNumber.Contains(input.ApplicationNumber))
            .WhereIf(!string.IsNullOrWhiteSpace(input.ApplicantName),
                a => a.ProspectiveStudentFirstName.Contains(input.ApplicantName)
                    || a.ProspectiveStudentLastName.Contains(input.ApplicantName))
            .WhereIf(input.Status.HasValue, a => a.Status == input.Status.Value)
            .WhereIf(input.AcademicYearId.HasValue, a => a.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.GradeId.HasValue, a => a.AppliedGradeId == input.GradeId.Value)
            .WhereIf(input.SubmittedDateFrom.HasValue, a => a.SubmissionDate >= input.SubmittedDateFrom.Value)
            .WhereIf(input.SubmittedDateTo.HasValue, a => a.SubmissionDate <= input.SubmittedDateTo.Value)
            .WhereIf(input.IsFeePaid.HasValue, a => input.IsFeePaid.Value
                ? a.ApplicationFee != null && a.ApplicationFee.Status == PaymentStatus.Completed
                : a.ApplicationFee == null || a.ApplicationFee.Status != PaymentStatus.Completed)
            .WhereIf(input.IsOnWaitlist.HasValue, a => input.IsOnWaitlist.Value
                ? a.Status == ApplicationStatus.Waitlisted
                : a.Status != ApplicationStatus.Waitlisted)
            .WhereIf(input.HasExpiredOffer.HasValue && input.HasExpiredOffer.Value,
                a => a.Status == ApplicationStatus.Approved && a.ExpiryDate < DateTime.UtcNow);

        var totalCount = await query.CountAsync();

        var applications = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<ApplicationListDto>(
            totalCount,
            ObjectMapper.Map<List<ApplicationListDto>>(applications));
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_Create)]
    public async Task<ApplicationDto> CreateAsync(CreateApplicationDto input)
    {
        // Validate admission settings exist and applications are open
        var settings = await GetAdmissionSettingsAsync(input.AcademicYearId, input.ApplyingForGradeId);

        if (!settings.IsAcceptingApplications)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationsNotOpen,
                "Applications are not currently being accepted for this grade.");

        // Validate age appropriateness (ADM-004)
        ValidateAgeForGrade(input.DateOfBirth, settings);

        // Validate SA ID if SA citizen (ADM-002)
        if (input.IsSACitizen && string.IsNullOrWhiteSpace(input.IdNumber))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidProspectiveStudentInfo,
                "SA ID number is required for South African citizens.");

        if (!input.IsSACitizen && string.IsNullOrWhiteSpace(input.PassportNumber))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidProspectiveStudentInfo,
                "Passport number is required for non-South African citizens.");

        // Generate application number (ADM-001)
        var applicationNumber = await GenerateApplicationNumberAsync(input.AcademicYearId);

        var application = new Application(
            Guid.NewGuid(),
            AbpSession.TenantId,
            applicationNumber,
            input.FirstName,
            input.LastName,
            input.DateOfBirth,
            input.Gender,
            input.ApplyingForGradeId,
            input.AcademicYearId,
            input.CreatorEmailAddress)
        {
            ProspectiveStudentMiddleName = input.MiddleName,
            IdNumber = input.IdNumber,
            PassportNumber = input.PassportNumber,
            IsSACitizen = input.IsSACitizen,
            PreviousSchool = input.PreviousSchool
        };

        await _applicationRepository.InsertAsync(application);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(application.Id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_Edit)]
    public async Task<ApplicationDto> UpdateAsync(Guid id, UpdateApplicationDto input)
    {
        var application = await _applicationRepository.GetAsync(id);

        if (application.Status != ApplicationStatus.Draft)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "Only draft applications can be edited.");

        // Update properties if provided
        if (!string.IsNullOrWhiteSpace(input.FirstName))
            application.ProspectiveStudentFirstName = input.FirstName;

        if (!string.IsNullOrWhiteSpace(input.LastName))
            application.ProspectiveStudentLastName = input.LastName;

        if (input.MiddleName != null)
            application.ProspectiveStudentMiddleName = input.MiddleName;

        if (input.DateOfBirth.HasValue)
            application.DateOfBirth = input.DateOfBirth.Value;

        if (input.Gender.HasValue)
            application.Gender = input.Gender.Value;

        if (!string.IsNullOrWhiteSpace(input.IdNumber))
            application.IdNumber = input.IdNumber;

        if (!string.IsNullOrWhiteSpace(input.PassportNumber))
            application.PassportNumber = input.PassportNumber;

        if (input.IsSACitizen.HasValue)
            application.IsSACitizen = input.IsSACitizen.Value;

        if (input.ApplyingForGradeId.HasValue)
            application.AppliedGradeId = input.ApplyingForGradeId.Value;

        if (!string.IsNullOrWhiteSpace(input.PreviousSchool))
            application.PreviousSchool = input.PreviousSchool;

        await _applicationRepository.UpdateAsync(application);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var application = await _applicationRepository.GetAsync(id);

        if (application.Status != ApplicationStatus.Draft)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "Only draft applications can be deleted.");

        await _applicationRepository.DeleteAsync(application);
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_Submit)]
    public async Task<ApplicationDto> SubmitAsync(Guid id)
    {
        var application = await GetApplicationWithDetailsAsync(id);

        if (application == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound, "Application not found.");

        // Validate at least one parent exists (ADM-003)
        if (application.ApplicantParents == null || !application.ApplicantParents.Any())
            throw new UserFriendlyException(AdmissionsExceptionCodes.ParentInformationRequired,
                "At least one parent/guardian is required before submitting.");

        // Validate at least one primary contact
        if (!application.ApplicantParents.Any(p => p.IsPrimaryContact))
            throw new UserFriendlyException(AdmissionsExceptionCodes.ParentInformationRequired,
                "At least one parent must be marked as primary contact.");

        // Validate at least one financially responsible parent
        if (!application.ApplicantParents.Any(p => p.IsFinanciallyResponsible))
            throw new UserFriendlyException(AdmissionsExceptionCodes.ParentInformationRequired,
                "At least one parent must be marked as financially responsible.");

        // Perform the status transition
        application.Submit();

        await _applicationRepository.UpdateAsync(application);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<ApplicationDto>(application);
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_Withdraw)]
    public async Task<ApplicationDto> WithdrawAsync(Guid id, string reason = null)
    {
        var application = await _applicationRepository.GetAsync(id);

        if (application == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound, "Application not found.");

        application.Withdraw(reason);

        await _applicationRepository.UpdateAsync(application);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_ViewAll)]
    public async Task<ApplicationStatisticsDto> GetStatisticsAsync(Guid academicYearId, Guid? gradeId = null)
    {
        var query = _applicationRepository
            .GetAll()
            .Where(a => a.AcademicYearId == academicYearId)
            .WhereIf(gradeId.HasValue, a => a.AppliedGradeId == gradeId.Value);

        var statistics = new ApplicationStatisticsDto
        {
            AcademicYearId = academicYearId,
            TotalApplications = await query.CountAsync(),
            DraftApplications = await query.CountAsync(a => a.Status == ApplicationStatus.Draft),
            SubmittedApplications = await query.CountAsync(a => a.Status == ApplicationStatus.PaymentPending),
            UnderReviewApplications = await query.CountAsync(a => a.Status == ApplicationStatus.UnderReview
                || a.Status == ApplicationStatus.DocumentsRequired
                || a.Status == ApplicationStatus.InterviewScheduled
                || a.Status == ApplicationStatus.AssessmentScheduled),
            UnderConsiderationApplications = await query.CountAsync(a => a.Status == ApplicationStatus.UnderConsideration),
            ApprovedApplications = await query.CountAsync(a => a.Status == ApplicationStatus.Approved),
            RejectedApplications = await query.CountAsync(a => a.Status == ApplicationStatus.Rejected),
            WaitlistedApplications = await query.CountAsync(a => a.Status == ApplicationStatus.Waitlisted),
            EnrolledApplications = await query.CountAsync(a => a.Status == ApplicationStatus.Enrolled),
            WithdrawnApplications = await query.CountAsync(a => a.Status == ApplicationStatus.Withdrawn),
            ExpiredApplications = await query.CountAsync(a => a.Status == ApplicationStatus.Expired)
        };

        return statistics;
    }

    #region Private Helper Methods

    private async Task<Application> GetApplicationWithDetailsAsync(Guid id)
    {
        return await _applicationRepository
            .GetAll()
            .Include(a => a.AppliedGrade)
            .Include(a => a.AcademicYear)
            .Include(a => a.ApplicantParents)
            .Include(a => a.ApplicationDocuments)
            .Include(a => a.ApplicationFee)
            .Include(a => a.AdmissionInterview)
            .Include(a => a.AdmissionAssessment)
            .Include(a => a.Waitlist)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    private async Task<Domain.Admissions.Entities.AdmissionSettings> GetAdmissionSettingsAsync(Guid academicYearId, Guid gradeId)
    {
        // Try grade-specific settings first
        var settings = await _settingsRepository
            .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == gradeId);

        // Fall back to default settings
        if (settings == null)
        {
            settings = await _settingsRepository
                .FirstOrDefaultAsync(s => s.AcademicYearId == academicYearId && s.GradeId == null);
        }

        if (settings == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.AdmissionSettingsNotFound,
                "Admission settings not found. Please contact the school administrator.");

        return settings;
    }

    private void ValidateAgeForGrade(DateTime dateOfBirth, Domain.Admissions.Entities.AdmissionSettings settings)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;

        if (settings.MinimumAge.HasValue && age < settings.MinimumAge.Value)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidGradeForAge,
                $"Student must be at least {settings.MinimumAge.Value} years old for this grade.");

        if (settings.MaximumAge.HasValue && age > settings.MaximumAge.Value)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidGradeForAge,
                $"Student must be no older than {settings.MaximumAge.Value} years old for this grade.");
    }

    private async Task<string> GenerateApplicationNumberAsync(Guid academicYearId)
    {
        var tenantId = AbpSession.TenantId ?? 0;
        var year = DateTime.UtcNow.Year;

        // Get the count of applications for this tenant and year
        var count = await _applicationRepository
            .CountAsync(a => a.CreationTime.Year == year);

        var sequence = (count + 1).ToString("D5");
        return $"APP-{tenantId:D3}-{year}-{sequence}";
    }

    #endregion
}
