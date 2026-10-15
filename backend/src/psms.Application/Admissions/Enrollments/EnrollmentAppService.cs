using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.Enrollments.Dto;
using psms.Admissions.Shared;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Admissions.Enrollments;

/// <summary>
/// Service for managing student enrollments.
/// Implements ADM-025 to ADM-029.
/// </summary>
[AbpAuthorize(PermissionNames.Admissions_Enrollment)]
public class EnrollmentAppService : ApplicationService, IEnrollmentAppService
{
    private readonly IRepository<Application, Guid> _applicationRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<Class, Guid> _classRepository;
    private readonly IRepository<ApplicantParent, Guid> _applicantParentRepository;
    private readonly IRepository<StudentParent, Guid> _studentParentRepository;
    private readonly IRepository<Parent, Guid> _parentRepository;
    private readonly IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> _settingsRepository;
    private readonly psms.Academic.Students.IStudentLoginProvisioner _loginProvisioner;
    private readonly ICurrentApplicantResolver _currentApplicant;

    public EnrollmentAppService(
        IRepository<Application, Guid> applicationRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<Class, Guid> classRepository,
        IRepository<ApplicantParent, Guid> applicantParentRepository,
        IRepository<StudentParent, Guid> studentParentRepository,
        IRepository<Parent, Guid> parentRepository,
        IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> settingsRepository,
        psms.Academic.Students.IStudentLoginProvisioner loginProvisioner,
        ICurrentApplicantResolver currentApplicant)
    {
        _applicationRepository = applicationRepository;
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _applicantParentRepository = applicantParentRepository;
        _studentParentRepository = studentParentRepository;
        _parentRepository = parentRepository;
        _settingsRepository = settingsRepository;
        _loginProvisioner = loginProvisioner;
        _currentApplicant = currentApplicant;
    }

    /// <summary>
    /// An applicant holds Enrollment.View so they can watch their own offer.
    /// Nothing checked that the record they asked for was theirs, so any id
    /// returned another family's child - name, date of birth and grade - and
    /// the pending list handed back every approved applicant at the school.
    /// Staff are told apart by Applications.ViewAll, which no applicant holds.
    /// </summary>
    private async Task<Application> GetOwnOrAnyAsync(Guid applicationId, IQueryable<Application> query)
    {
        var application = await query.FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound, "Application not found.");

