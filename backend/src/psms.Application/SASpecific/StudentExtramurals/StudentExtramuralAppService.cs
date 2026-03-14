using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.SASpecific.Entities;
using psms.Domain.Shared.Enums;
using psms.SASpecific.Shared;
using psms.SASpecific.StudentExtramurals.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.SASpecific.StudentExtramurals;

/// <summary>
/// Service for managing student extramural enrollments.
/// </summary>
[AbpAuthorize(PermissionNames.SASpecific_Extramurals)]
public class StudentExtramuralAppService : ApplicationService, IStudentExtramuralAppService
{
    private readonly IRepository<StudentExtramural, Guid> _studentExtramuralRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<ExtramuralActivity, Guid> _extramuralActivityRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;

    public StudentExtramuralAppService(
        IRepository<StudentExtramural, Guid> studentExtramuralRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<ExtramuralActivity, Guid> extramuralActivityRepository,
        IRepository<AcademicYear, Guid> academicYearRepository)
    {
        _studentExtramuralRepository = studentExtramuralRepository;
        _studentRepository = studentRepository;
        _extramuralActivityRepository = extramuralActivityRepository;
        _academicYearRepository = academicYearRepository;
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_View)]
    public async Task<StudentExtramuralDto> GetAsync(Guid id)
    {
        var enrollment = await _studentExtramuralRepository
            .GetAll()
            .Include(se => se.Student)
            .Include(se => se.ExtramuralActivity)
            .Include(se => se.AcademicYear)
            .FirstOrDefaultAsync(se => se.Id == id && se.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentExtramuralNotFound,
                "Student extramural enrollment not found.");

        return ObjectMapper.Map<StudentExtramuralDto>(enrollment);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_View)]
    public async Task<PagedResultDto<StudentExtramuralListDto>> GetAllAsync(GetStudentExtramuralsInput input)
    {
        var query = _studentExtramuralRepository
            .GetAll()
            .Include(se => se.Student)
            .Include(se => se.ExtramuralActivity)
            .Include(se => se.AcademicYear)
            .Where(se => se.TenantId == AbpSession.TenantId)
            .WhereIf(input.StudentId.HasValue, se => se.StudentId == input.StudentId.Value)
            .WhereIf(input.ExtramuralActivityId.HasValue, se => se.ExtramuralActivityId == input.ExtramuralActivityId.Value)
            .WhereIf(input.AcademicYearId.HasValue, se => se.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.Status.HasValue, se => se.Status == input.Status.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                se => se.Student.FirstName.ToLower().Contains(input.Keyword.ToLower())
                    || se.Student.LastName.ToLower().Contains(input.Keyword.ToLower())
                    || se.ExtramuralActivity.ActivityName.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "StartDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<StudentExtramuralListDto>(
            totalCount,
            ObjectMapper.Map<List<StudentExtramuralListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_View)]
    public async Task<ListResultDto<StudentExtramuralListDto>> GetByActivityAsync(Guid activityId)
    {
        var items = await _studentExtramuralRepository
            .GetAll()
            .Include(se => se.Student)
            .Include(se => se.ExtramuralActivity)
            .Include(se => se.AcademicYear)
            .Where(se => se.TenantId == AbpSession.TenantId
                && se.ExtramuralActivityId == activityId)
            .OrderBy(se => se.Student.LastName)
            .ToListAsync();

        return new ListResultDto<StudentExtramuralListDto>(
            ObjectMapper.Map<List<StudentExtramuralListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_View)]
    public async Task<ListResultDto<StudentExtramuralListDto>> GetByStudentAsync(Guid studentId)
    {
        var items = await _studentExtramuralRepository
            .GetAll()
            .Include(se => se.Student)
            .Include(se => se.ExtramuralActivity)
            .Include(se => se.AcademicYear)
            .Where(se => se.TenantId == AbpSession.TenantId
                && se.StudentId == studentId)
            .OrderBy(se => se.StartDate)
            .ToListAsync();

        return new ListResultDto<StudentExtramuralListDto>(
            ObjectMapper.Map<List<StudentExtramuralListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Manage)]
    public async Task<StudentExtramuralDto> CreateAsync(CreateStudentExtramuralDto input)
    {
        // Validate Student exists (include Grade for eligibility check)
        var student = await _studentRepository
            .GetAll()
            .Include(s => s.CurrentGrade)
            .FirstOrDefaultAsync(s => s.Id == input.StudentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentNotFound,
                "Student not found.");

        // Validate ExtramuralActivity exists and is active
        var activity = await _extramuralActivityRepository
            .FirstOrDefaultAsync(ea => ea.Id == input.ExtramuralActivityId && ea.TenantId == AbpSession.TenantId);

        if (activity == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.ExtramuralActivityNotFound,
                "Extramural activity not found.");

        if (!activity.IsActive)
            throw new UserFriendlyException(SASpecificExceptionCodes.ExtramuralInactive,
                "Extramural activity is not active.");

        if (!activity.IsRegistrationOpen)
            throw new UserFriendlyException(SASpecificExceptionCodes.RegistrationClosed,
                "Registration is closed for this activity.");

        // Check grade eligibility
        if (student.CurrentGrade != null && !activity.IsGradeEligible((int)student.CurrentGrade.GradeLevel))
            throw new UserFriendlyException(SASpecificExceptionCodes.GradeNotEligible,
                "Student's grade is not eligible for this activity.");

        // Validate AcademicYear exists
        var academicYear = await _academicYearRepository
            .FirstOrDefaultAsync(ay => ay.Id == input.AcademicYearId && ay.TenantId == AbpSession.TenantId);

        if (academicYear == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.AcademicYearNotFound,
                "Academic year not found.");

        // Duplicate check: (StudentId, ExtramuralActivityId, AcademicYearId)
        var duplicateExists = await _studentExtramuralRepository
            .GetAll()
            .AnyAsync(se => se.TenantId == AbpSession.TenantId
                && se.StudentId == input.StudentId
                && se.ExtramuralActivityId == input.ExtramuralActivityId
                && se.AcademicYearId == input.AcademicYearId);

        if (duplicateExists)
            throw new UserFriendlyException(SASpecificExceptionCodes.DuplicateStudentExtramural,
                "This student is already enrolled in this extramural activity for the selected academic year.");

        // Enroll student (checks capacity)
        try
        {
            activity.EnrollStudent();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(SASpecificExceptionCodes.ExtramuralAtCapacity,
                "Extramural activity is at full capacity.");
        }

        var enrollment = new StudentExtramural(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.StudentId,
            input.ExtramuralActivityId,
            input.AcademicYearId,
            input.StartDate)
        {
            TermNumber = input.TermNumber,
            TeamAssignment = input.TeamAssignment?.Trim(),
            PositionRole = input.PositionRole?.Trim(),
            MedicalNotes = input.MedicalNotes?.Trim(),
            EmergencyContactName = input.EmergencyContactName?.Trim(),
            EmergencyContactPhone = input.EmergencyContactPhone?.Trim(),
            Notes = input.Notes?.Trim()
        };

        await _extramuralActivityRepository.UpdateAsync(activity);
        await _studentExtramuralRepository.InsertAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(enrollment.Id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Manage)]
    public async Task<StudentExtramuralDto> UpdateAsync(Guid id, UpdateStudentExtramuralDto input)
    {
        var enrollment = await _studentExtramuralRepository
            .FirstOrDefaultAsync(se => se.Id == id && se.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentExtramuralNotFound,
                "Student extramural enrollment not found.");

        if (input.TermNumber.HasValue) enrollment.TermNumber = input.TermNumber.Value;
        if (input.TeamAssignment != null) enrollment.TeamAssignment = input.TeamAssignment.Trim();
        if (input.PositionRole != null) enrollment.PositionRole = input.PositionRole.Trim();
        if (input.MedicalNotes != null) enrollment.MedicalNotes = input.MedicalNotes.Trim();
        if (input.EmergencyContactName != null) enrollment.EmergencyContactName = input.EmergencyContactName.Trim();
        if (input.EmergencyContactPhone != null) enrollment.EmergencyContactPhone = input.EmergencyContactPhone.Trim();
        if (input.AttendanceRate.HasValue) enrollment.AttendanceRate = input.AttendanceRate.Value;
        if (input.Notes != null) enrollment.Notes = input.Notes.Trim();

        await _studentExtramuralRepository.UpdateAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var enrollment = await _studentExtramuralRepository
            .FirstOrDefaultAsync(se => se.Id == id && se.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentExtramuralNotFound,
                "Student extramural enrollment not found.");

        // Remove student from activity count
        var activity = await _extramuralActivityRepository
            .FirstOrDefaultAsync(ea => ea.Id == enrollment.ExtramuralActivityId && ea.TenantId == AbpSession.TenantId);

        if (activity != null)
        {
            activity.RemoveStudent();
            await _extramuralActivityRepository.UpdateAsync(activity);
        }

        await _studentExtramuralRepository.DeleteAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Manage)]
    public async Task<StudentExtramuralDto> SuspendAsync(Guid id)
    {
        var enrollment = await _studentExtramuralRepository
            .FirstOrDefaultAsync(se => se.Id == id && se.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentExtramuralNotFound,
                "Student extramural enrollment not found.");

        if (enrollment.Status == EnrollmentStatus.Terminated)
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidEnrollmentStatusTransition,
                "Cannot suspend a terminated enrollment.");

        try
        {
            enrollment.Suspend();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidEnrollmentStatusTransition,
                "Invalid enrollment status transition.");
        }

        await _studentExtramuralRepository.UpdateAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Manage)]
    public async Task<StudentExtramuralDto> ReactivateAsync(Guid id)
    {
        var enrollment = await _studentExtramuralRepository
            .FirstOrDefaultAsync(se => se.Id == id && se.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentExtramuralNotFound,
                "Student extramural enrollment not found.");

        if (enrollment.Status == EnrollmentStatus.Terminated)
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidEnrollmentStatusTransition,
                "Cannot reactivate a terminated enrollment.");

        try
        {
            enrollment.Reactivate();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidEnrollmentStatusTransition,
                "Invalid enrollment status transition.");
        }

        await _studentExtramuralRepository.UpdateAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Manage)]
    public async Task<StudentExtramuralDto> TerminateAsync(Guid id)
    {
        var enrollment = await _studentExtramuralRepository
            .FirstOrDefaultAsync(se => se.Id == id && se.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentExtramuralNotFound,
                "Student extramural enrollment not found.");

        if (enrollment.Status == EnrollmentStatus.Terminated)
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidEnrollmentStatusTransition,
                "Enrollment is already terminated.");

        try
        {
            enrollment.Terminate();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(SASpecificExceptionCodes.InvalidEnrollmentStatusTransition,
                "Invalid enrollment status transition.");
        }

        // Remove student from activity count
        var activity = await _extramuralActivityRepository
            .FirstOrDefaultAsync(ea => ea.Id == enrollment.ExtramuralActivityId && ea.TenantId == AbpSession.TenantId);

        if (activity != null)
        {
            activity.RemoveStudent();
            await _extramuralActivityRepository.UpdateAsync(activity);
        }

        await _studentExtramuralRepository.UpdateAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_Extramurals_Manage)]
    public async Task<StudentExtramuralDto> SignConsentFormAsync(Guid id)
    {
        var enrollment = await _studentExtramuralRepository
            .FirstOrDefaultAsync(se => se.Id == id && se.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentExtramuralNotFound,
                "Student extramural enrollment not found.");

        enrollment.SignConsentForm();

        await _studentExtramuralRepository.UpdateAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
