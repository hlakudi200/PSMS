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
using psms.SASpecific.StudentAfterCares.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.SASpecific.StudentAfterCares;

/// <summary>
/// Service for managing student after-care enrollments.
/// </summary>
[AbpAuthorize(PermissionNames.SASpecific_AfterCare)]
public class StudentAfterCareAppService : ApplicationService, IStudentAfterCareAppService
{
    private readonly IRepository<StudentAfterCare, Guid> _studentAfterCareRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<AfterCare, Guid> _afterCareRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;

    public StudentAfterCareAppService(
        IRepository<StudentAfterCare, Guid> studentAfterCareRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<AfterCare, Guid> afterCareRepository,
        IRepository<AcademicYear, Guid> academicYearRepository)
    {
        _studentAfterCareRepository = studentAfterCareRepository;
        _studentRepository = studentRepository;
        _afterCareRepository = afterCareRepository;
        _academicYearRepository = academicYearRepository;
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_View)]
    public async Task<StudentAfterCareDto> GetAsync(Guid id)
    {
        var enrollment = await _studentAfterCareRepository
            .GetAll()
            .Include(sac => sac.Student)
            .Include(sac => sac.AfterCare)
            .Include(sac => sac.AcademicYear)
            .FirstOrDefaultAsync(sac => sac.Id == id && sac.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentAfterCareNotFound,
                "Student after-care enrollment not found.");

        return ObjectMapper.Map<StudentAfterCareDto>(enrollment);
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_View)]
    public async Task<PagedResultDto<StudentAfterCareListDto>> GetAllAsync(GetStudentAfterCaresInput input)
    {
        var query = _studentAfterCareRepository
            .GetAll()
            .Include(sac => sac.Student)
            .Include(sac => sac.AfterCare)
            .Include(sac => sac.AcademicYear)
            .Where(sac => sac.TenantId == AbpSession.TenantId)
            .WhereIf(input.StudentId.HasValue, sac => sac.StudentId == input.StudentId.Value)
            .WhereIf(input.AfterCareId.HasValue, sac => sac.AfterCareId == input.AfterCareId.Value)
            .WhereIf(input.AcademicYearId.HasValue, sac => sac.AcademicYearId == input.AcademicYearId.Value)
            .WhereIf(input.Status.HasValue, sac => sac.Status == input.Status.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                sac => sac.Student.FirstName.ToLower().Contains(input.Keyword.ToLower())
                    || sac.Student.LastName.ToLower().Contains(input.Keyword.ToLower())
                    || sac.AfterCare.ProgramName.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "StartDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<StudentAfterCareListDto>(
            totalCount,
            ObjectMapper.Map<List<StudentAfterCareListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_View)]
    public async Task<ListResultDto<StudentAfterCareListDto>> GetByAfterCareAsync(Guid afterCareId)
    {
        var items = await _studentAfterCareRepository
            .GetAll()
            .Include(sac => sac.Student)
            .Include(sac => sac.AfterCare)
            .Include(sac => sac.AcademicYear)
            .Where(sac => sac.TenantId == AbpSession.TenantId
                && sac.AfterCareId == afterCareId)
            .OrderBy(sac => sac.Student.LastName)
            .ToListAsync();

        return new ListResultDto<StudentAfterCareListDto>(
            ObjectMapper.Map<List<StudentAfterCareListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_View)]
    public async Task<ListResultDto<StudentAfterCareListDto>> GetByStudentAsync(Guid studentId)
    {
        var items = await _studentAfterCareRepository
            .GetAll()
            .Include(sac => sac.Student)
            .Include(sac => sac.AfterCare)
            .Include(sac => sac.AcademicYear)
            .Where(sac => sac.TenantId == AbpSession.TenantId
                && sac.StudentId == studentId)
            .OrderBy(sac => sac.StartDate)
            .ToListAsync();

        return new ListResultDto<StudentAfterCareListDto>(
            ObjectMapper.Map<List<StudentAfterCareListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_Manage)]
    public async Task<StudentAfterCareDto> CreateAsync(CreateStudentAfterCareDto input)
    {
        // Validate Student exists
        var student = await _studentRepository
            .FirstOrDefaultAsync(s => s.Id == input.StudentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentNotFound,
                "Student not found.");

        // Validate AfterCare exists and is active
        var afterCare = await _afterCareRepository
            .FirstOrDefaultAsync(ac => ac.Id == input.AfterCareId && ac.TenantId == AbpSession.TenantId);

        if (afterCare == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.AfterCareNotFound,
                "After-care program not found.");

        if (!afterCare.IsActive)
            throw new UserFriendlyException(SASpecificExceptionCodes.AfterCareInactive,
                "After-care program is not active.");

        // Validate AcademicYear exists
        var academicYear = await _academicYearRepository
            .FirstOrDefaultAsync(ay => ay.Id == input.AcademicYearId && ay.TenantId == AbpSession.TenantId);

        if (academicYear == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.AcademicYearNotFound,
                "Academic year not found.");

        // Duplicate check: (StudentId, AfterCareId, AcademicYearId)
        var duplicateExists = await _studentAfterCareRepository
            .GetAll()
            .AnyAsync(sac => sac.TenantId == AbpSession.TenantId
                && sac.StudentId == input.StudentId
                && sac.AfterCareId == input.AfterCareId
                && sac.AcademicYearId == input.AcademicYearId);

        if (duplicateExists)
            throw new UserFriendlyException(SASpecificExceptionCodes.DuplicateStudentAfterCare,
                "This student is already enrolled in this after-care program for the selected academic year.");

        // Enroll student (checks capacity)
        try
        {
            afterCare.EnrollStudent();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(SASpecificExceptionCodes.AfterCareAtCapacity,
                "After-care program is at full capacity.");
        }

        var enrollment = new StudentAfterCare(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.StudentId,
            input.AfterCareId,
            input.AcademicYearId,
            input.StartDate)
        {
            DaysEnrolled = input.DaysEnrolled,
            DietaryRequirements = input.DietaryRequirements?.Trim(),
            MedicalNotes = input.MedicalNotes?.Trim(),
            AuthorizedPickupPersons = input.AuthorizedPickupPersons,
            UsualPickupTime = input.UsualPickupTime,
            Notes = input.Notes?.Trim()
        };

        await _afterCareRepository.UpdateAsync(afterCare);
        await _studentAfterCareRepository.InsertAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(enrollment.Id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_Manage)]
    public async Task<StudentAfterCareDto> UpdateAsync(Guid id, UpdateStudentAfterCareDto input)
    {
        var enrollment = await _studentAfterCareRepository
            .FirstOrDefaultAsync(sac => sac.Id == id && sac.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentAfterCareNotFound,
                "Student after-care enrollment not found.");

        if (input.DaysEnrolled != null) enrollment.DaysEnrolled = input.DaysEnrolled;
        if (input.DietaryRequirements != null) enrollment.DietaryRequirements = input.DietaryRequirements.Trim();
        if (input.MedicalNotes != null) enrollment.MedicalNotes = input.MedicalNotes.Trim();
        if (input.AuthorizedPickupPersons != null) enrollment.AuthorizedPickupPersons = input.AuthorizedPickupPersons;
        if (input.UsualPickupTime.HasValue) enrollment.UsualPickupTime = input.UsualPickupTime.Value;
        if (input.Notes != null) enrollment.Notes = input.Notes.Trim();

        await _studentAfterCareRepository.UpdateAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var enrollment = await _studentAfterCareRepository
            .FirstOrDefaultAsync(sac => sac.Id == id && sac.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentAfterCareNotFound,
                "Student after-care enrollment not found.");

        // Remove student from after-care count
        var afterCare = await _afterCareRepository
            .FirstOrDefaultAsync(ac => ac.Id == enrollment.AfterCareId && ac.TenantId == AbpSession.TenantId);

        if (afterCare != null)
        {
            afterCare.RemoveStudent();
            await _afterCareRepository.UpdateAsync(afterCare);
        }

        await _studentAfterCareRepository.DeleteAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_Manage)]
    public async Task<StudentAfterCareDto> SuspendAsync(Guid id)
    {
        var enrollment = await _studentAfterCareRepository
            .FirstOrDefaultAsync(sac => sac.Id == id && sac.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentAfterCareNotFound,
                "Student after-care enrollment not found.");

        if (enrollment.Status == EnrollmentStatus.Terminated)
            throw new UserFriendlyException(SASpecificExceptionCodes.EnrollmentAlreadyTerminated,
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

        await _studentAfterCareRepository.UpdateAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_Manage)]
    public async Task<StudentAfterCareDto> ReactivateAsync(Guid id)
    {
        var enrollment = await _studentAfterCareRepository
            .FirstOrDefaultAsync(sac => sac.Id == id && sac.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentAfterCareNotFound,
                "Student after-care enrollment not found.");

        if (enrollment.Status == EnrollmentStatus.Terminated)
            throw new UserFriendlyException(SASpecificExceptionCodes.EnrollmentAlreadyTerminated,
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

        await _studentAfterCareRepository.UpdateAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.SASpecific_AfterCare_Manage)]
    public async Task<StudentAfterCareDto> TerminateAsync(Guid id)
    {
        var enrollment = await _studentAfterCareRepository
            .FirstOrDefaultAsync(sac => sac.Id == id && sac.TenantId == AbpSession.TenantId);

        if (enrollment == null)
            throw new UserFriendlyException(SASpecificExceptionCodes.StudentAfterCareNotFound,
                "Student after-care enrollment not found.");

        if (enrollment.Status == EnrollmentStatus.Terminated)
            throw new UserFriendlyException(SASpecificExceptionCodes.EnrollmentAlreadyTerminated,
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

        // Remove student from after-care count
        var afterCare = await _afterCareRepository
            .FirstOrDefaultAsync(ac => ac.Id == enrollment.AfterCareId && ac.TenantId == AbpSession.TenantId);

        if (afterCare != null)
        {
            afterCare.RemoveStudent();
            await _afterCareRepository.UpdateAsync(afterCare);
        }

        await _studentAfterCareRepository.UpdateAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
