using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.Teachers.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using psms.Infrastructure.Querying;

namespace psms.Academic.Teachers;

/// <summary>
/// Service for managing teachers.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Teachers)]
public class TeacherAppService : ApplicationService, ITeacherAppService
{
    /// <summary>
    /// The teacher list's sortable columns.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> TeacherSortMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["fullName"] = "LastName, FirstName",
            ["firstName"] = "FirstName",
            ["lastName"] = "LastName",
            ["employeeNumber"] = "EmployeeNumber",
            ["email"] = "Email",
            ["dateOfJoining"] = "DateOfJoining",
            ["employmentStatus"] = "EmploymentStatus",
            ["isActive"] = "IsActive",
        };

    private readonly IRepository<Teacher, Guid> _teacherRepository;

    public TeacherAppService(IRepository<Teacher, Guid> teacherRepository)
    {
        _teacherRepository = teacherRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_View)]
    public async Task<TeacherDto> GetAsync(Guid id)
    {
        var teacher = await _teacherRepository
            .GetAll()
            .Include(t => t.SubjectAssignments)
            .Include(t => t.ClassAssignments)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (teacher == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Teacher not found.");

        return ObjectMapper.Map<TeacherDto>(teacher);
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_View)]
    public async Task<TeacherDto> GetByCurrentUserAsync()
    {
        // No active user (e.g. host context) — return null so the caller can
        // surface a friendly "no teacher profile" message instead of an error
        // modal from the global axios interceptor.
        if (AbpSession.UserId == null)
            return null;

        var teacher = await _teacherRepository
            .GetAll()
            .Include(t => t.SubjectAssignments)
            .Include(t => t.ClassAssignments)
            .FirstOrDefaultAsync(t => t.UserId == AbpSession.UserId.Value
                                   && t.TenantId == AbpSession.TenantId);

        return teacher == null ? null : ObjectMapper.Map<TeacherDto>(teacher);
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_View)]
    public async Task<PagedResultDto<TeacherListDto>> GetAllAsync(GetAcademicEntityInput input)
    {
        var query = _teacherRepository
            .GetAll()
            .Include(t => t.SubjectAssignments)
            .Include(t => t.ClassAssignments)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                t => t.FirstName.ToLower().Contains(input.Keyword.ToLower())
                  || t.LastName.ToLower().Contains(input.Keyword.ToLower())
                  || t.EmployeeNumber.ToLower().Contains(input.Keyword.ToLower())
                  || t.Email.ToLower().Contains(input.Keyword.ToLower()))
            .WhereIf(input.IsActive.HasValue,
                t => t.IsActive == input.IsActive.Value);

        var totalCount = await query.CountAsync();

        var teachers = await query
            .ApplySorting(input.Sorting, "LastName ASC", TeacherSortMap)
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<TeacherListDto>(
            totalCount,
            ObjectMapper.Map<List<TeacherListDto>>(teachers));
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_Create)]
    public async Task<TeacherDto> CreateAsync(CreateTeacherDto input)
    {
        // Validate unique employee number
        var existingByNumber = await _teacherRepository
            .FirstOrDefaultAsync(t => t.EmployeeNumber == input.EmployeeNumber);

        if (existingByNumber != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateEmployeeNumber,
                $"A teacher with employee number '{input.EmployeeNumber}' already exists.");

        // Validate unique email (case-insensitive)
        var normalizedEmail = input.Email.Trim().ToLowerInvariant();
        var existingByEmail = await _teacherRepository
            .FirstOrDefaultAsync(t => t.Email.ToLower() == normalizedEmail);

        if (existingByEmail != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateTeacherEmail,
                $"A teacher with email '{input.Email}' already exists.");

        var teacher = new Teacher(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.UserId,
            input.FirstName,
            input.LastName,
            input.EmployeeNumber,
            normalizedEmail)
        {
            MiddleName = input.MiddleName,
            Phone = input.Phone,
            Qualifications = input.Qualifications,
            QualifiedSubjects = input.QualifiedSubjects,
            EmploymentStatus = input.EmploymentStatus
        };

        if (input.DateOfJoining.HasValue)
            teacher.DateOfJoining = input.DateOfJoining.Value;

        if (input.Address != null)
        {
            teacher.Address = new Address(
                input.Address.StreetAddress,
                input.Address.Suburb,
                input.Address.City,
                input.Address.Province,
                input.Address.PostalCode,
                input.Address.Country);
        }

        await _teacherRepository.InsertAsync(teacher);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(teacher.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_Edit)]
    public async Task<TeacherDto> UpdateAsync(Guid id, UpdateTeacherDto input)
    {
        var teacher = await _teacherRepository.GetAsync(id);

        // Validate unique email if changing (case-insensitive)
        if (input.Email != null)
        {
            var normalizedEmail = input.Email.Trim().ToLowerInvariant();
            if (normalizedEmail != teacher.Email.ToLowerInvariant())
            {
                var existingByEmail = await _teacherRepository
                    .FirstOrDefaultAsync(t => t.Email.ToLower() == normalizedEmail && t.Id != id);

                if (existingByEmail != null)
                    throw new UserFriendlyException(AcademicExceptionCodes.DuplicateTeacherEmail,
                        $"A teacher with email '{input.Email}' already exists.");
            }

            teacher.Email = normalizedEmail;
        }

        if (input.FirstName != null) teacher.FirstName = input.FirstName;
        if (input.LastName != null) teacher.LastName = input.LastName;
        if (input.MiddleName != null) teacher.MiddleName = input.MiddleName;
        if (input.Phone != null) teacher.Phone = input.Phone;
        if (input.DateOfJoining.HasValue) teacher.DateOfJoining = input.DateOfJoining.Value;
        if (input.Qualifications != null) teacher.Qualifications = input.Qualifications;
        if (input.QualifiedSubjects != null) teacher.QualifiedSubjects = input.QualifiedSubjects;
        if (input.EmploymentStatus != null) teacher.EmploymentStatus = input.EmploymentStatus;
        if (input.ProfilePhotoUrl != null) teacher.ProfilePhotoUrl = input.ProfilePhotoUrl;
        if (input.IsActive.HasValue) teacher.IsActive = input.IsActive.Value;

        if (input.Address != null)
        {
            teacher.Address = new Address(
                input.Address.StreetAddress,
                input.Address.Suburb,
                input.Address.City,
                input.Address.Province,
                input.Address.PostalCode,
                input.Address.Country);
        }

        await _teacherRepository.UpdateAsync(teacher);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var teacher = await _teacherRepository
            .GetAll()
            .Include(t => t.SubjectAssignments)
            .Include(t => t.ClassAssignments)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (teacher == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Teacher not found.");

        if (teacher.SubjectAssignments.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteTeacherWithSubjects,
                "Cannot delete a teacher that has subject assignments. Remove assignments first.");

        if (teacher.ClassAssignments.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteTeacherWithClasses,
                "Cannot delete a teacher that has class assignments. Remove assignments first.");

        await _teacherRepository.DeleteAsync(teacher);
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_View)]
    public async Task<ListResultDto<TeacherListDto>> GetActiveTeachersAsync()
    {
        var teachers = await _teacherRepository
            .GetAll()
            .Include(t => t.SubjectAssignments)
            .Include(t => t.ClassAssignments)
            .Where(t => t.IsActive)
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToListAsync();

        return new ListResultDto<TeacherListDto>(
            ObjectMapper.Map<List<TeacherListDto>>(teachers));
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_Edit)]
    public async Task ActivateAsync(Guid id)
    {
        var teacher = await _teacherRepository.GetAsync(id);
        teacher.IsActive = true;
        await _teacherRepository.UpdateAsync(teacher);
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_Edit)]
    public async Task DeactivateAsync(Guid id)
    {
        var teacher = await _teacherRepository.GetAsync(id);
        teacher.IsActive = false;
        await _teacherRepository.UpdateAsync(teacher);
    }
}
