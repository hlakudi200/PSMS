using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Classes.Dto;
using psms.Academic.Shared;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Academic.Classes;

/// <summary>
/// Service for managing classes/sections within grades.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Classes)]
public class ClassAppService : ApplicationService, IClassAppService
{
    private readonly IRepository<Class, Guid> _classRepository;
    private readonly IRepository<Grade, Guid> _gradeRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;
    private readonly IRepository<Teacher, Guid> _teacherRepository;

    public ClassAppService(
        IRepository<Class, Guid> classRepository,
        IRepository<Grade, Guid> gradeRepository,
        IRepository<AcademicYear, Guid> academicYearRepository,
        IRepository<Teacher, Guid> teacherRepository)
    {
        _classRepository = classRepository;
        _gradeRepository = gradeRepository;
        _academicYearRepository = academicYearRepository;
        _teacherRepository = teacherRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Classes_View)]
    public async Task<ClassDto> GetAsync(Guid id)
    {
        var cls = await _classRepository
            .GetAll()
            .Include(c => c.Grade)
            .Include(c => c.AcademicYear)
            .Include(c => c.ClassTeacher)
            .Include(c => c.Students)
            .Include(c => c.TeacherAssignments)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == AbpSession.TenantId);

        if (cls == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotFound, "Class not found.");

        return ObjectMapper.Map<ClassDto>(cls);
    }

    [AbpAuthorize(PermissionNames.Academic_Classes_View)]
    public async Task<PagedResultDto<ClassListDto>> GetAllAsync(GetAcademicEntityInput input)
    {
        var query = _classRepository
            .GetAll()
            .Include(c => c.Grade)
            .Include(c => c.AcademicYear)
            .Include(c => c.ClassTeacher)
            .Include(c => c.Students)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                c => c.ClassName.ToLower().Contains(input.Keyword.ToLower())
                  || c.Grade.GradeName.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var classes = await query
            .OrderBy(input.Sorting ?? "ClassName ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<ClassListDto>(
            totalCount,
            ObjectMapper.Map<List<ClassListDto>>(classes));
    }

    [AbpAuthorize(PermissionNames.Academic_Classes_Create)]
    public async Task<ClassDto> CreateAsync(CreateClassDto input)
    {
        // Validate grade exists and is active
        var grade = await _gradeRepository.FirstOrDefaultAsync(g => g.Id == input.GradeId);
        if (grade == null)
            throw new UserFriendlyException(AcademicExceptionCodes.GradeNotFound, "Grade not found.");
        if (!grade.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.GradeNotActive, "Grade is not active.");

        // Validate academic year exists
        var academicYear = await _academicYearRepository.FirstOrDefaultAsync(ay => ay.Id == input.AcademicYearId);
        if (academicYear == null)
            throw new UserFriendlyException(AcademicExceptionCodes.AcademicYearNotFound, "Academic year not found.");

        // Validate unique class name within grade and academic year
        var existingByName = await _classRepository
            .FirstOrDefaultAsync(c => c.GradeId == input.GradeId
                && c.AcademicYearId == input.AcademicYearId
                && c.ClassName == input.ClassName);

        if (existingByName != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateClassName,
                $"A class with name '{input.ClassName}' already exists in this grade for this academic year.");

        // Validate class teacher if provided
        if (input.ClassTeacherId.HasValue)
        {
            var teacher = await _teacherRepository.FirstOrDefaultAsync(t => t.Id == input.ClassTeacherId.Value);
            if (teacher == null)
                throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Class teacher not found.");
            if (!teacher.IsActive)
                throw new UserFriendlyException(AcademicExceptionCodes.ClassTeacherNotActive, "Class teacher is not active.");
        }

        var cls = new Class(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.ClassName,
            input.GradeId,
            input.AcademicYearId,
            input.MaxCapacity)
        {
            ClassTeacherId = input.ClassTeacherId
        };

        await _classRepository.InsertAsync(cls);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(cls.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Classes_Edit)]
    public async Task<ClassDto> UpdateAsync(Guid id, UpdateClassDto input)
    {
        var cls = await _classRepository.GetAsync(id);

        // Validate unique class name if changing (scoped to grade + academic year)
        if (input.ClassName != null && input.ClassName != cls.ClassName)
        {
            var existingByName = await _classRepository
                .FirstOrDefaultAsync(c => c.GradeId == cls.GradeId
                    && c.AcademicYearId == cls.AcademicYearId
                    && c.ClassName == input.ClassName
                    && c.Id != id);

            if (existingByName != null)
                throw new UserFriendlyException(AcademicExceptionCodes.DuplicateClassName,
                    $"A class with name '{input.ClassName}' already exists in this grade for this academic year.");

            cls.ClassName = input.ClassName;
        }

        if (input.MaxCapacity.HasValue)
        {
            // Validate capacity is not less than current student count
            var studentCount = await _classRepository
                .GetAll()
                .Where(c => c.Id == id)
                .SelectMany(c => c.Students)
                .CountAsync(s => !s.IsDeleted);

            if (input.MaxCapacity.Value < studentCount)
                throw new UserFriendlyException(AcademicExceptionCodes.ClassCapacityTooLow,
                    $"Cannot set capacity to {input.MaxCapacity.Value}. There are currently {studentCount} students enrolled.");

            cls.MaxCapacity = input.MaxCapacity.Value;
        }

        if (input.ClassTeacherId.HasValue)
        {
            var teacher = await _teacherRepository.FirstOrDefaultAsync(t => t.Id == input.ClassTeacherId.Value);
            if (teacher == null)
                throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Class teacher not found.");
            if (!teacher.IsActive)
                throw new UserFriendlyException(AcademicExceptionCodes.ClassTeacherNotActive, "Class teacher is not active.");

            cls.ClassTeacherId = input.ClassTeacherId.Value;
        }

        if (input.IsActive.HasValue) cls.IsActive = input.IsActive.Value;

        await _classRepository.UpdateAsync(cls);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Classes_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var cls = await _classRepository
            .GetAll()
            .Include(c => c.Students)
            .Include(c => c.TeacherAssignments)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == AbpSession.TenantId);

        if (cls == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotFound, "Class not found.");

        if (cls.Students.Any(s => !s.IsDeleted))
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteClassWithStudents,
                "Cannot delete a class that has enrolled students. Remove students first.");

        if (cls.TeacherAssignments.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteClassWithTeachers,
                "Cannot delete a class that has teacher assignments. Remove assignments first.");

        await _classRepository.DeleteAsync(cls);
    }

    [AbpAuthorize(PermissionNames.Academic_Classes_View)]
    public async Task<ListResultDto<ClassListDto>> GetActiveClassesAsync()
    {
        var classes = await _classRepository
            .GetAll()
            .Include(c => c.Grade)
            .Include(c => c.AcademicYear)
            .Include(c => c.ClassTeacher)
            .Include(c => c.Students)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Grade.GradeLevel)
            .ThenBy(c => c.ClassName)
            .ToListAsync();

        return new ListResultDto<ClassListDto>(
            ObjectMapper.Map<List<ClassListDto>>(classes));
    }

    [AbpAuthorize(PermissionNames.Academic_Classes_View)]
    public async Task<ListResultDto<ClassListDto>> GetClassesByGradeAsync(Guid gradeId)
    {
        var classes = await _classRepository
            .GetAll()
            .Include(c => c.Grade)
            .Include(c => c.AcademicYear)
            .Include(c => c.ClassTeacher)
            .Include(c => c.Students)
            .Where(c => c.GradeId == gradeId)
            .OrderBy(c => c.ClassName)
            .ToListAsync();

        return new ListResultDto<ClassListDto>(
            ObjectMapper.Map<List<ClassListDto>>(classes));
    }

    [AbpAuthorize(PermissionNames.Academic_Classes_View)]
    public async Task<ListResultDto<ClassListDto>> GetClassesByAcademicYearAsync(Guid academicYearId)
    {
        var classes = await _classRepository
            .GetAll()
            .Include(c => c.Grade)
            .Include(c => c.AcademicYear)
            .Include(c => c.ClassTeacher)
            .Include(c => c.Students)
            .Where(c => c.AcademicYearId == academicYearId)
            .OrderBy(c => c.Grade.GradeLevel)
            .ThenBy(c => c.ClassName)
            .ToListAsync();

        return new ListResultDto<ClassListDto>(
            ObjectMapper.Map<List<ClassListDto>>(classes));
    }

    [AbpAuthorize(PermissionNames.Academic_Classes_Edit)]
    public async Task<ClassDto> AssignClassTeacherAsync(Guid id, Guid teacherId)
    {
        var cls = await _classRepository.GetAsync(id);

        var teacher = await _teacherRepository.FirstOrDefaultAsync(t => t.Id == teacherId);
        if (teacher == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Teacher not found.");
        if (!teacher.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassTeacherNotActive, "Teacher is not active.");

        cls.ClassTeacherId = teacherId;
        await _classRepository.UpdateAsync(cls);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Classes_Edit)]
    public async Task<ClassDto> RemoveClassTeacherAsync(Guid id)
    {
        var cls = await _classRepository.GetAsync(id);

        cls.ClassTeacherId = null;
        await _classRepository.UpdateAsync(cls);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Classes_Edit)]
    public async Task ActivateAsync(Guid id)
    {
        var cls = await _classRepository.GetAsync(id);
        cls.IsActive = true;
        await _classRepository.UpdateAsync(cls);
    }

    [AbpAuthorize(PermissionNames.Academic_Classes_Edit)]
    public async Task DeactivateAsync(Guid id)
    {
        var cls = await _classRepository.GetAsync(id);
        cls.IsActive = false;
        await _classRepository.UpdateAsync(cls);
    }
}
