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

    public EnrollmentAppService(
        IRepository<Application, Guid> applicationRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<Class, Guid> classRepository,
        IRepository<ApplicantParent, Guid> applicantParentRepository,
        IRepository<StudentParent, Guid> studentParentRepository,
        IRepository<Parent, Guid> parentRepository,
        IRepository<Domain.Admissions.Entities.AdmissionSettings, Guid> settingsRepository)
    {
        _applicationRepository = applicationRepository;
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _applicantParentRepository = applicantParentRepository;
        _studentParentRepository = studentParentRepository;
        _parentRepository = parentRepository;
        _settingsRepository = settingsRepository;
    }

    [AbpAuthorize(PermissionNames.Admissions_Enrollment_View)]
    public async Task<EnrollmentDto> GetByApplicationAsync(Guid applicationId)
    {
        var application = await _applicationRepository
            .GetAll()
            .Include(a => a.AppliedGrade)
            .Include(a => a.CreatedStudent)
                .ThenInclude(s => s.CurrentClass)
            .FirstOrDefaultAsync(a => a.Id == applicationId);

        if (application == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound, "Application not found.");

        return MapToEnrollmentDto(application);
    }

    [AbpAuthorize(PermissionNames.Admissions_Enrollment_View)]
    public async Task<PagedResultDto<EnrollmentDto>> GetPendingEnrollmentsAsync(PagedAndSortedResultRequestDto input)
    {
        var query = _applicationRepository
            .GetAll()
            .Include(a => a.AppliedGrade)
            .Where(a => a.Status == ApplicationStatus.Approved);

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

    [AbpAuthorize(PermissionNames.Admissions_Enrollment_AcceptOffer)]
    public async Task<EnrollmentDto> AcceptOfferAsync(AcceptOfferDto input)
    {
        var application = await _applicationRepository
            .GetAll()
            .Include(a => a.AppliedGrade)
            .FirstOrDefaultAsync(a => a.Id == input.ApplicationId);

        if (application == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.ApplicationNotFound, "Application not found.");

        if (application.Status != ApplicationStatus.Approved)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "Only approved applications can accept offers.");

        if (!input.ConfirmAcceptance)
            throw new UserFriendlyException("ACCEPTANCE_NOT_CONFIRMED",
                "You must confirm acceptance of the offer.");

        // Check if offer has expired
        if (application.IsOfferExpired())
            throw new UserFriendlyException(AdmissionsExceptionCodes.OfferExpired,
                "The admission offer has expired.");

        // Note: Application status remains Approved until enrollment is complete
        // This could be tracked with additional fields or a separate enrollment record

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetByApplicationAsync(input.ApplicationId);
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

        // Link parents to student (ADM-027)
        await LinkParentsToStudentAsync(application.Id, student.Id);

        // Update application status to enrolled
        application.MarkAsEnrolled(student.Id);
        await _applicationRepository.UpdateAsync(application);

        // Increment admission settings enrollment count (ADM-028)
        var settings = await _settingsRepository
            .FirstOrDefaultAsync(s => s.AcademicYearId == application.AcademicYearId
                && s.GradeId == application.AppliedGradeId);
        if (settings != null)
        {
            settings.CurrentEnrolledCount++;
            await _settingsRepository.UpdateAsync(settings);
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetByApplicationAsync(input.ApplicationId);
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
            IsOfferAccepted = application.Status == ApplicationStatus.Approved || application.Status == ApplicationStatus.Enrolled,
            IsFormsSubmitted = application.CreatedStudentId.HasValue,
            IsClassAssigned = application.CreatedStudent?.CurrentClassId != null,
            IsEnrollmentComplete = application.Status == ApplicationStatus.Enrolled,
            OfferAcceptedDate = application.DecisionDate,
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