        var ownerOnly = await _currentApplicant.GetOwnApplicationsOnlyForAsync();
        if (ownerOnly.HasValue && application.CreatorUserId != ownerOnly.Value)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound, "Application not found.");

        return application;
    }

    [AbpAuthorize(PermissionNames.Admissions_Enrollment_View)]
    public async Task<EnrollmentDto> GetByApplicationAsync(Guid applicationId)
    {
        var application = await GetOwnOrAnyAsync(applicationId, _applicationRepository
            .GetAll()
            .Include(a => a.AppliedGrade)
            .Include(a => a.CreatedStudent)
                .ThenInclude(s => s.CurrentClass));

        return MapToEnrollmentDto(application);
    }

    [AbpAuthorize(PermissionNames.Admissions_Enrollment_View)]
    public async Task<PagedResultDto<EnrollmentDto>> GetPendingEnrollmentsAsync(GetPendingEnrollmentsInput input)
    {
        // The queue of people waiting to be enrolled is the school's list. An
        // applicant asking for it gets their own application and nobody else's.
        var ownerOnly = await _currentApplicant.GetOwnApplicationsOnlyForAsync();

        var query = _applicationRepository
            .GetAll()
            .Include(a => a.AppliedGrade)
            .Where(a => a.Status == ApplicationStatus.Approved)
            .WhereIf(ownerOnly.HasValue, a => a.CreatorUserId == ownerOnly.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                a => a.ProspectiveStudentFirstName.ToLower().Contains(input.Keyword.ToLower())
                    || a.ProspectiveStudentLastName.ToLower().Contains(input.Keyword.ToLower())
                    || a.ApplicationNumber.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var applications = await query
            .OrderBy(input.Sorting ?? "DecisionDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<EnrollmentDto>(
            totalCount,
            applications.Select(MapToEnrollmentDto).ToList());
    }

    [AbpAuthorize(PermissionNames.Admissions_Enrollment_View)]
    public async Task<List<ClassAvailabilityDto>> GetAvailableClassesAsync(Guid gradeId)
    {
        var classes = await _classRepository
            .GetAll()
            .Include(c => c.Grade)
            .Include(c => c.ClassTeacher)
            .Include(c => c.Students)
            .Where(c => c.GradeId == gradeId && c.IsActive)
            .ToListAsync();

        return classes.Select(c => new ClassAvailabilityDto
        {
            ClassId = c.Id,
            ClassName = c.ClassName,
            GradeId = c.GradeId,
            GradeName = c.Grade?.GradeName,
            MaxCapacity = c.MaxCapacity,
            CurrentEnrolled = c.Students?.Count(s => s.IsActive) ?? 0,
            TeacherName = c.ClassTeacher != null
                ? $"{c.ClassTeacher.FirstName} {c.ClassTeacher.LastName}"
                : null
        }).ToList();
    }

    /// <summary>
    /// The family says yes (ADM-025).
    /// <para>
    /// This used to validate the offer carefully and then save nothing, under a
    /// note that acceptance "could be tracked with additional fields". Meanwhile
    /// the mapper reported every approved application as accepted, dated the day
    /// the school itself decided. The school was shown a yes nobody had given,
    /// and could enrol a child on it.
    /// </para>
    /// <para>
    /// Staff may record it too - families telephone, and some have no email -
    /// but it is then their act, against their name, rather than an assumption.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Admissions_Enrollment_AcceptOffer)]
    public async Task<EnrollmentDto> AcceptOfferAsync(AcceptOfferDto input)
    {
        var application = await GetOwnOrAnyAsync(input.ApplicationId,
            _applicationRepository.GetAll().Include(a => a.AppliedGrade));

        if (!input.ConfirmAcceptance)
            throw new UserFriendlyException("ACCEPTANCE_NOT_CONFIRMED",
                "Confirm that you are accepting the place before this can be recorded.");

        if (application.Status != ApplicationStatus.Approved)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "There is no offer on this application to accept.");

        if (application.OfferDeclinedDate.HasValue)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "This offer was turned down already. The school has to make a new one.");

        if (application.IsOfferExpired())
            throw new UserFriendlyException(AdmissionsExceptionCodes.OfferExpired,
                "This offer has expired. Speak to the school about extending it.");

        application.AcceptOffer(AbpSession.UserId);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetByApplicationAsync(input.ApplicationId);
    }

    /// <summary>
    /// The family turns the place down (ADM-026). The application ends and the
    /// place stops being held, which is what makes it worth asking.
    /// </summary>
    [AbpAuthorize(PermissionNames.Admissions_Enrollment_DeclineOffer)]
    public async Task<EnrollmentDto> DeclineOfferAsync(DeclineOfferDto input)
    {
        var application = await GetOwnOrAnyAsync(input.ApplicationId,
            _applicationRepository.GetAll().Include(a => a.AppliedGrade));

        if (!input.ConfirmDecline)
            throw new UserFriendlyException("DECLINE_NOT_CONFIRMED",
                "Confirm that you are turning the place down before this can be recorded.");

        if (application.Status != ApplicationStatus.Approved)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "There is no offer on this application to turn down.");

        if (application.OfferAcceptedDate.HasValue)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "This offer was accepted already. Speak to the school to withdraw instead.");

        application.DeclineOffer(input.Reason);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToEnrollmentDto(application);
    }

    [AbpAuthorize(PermissionNames.Admissions_Enrollment_AssignClass)]
    public async Task<EnrollmentDto> AssignClassAsync(AssignClassDto input)
    {
        var application = await _applicationRepository.GetAsync(input.ApplicationId);

        if (application.Status != ApplicationStatus.Approved)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "Only approved applications can be assigned to a class.");

        // Validate class exists and has capacity
        var targetClass = await _classRepository
            .GetAll()
            .Include(c => c.Students)
            .FirstOrDefaultAsync(c => c.Id == input.ClassId);

        if (targetClass == null)
            throw new UserFriendlyException("CLASS_NOT_FOUND", "Class not found.");

        if (!targetClass.IsActive)
            throw new UserFriendlyException("CLASS_NOT_ACTIVE", "Class is not active.");

        var currentEnrolled = targetClass.Students?.Count(s => s.IsActive) ?? 0;
        if (currentEnrolled >= targetClass.MaxCapacity)
            throw new UserFriendlyException(AdmissionsExceptionCodes.GradeCapacityFull,
                "The selected class has reached maximum capacity.");

        // Validate class is for the correct grade
        if (targetClass.GradeId != application.AppliedGradeId)
            throw new UserFriendlyException("CLASS_GRADE_MISMATCH",
                "The selected class is not for the applied grade.");

        // Note: Class assignment is tracked when completing enrollment

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetByApplicationAsync(input.ApplicationId);
    }

    [AbpAuthorize(PermissionNames.Admissions_Enrollment_Complete)]
    public async Task<EnrollmentDto> CompleteEnrollmentAsync(CompleteEnrollmentDto input)
    {
        var application = await _applicationRepository
            .GetAll()
            .Include(a => a.AppliedGrade)
            .FirstOrDefaultAsync(a => a.Id == input.ApplicationId);

        if (application == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound, "Application not found.");

        if (application.Status != ApplicationStatus.Approved)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "Only approved applications can complete enrollment.");

        if (application.CreatedStudentId.HasValue)
            throw new UserFriendlyException("ALREADY_ENROLLED",
                "This application has already been enrolled.");

        /* A place is given, not taken. Enrolment creates a learner record and a
           portal sign-in for somebody's child, and it used to run off an
           acceptance the mapper had invented from the school's own decision.
           If the family has not said yes on their own screen, somebody at the
           school has to record that they said yes some other way - which is a
           deliberate act with a name against it, and takes two clicks. */
        if (!application.OfferAcceptedDate.HasValue)
            throw new UserFriendlyException(AdmissionsExceptionCodes.OfferNotAccepted,
                "This family has not accepted the place yet. They can accept it on their own "
                + "application screen, or record their acceptance here if they told the school "
                + "another way.");

        // Validate class exists and has capacity
        var targetClass = await _classRepository
            .GetAll()
            .Include(c => c.Students)
            .Include(c => c.Grade)
            .FirstOrDefaultAsync(c => c.Id == input.ClassId);

        if (targetClass == null)
            throw new UserFriendlyException("CLASS_NOT_FOUND", "Class not found.");

        var currentEnrolled = targetClass.Students?.Count(s => s.IsActive) ?? 0;
        if (currentEnrolled >= targetClass.MaxCapacity)
            throw new UserFriendlyException(AdmissionsExceptionCodes.GradeCapacityFull,
                "The selected class has reached maximum capacity.");

        /* ADM-028, and a different thing from the class above: that is how many
           desks are in a room, this is how many new learners the school said it
           would take into this grade this year.

           The count was incremented on every enrolment and never checked, so a
           grade with two places could take a third and a fourth. The available
           figure floors at zero, so the overshoot never even showed. Approving
           more applicants than there are seats is normal and expected; seating
           more children than there is room for is not. Checked here, before a
           student record and a portal login are created for them. */
        var settings = await _settingsRepository
            .FirstOrDefaultAsync(s => s.AcademicYearId == application.AcademicYearId
                && s.GradeId == application.AppliedGradeId);

        if (settings != null && settings.IsAtCapacity())
            throw new UserFriendlyException(AdmissionsExceptionCodes.GradeCapacityFull,
                $"{application.AppliedGrade?.GradeName ?? "This grade"} is full — all "
                + $"{settings.MaxCapacity} places for the year are taken. Raise the intake first, "
                + "or offer this applicant a place on the waiting list.");

        // Generate admission number if not provided
        var admissionNumber = input.AdmissionNumber ?? await GenerateAdmissionNumberAsync();

        // Create student record (ADM-026)
        var student = new Student(
            Guid.NewGuid(),
            AbpSession.TenantId,
            application.ProspectiveStudentFirstName,
            application.ProspectiveStudentLastName,
            application.DateOfBirth,
            application.Gender,
            admissionNumber,
            DateTime.UtcNow,
            application.AppliedGradeId,
            input.ClassId)
        {
            MiddleName = application.ProspectiveStudentMiddleName,
            IdNumber = application.IdNumber,
            PassportNumber = application.PassportNumber,
            IsSACitizen = application.IsSACitizen,
            EmergencyContactName = input.EmergencyContactName,
            EmergencyContactPhone = input.EmergencyContactPhone,
            MedicalConditions = input.MedicalConditions,
            POPIAConsentGiven = input.POPIAConsentGiven,
            POPIAConsentDate = input.POPIAConsentGiven ? DateTime.UtcNow : null
        };

        await _studentRepository.InsertAsync(student);

        // LC-07: provision the student's portal login in the same unit of work,
        // exactly like the manual StudentAppService path, so admissions-enrolled
        // students also get a linked account (and can join their live classes).
        var login = await _loginProvisioner.ProvisionAsync(student);

        // Link parents to student (ADM-027)
        await LinkParentsToStudentAsync(application.Id, student.Id);

        // Update application status to enrolled
        application.MarkAsEnrolled(student.Id);
        await _applicationRepository.UpdateAsync(application);

        // One more place taken. The capacity that allowed it was checked above,
        // before any record was created for this learner.
        if (settings != null)
        {
            settings.IncrementEnrolledCount();
            await _settingsRepository.UpdateAsync(settings);
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        var dto = await GetByApplicationAsync(input.ApplicationId);
        // Surface the freshly-provisioned login once so the admissions officer
        // can hand the credentials to the family (never returned on reads).
        dto.LoginUserName = login.UserName;
        dto.TemporaryPassword = login.TemporaryPassword;
        return dto;
    }

    #region Private Methods

    private EnrollmentDto MapToEnrollmentDto(Application application)
    {
        return new EnrollmentDto
        {
            Id = application.Id,
            ApplicationId = application.Id,
            ApplicationNumber = application.ApplicationNumber,
            ApplicantName = application.GetProspectiveStudentFullName(),
            FirstName = application.ProspectiveStudentFirstName,
            LastName = application.ProspectiveStudentLastName,
            DateOfBirth = application.DateOfBirth,
            Gender = application.Gender,
            GradeId = application.AppliedGradeId,
            GradeName = application.AppliedGrade?.GradeName,
            AssignedClassId = application.CreatedStudent?.CurrentClassId,
            AssignedClassName = application.CreatedStudent?.CurrentClass?.ClassName,
            IsOfferAccepted = application.OfferAcceptedDate.HasValue,
            IsFormsSubmitted = application.CreatedStudentId.HasValue,
            IsClassAssigned = application.CreatedStudent?.CurrentClassId != null,
            IsEnrollmentComplete = application.Status == ApplicationStatus.Enrolled,
            OfferAcceptedDate = application.OfferAcceptedDate,
            OfferAcceptedByUserId = application.OfferAcceptedByUserId,
            OfferDeclinedDate = application.OfferDeclinedDate,
            OfferDeclineReason = application.OfferDeclineReason,
            OfferExpiryDate = application.ExpiryDate,
            IsOfferExpired = application.IsOfferExpired(),
            EnrollmentCompletedDate = application.Status == ApplicationStatus.Enrolled ? application.LastModificationTime : null,
            CreatedStudentId = application.CreatedStudentId,
            AdmissionNumber = application.CreatedStudent?.AdmissionNumber
        };
    }

    private async Task<string> GenerateAdmissionNumberAsync()
    {
        var tenantId = AbpSession.TenantId ?? 0;
        var year = DateTime.UtcNow.Year;
        var prefix = $"STU-{tenantId:D3}-{year}-";

        // Get the highest existing sequence number to avoid race conditions
        var lastAdmNumber = await _studentRepository
            .GetAll()
            .Where(s => s.TenantId == AbpSession.TenantId && s.AdmissionNumber.StartsWith(prefix))
            .OrderByDescending(s => s.AdmissionNumber)
            .Select(s => s.AdmissionNumber)
            .FirstOrDefaultAsync();

        var nextSequence = 1;
        if (lastAdmNumber != null)
        {
            var lastSequence = lastAdmNumber.Substring(prefix.Length);
            if (int.TryParse(lastSequence, out var parsed))
                nextSequence = parsed + 1;
        }

        return $"{prefix}{nextSequence:D4}";
    }

    private async Task LinkParentsToStudentAsync(Guid applicationId, Guid studentId)
    {
        // Get applicant parents
        var applicantParents = await _applicantParentRepository
            .GetAll()
            .Where(ap => ap.ApplicationId == applicationId)
            .ToListAsync();

        foreach (var applicantParent in applicantParents)
        {
            // Check if parent already exists (by ID number or email, case-insensitive)
            var normalizedEmail = applicantParent.Email?.Trim().ToLowerInvariant();
            var existingParent = await _parentRepository
                .FirstOrDefaultAsync(p =>
                    (applicantParent.IdNumber != null && p.IdNumber == applicantParent.IdNumber) ||
                    (normalizedEmail != null && p.Email.ToLower() == normalizedEmail));

            Guid parentId;

            if (existingParent != null)
            {
                parentId = existingParent.Id;
            }
            else
            {
                // Create new parent record
                // Note: UserId will be 0 initially, and can be linked when parent creates account
                var parent = new Parent(
                    Guid.NewGuid(),
                    AbpSession.TenantId,
                    0, // UserId - to be linked when parent creates account
                    applicantParent.FirstName,
                    applicantParent.LastName,
                    applicantParent.Email,
                    applicantParent.PhoneNumber)
                {
                    IdNumber = applicantParent.IdNumber,
                    WorkPhone = applicantParent.AlternatePhone,
                    Occupation = applicantParent.Occupation,
                    Employer = applicantParent.Employer
                };

                await _parentRepository.InsertAsync(parent);
                parentId = parent.Id;
            }

            // Create student-parent link
            var studentParent = new StudentParent(
                Guid.NewGuid(),
                studentId,
                parentId,
                applicantParent.Relationship)
            {
                IsPrimaryContact = applicantParent.IsPrimaryContact,
                IsFinanciallyResponsible = applicantParent.IsFinanciallyResponsible,
                CanPickupStudent = true,
                LivesWithStudent = applicantParent.IsPrimaryContact
            };

            await _studentParentRepository.InsertAsync(studentParent);
        }
    }

    #endregion
}
