using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.AdmissionSettings;
using psms.Admissions.AdmissionSettings.Dto;
using psms.Admissions.Applications.Dto;
using psms.Admissions.Shared;
using psms.Authorization;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Shared.Validators;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Shared;
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
    private readonly WorkflowStarterService _workflowStarter;
    private readonly ICurrentApplicantResolver _currentApplicant;
    private readonly IRepository<psms.Authorization.Users.User, long> _userRepository;

    public ApplicationAppService(
        IRepository<Application, Guid> applicationRepository,
        IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> settingsRepository,
        IRepository<ApplicantParent, Guid> parentRepository,
        WorkflowStarterService workflowStarter,
        ICurrentApplicantResolver currentApplicant,
        IRepository<psms.Authorization.Users.User, long> userRepository)
    {
        _currentApplicant = currentApplicant;
        _userRepository = userRepository;
        _applicationRepository = applicationRepository;
        _settingsRepository = settingsRepository;
        _parentRepository = parentRepository;
        _workflowStarter = workflowStarter;
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    public async Task<ApplicationDto> GetAsync(Guid id)
    {
        var application = await GetApplicationWithDetailsAsync(id);

        if (application == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound, "Application not found.");

        await AssertMineIfApplicantAsync(application);

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

        await AssertMineIfApplicantAsync(application);

        return ObjectMapper.Map<ApplicationDto>(application);
    }

    /// <summary>
    /// The applications this parent has started, so the apply screen can find
    /// them again.
    /// <para>
    /// There was no way to. Listing applications needs
    /// Applications.ViewAll — "every application at this school" — which an
    /// applicant does not and should not hold, so a parent who signed back in
    /// had no route to the draft they had already begun.
    /// </para>
    /// <para>
    /// Staff get the same answer from GetAll, which is scoped to the school
    /// and filterable; this is deliberately only ever the caller's own.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Admissions_Applications_View)]
    public async Task<ListResultDto<ApplicationListDto>> GetMineAsync()
    {
        // The resolver already knows who this applicant is; for staff asking
        // the same question, "mine" is simply what they created.
        var ownerId = await _currentApplicant.GetOwnApplicationsOnlyForAsync() ?? AbpSession.UserId;

        if (!ownerId.HasValue)
            return new ListResultDto<ApplicationListDto>(new List<ApplicationListDto>());

        var mine = await _applicationRepository
            .GetAll()
            .Include(a => a.AppliedGrade)
            .Include(a => a.AcademicYear)
            .Where(a => a.CreatorUserId == ownerId.Value)
            .OrderByDescending(a => a.CreationTime)
            .ToListAsync();

        return new ListResultDto<ApplicationListDto>(
            ObjectMapper.Map<List<ApplicationListDto>>(mine));
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
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                a => a.ProspectiveStudentFirstName.ToLower().Contains(input.Keyword.ToLower())
                    || a.ProspectiveStudentLastName.ToLower().Contains(input.Keyword.ToLower())
                    || a.ApplicationNumber.ToLower().Contains(input.Keyword.ToLower()))
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

    /// <summary>
    /// What a prospective parent may apply for at this school, and on what
    /// terms.
    /// <para>
    /// The application form needs an academic year and a grade. An applicant
    /// holds neither <c>Academic.Calendar.View</c> nor
    /// <c>Academic.Grades.View</c>, and should not: a parent applying has no
    /// business reading the school's whole timetable of years and grades. So
    /// the question is answered in admissions terms instead, and only for
    /// intakes that are actually open.
    /// </para>
    /// <para>
    /// It lives on this service rather than beside the other settings calls
    /// because of where ABP checks permissions: a method-level attribute does
    /// not replace the class-level one, it adds to it. AdmissionSettingsAppService
    /// is guarded by Admissions.Settings, which an applicant does not hold, so
    /// the method was refused before its own guard was ever consulted.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Admissions_Applications_Create)]
    public async Task<ListResultDto<OpenIntakeDto>> GetOpenIntakesAsync()
    {
        var settings = await _settingsRepository
            .GetAll()
            .Include(s => s.AcademicYear)
            .Include(s => s.Grade)
            .ToListAsync();

        var open = settings
            .Where(x => x.AreApplicationsOpen() && !x.IsAtCapacity())
            .OrderBy(x => x.AcademicYear.Year)
            .ThenBy(x => x.GradeId == null ? 0 : 1)
            .ThenBy(x => x.Grade == null ? 0 : (int)x.Grade.GradeLevel)
            .Select(x => new OpenIntakeDto
            {
                AcademicYearId = x.AcademicYearId,
                AcademicYearName = x.AcademicYear?.YearName,
                GradeId = x.GradeId,
                GradeName = x.Grade?.GradeName,
                ApplicationCloseDate = x.ApplicationCloseDate,
                FeeRequired = x.RequiresApplicationFee(),
                FeeAmount = x.ApplicationFeeAmount,
                IsInterviewRequired = x.IsInterviewRequired,
                IsAssessmentRequired = x.IsAssessmentRequired,
                MinimumAge = x.MinimumAge,
                MaximumAge = x.MaximumAge,
                RequiredDocuments = x.RequiredDocuments,
                AvailableSpots = x.MaxCapacity.HasValue ? x.GetAvailableSpots() : (int?)null,
            })
            .ToList();

        return new ListResultDto<OpenIntakeDto>(open);
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

        if (input.IsSACitizen && !string.IsNullOrWhiteSpace(input.IdNumber) && !SAIdNumberValidator.IsValid(input.IdNumber))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidSaIdNumber,
                "Invalid South African ID number.");

        if (!input.IsSACitizen && string.IsNullOrWhiteSpace(input.PassportNumber))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidProspectiveStudentInfo,
                "Passport number is required for non-South African citizens.");

        // Where the school writes about this. A parent applying for their own
        // child is signed in, so their account's address is the answer and the
        // form does not have to ask for something it already knows.
        var contactEmail = input.CreatorEmailAddress;
        if (string.IsNullOrWhiteSpace(contactEmail))
        {
            contactEmail = await _userRepository
                .GetAll()
                .Where(u => u.Id == (AbpSession.UserId ?? 0))
                .Select(u => u.EmailAddress)
                .FirstOrDefaultAsync();
        }

        if (string.IsNullOrWhiteSpace(contactEmail))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidProspectiveStudentInfo,
                "An email address is needed so the school can write to you about this application.");

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
            contactEmail)
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
        await AssertMineIfApplicantAsync(application);

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
        {
            // Re-validate age for the new grade (ADM-004)
            var dob = input.DateOfBirth ?? application.DateOfBirth;
            var newSettings = await GetAdmissionSettingsAsync(application.AcademicYearId, input.ApplyingForGradeId.Value);
            ValidateAgeForGrade(dob, newSettings);
            application.AppliedGradeId = input.ApplyingForGradeId.Value;
        }

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
        await AssertMineIfApplicantAsync(application);

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

        await AssertMineIfApplicantAsync(application);

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

        // Whether the school charges for this grade and year decides where a
        // submitted application lands: waiting for money, or straight in front
        // of the admissions officer.
        var settings = await GetAdmissionSettingsAsync(
            application.AcademicYearId, application.AppliedGradeId);

        var feeRequired = settings.RequiresApplicationFee();

        application.Submit(feeRequired);

        await _applicationRepository.UpdateAsync(application);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Review is what the admissions workflow is for, and until now only
        // paying for the application ever started it. A school that charges
        // nothing would have had every application reach review with no
        // workflow behind it.
        if (!feeRequired)
            await TryStartAdmissionsWorkflowAsync(application.Id);

        return ObjectMapper.Map<ApplicationDto>(application);
    }

    /// <summary>
    /// Puts the application in front of the admissions workflow, if the school
    /// has one configured. Never allowed to fail the thing that triggered it:
    /// an application that was legitimately submitted or paid for stays that
    /// way even if the workflow could not be started.
    /// </summary>
    private async Task TryStartAdmissionsWorkflowAsync(Guid applicationId)
    {
        try
        {
            await _workflowStarter.TryStartWorkflowAsync(
                AbpSession.TenantId,
                WorkflowEntityType.Application,
                applicationId,
                AbpSession.UserId ?? 0,
                (AbpSession.UserId ?? 0).ToString());
        }
        catch (Exception ex)
        {
            Logger.Warn($"Could not start the admissions workflow for application {applicationId}: {ex.Message}");
        }
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_Withdraw)]
    public async Task<ApplicationDto> WithdrawAsync(Guid id, string reason = null)
    {
        var application = await _applicationRepository.GetAsync(id);
        await AssertMineIfApplicantAsync(application);

        if (application == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound, "Application not found.");

        /* The entity guards this itself and says no with an InvalidOperationException,
           which would reach the parent as "An internal error occurred during your
           request!". They are entitled to know which of the two it is: a learner
           already enrolled, or an application already closed. */
        try
        {
            application.Withdraw(reason);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(
                AdmissionsExceptionCodes.ApplicationCannotBeWithdrawn,
                application.Status == ApplicationStatus.Enrolled
                    ? "This learner has already been enrolled, so the application can no longer be withdrawn. Speak to the school."
                    : "This application has already been closed, so there is nothing to withdraw.",
                ex);
        }

        await _applicationRepository.UpdateAsync(application);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Applications_ViewAll)]
    public async Task<ApplicationStatisticsDto> GetStatisticsAsync(Guid academicYearId, Guid? gradeId = null)
    {
        var statusCounts = await _applicationRepository
            .GetAll()
            .Where(a => a.AcademicYearId == academicYearId)
            .WhereIf(gradeId.HasValue, a => a.AppliedGradeId == gradeId.Value)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int Count(params ApplicationStatus[] statuses) =>
            statusCounts.Where(s => statuses.Contains(s.Status)).Sum(s => s.Count);

        return new ApplicationStatisticsDto
        {
            AcademicYearId = academicYearId,
            TotalApplications = statusCounts.Sum(s => s.Count),
            DraftApplications = Count(ApplicationStatus.Draft),
            SubmittedApplications = Count(ApplicationStatus.PaymentPending),
            UnderReviewApplications = Count(ApplicationStatus.UnderReview, ApplicationStatus.DocumentsRequired,
                ApplicationStatus.InterviewScheduled, ApplicationStatus.AssessmentScheduled),
            UnderConsiderationApplications = Count(ApplicationStatus.UnderConsideration),
            ApprovedApplications = Count(ApplicationStatus.Approved),
            RejectedApplications = Count(ApplicationStatus.Rejected),
            WaitlistedApplications = Count(ApplicationStatus.Waitlisted),
            EnrolledApplications = Count(ApplicationStatus.Enrolled),
            WithdrawnApplications = Count(ApplicationStatus.Withdrawn),
            ExpiredApplications = Count(ApplicationStatus.Expired)
        };
    }

    #region Private Helper Methods

    /// <summary>
    /// A prospective parent reads their own application and nobody else's.
    /// <para>
    /// They hold Applications.View so they can see theirs, and nothing checked
    /// that it was theirs. Application numbers run in sequence —
    /// APP-003-2026-00011 — so another family's application was reachable by
    /// counting: a child's name, date of birth, ID number, and both parents'
    /// contact details.
    /// </para>
    /// <para>
    /// Not-found rather than forbidden, so this cannot be used to discover
    /// which application numbers exist.
    /// </para>
    /// </summary>
    private async Task AssertMineIfApplicantAsync(Application application)
    {
        var ownerOnly = await _currentApplicant.GetOwnApplicationsOnlyForAsync();

        if (ownerOnly.HasValue && application.CreatorUserId != ownerOnly.Value)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound,
                "Application not found.");
    }

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
        var prefix = $"APP-{tenantId:D3}-{year}-";

        // Get the highest existing sequence number to avoid race conditions
        var lastAppNumber = await _applicationRepository
            .GetAll()
            .Where(a => a.ApplicationNumber.StartsWith(prefix))
            .OrderByDescending(a => a.ApplicationNumber)
            .Select(a => a.ApplicationNumber)
            .FirstOrDefaultAsync();

        var nextSequence = 1;
        if (lastAppNumber != null)
        {
            var lastSequence = lastAppNumber.Substring(prefix.Length);
            if (int.TryParse(lastSequence, out var parsed))
                nextSequence = parsed + 1;
        }

        return $"{prefix}{nextSequence:D5}";
    }

    #endregion
}
