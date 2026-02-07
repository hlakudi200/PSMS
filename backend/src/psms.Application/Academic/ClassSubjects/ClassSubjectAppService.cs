using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.ClassSubjects.Dto;
using psms.Academic.Shared;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AssessmentEntity = psms.Domain.Assessment.Entities.Assessment;

namespace psms.Academic.ClassSubjects;

/// <summary>
/// Service for managing class-subject assignments.
/// Links classes to subjects with optional teacher assignment.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_ClassSubjects)]
public class ClassSubjectAppService : ApplicationService, IClassSubjectAppService
{
    private readonly IRepository<ClassSubject, Guid> _classSubjectRepository;
    private readonly IRepository<Class, Guid> _classRepository;
    private readonly IRepository<Subject, Guid> _subjectRepository;
    private readonly IRepository<Teacher, Guid> _teacherRepository;
    private readonly IRepository<AssessmentEntity, Guid> _assessmentRepository;

    public ClassSubjectAppService(
        IRepository<ClassSubject, Guid> classSubjectRepository,
        IRepository<Class, Guid> classRepository,
        IRepository<Subject, Guid> subjectRepository,
        IRepository<Teacher, Guid> teacherRepository,
        IRepository<AssessmentEntity, Guid> assessmentRepository)
    {
        _classSubjectRepository = classSubjectRepository;
        _classRepository = classRepository;
        _subjectRepository = subjectRepository;
        _teacherRepository = teacherRepository;
        _assessmentRepository = assessmentRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_ClassSubjects_View)]
    public async Task<ClassSubjectDto> GetAsync(Guid id)
    {
        var classSubject = await _classSubjectRepository
            .GetAll()
            .Include(cs => cs.Class).ThenInclude(c => c.Grade)
            .Include(cs => cs.Subject)
            .Include(cs => cs.Teacher)
            .FirstOrDefaultAsync(cs => cs.Id == id && cs.TenantId == AbpSession.TenantId);

        if (classSubject == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassSubjectNotFound,
                "Class-subject assignment not found.");

        return ObjectMapper.Map<ClassSubjectDto>(classSubject);
    }

    [AbpAuthorize(PermissionNames.Academic_ClassSubjects_View)]
    public async Task<PagedResultDto<ClassSubjectListDto>> GetAllAsync(GetClassSubjectsInput input)
    {
        var query = _classSubjectRepository
            .GetAll()
            .Include(cs => cs.Class)
            .Include(cs => cs.Subject)
            .Include(cs => cs.Teacher)
            .Where(cs => cs.TenantId == AbpSession.TenantId)
            .WhereIf(input.ClassId.HasValue, cs => cs.ClassId == input.ClassId.Value)
            .WhereIf(input.SubjectId.HasValue, cs => cs.SubjectId == input.SubjectId.Value)
            .WhereIf(input.TeacherId.HasValue, cs => cs.TeacherId == input.TeacherId.Value)
            .WhereIf(input.IsActive.HasValue, cs => cs.IsActive == input.IsActive.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "Class.ClassName ASC, Subject.SubjectName ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<ClassSubjectListDto>(
            totalCount,
            ObjectMapper.Map<List<ClassSubjectListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Academic_ClassSubjects_View)]
    public async Task<ListResultDto<ClassSubjectListDto>> GetByClassAsync(Guid classId)
    {
        var items = await _classSubjectRepository
            .GetAll()
            .Include(cs => cs.Class)
            .Include(cs => cs.Subject)
            .Include(cs => cs.Teacher)
            .Where(cs => cs.TenantId == AbpSession.TenantId)
            .Where(cs => cs.ClassId == classId)
            .OrderBy(cs => cs.Subject.SubjectName)
            .ToListAsync();

        return new ListResultDto<ClassSubjectListDto>(
            ObjectMapper.Map<List<ClassSubjectListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Academic_ClassSubjects_View)]
    public async Task<ListResultDto<ClassSubjectListDto>> GetByTeacherAsync(Guid teacherId)
    {
        var items = await _classSubjectRepository
            .GetAll()
            .Include(cs => cs.Class)
            .Include(cs => cs.Subject)
            .Include(cs => cs.Teacher)
            .Where(cs => cs.TenantId == AbpSession.TenantId)
            .Where(cs => cs.TeacherId == teacherId)
            .OrderBy(cs => cs.Class.ClassName)
            .ThenBy(cs => cs.Subject.SubjectName)
            .ToListAsync();

        return new ListResultDto<ClassSubjectListDto>(
            ObjectMapper.Map<List<ClassSubjectListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Academic_ClassSubjects_Manage)]
    public async Task<ClassSubjectDto> CreateAsync(CreateClassSubjectDto input)
    {
        // Validate class exists and is active
        var cls = await _classRepository.FirstOrDefaultAsync(c => c.Id == input.ClassId);
        if (cls == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotFound, "Class not found.");
        if (!cls.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotActive, "Class is not active.");

        // Validate subject exists and is active
        var subject = await _subjectRepository.FirstOrDefaultAsync(s => s.Id == input.SubjectId);
        if (subject == null)
            throw new UserFriendlyException(AcademicExceptionCodes.SubjectNotFound, "Subject not found.");
        if (!subject.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.SubjectNotActive, "Subject is not active.");

        // Validate teacher exists and is active (if provided)
        if (input.TeacherId.HasValue)
        {
            var teacher = await _teacherRepository.FirstOrDefaultAsync(t => t.Id == input.TeacherId.Value);
            if (teacher == null)
                throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Teacher not found.");
            if (!teacher.IsActive)
                throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotActive, "Teacher is not active.");
        }

        // Check for duplicate (ClassId + SubjectId) per tenant
        var existing = await _classSubjectRepository
            .GetAll()
            .Where(cs => cs.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(cs => cs.ClassId == input.ClassId && cs.SubjectId == input.SubjectId);

        if (existing != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateClassSubject,
                $"Subject '{subject.SubjectName}' is already assigned to class '{cls.ClassName}'.");

        var classSubject = new ClassSubject(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.ClassId,
            input.SubjectId,
            input.TeacherId)
        {
            PeriodsPerWeek = input.PeriodsPerWeek
        };

        await _classSubjectRepository.InsertAsync(classSubject);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(classSubject.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_ClassSubjects_Manage)]
    public async Task<ClassSubjectDto> UpdateAsync(Guid id, UpdateClassSubjectDto input)
    {
        var classSubject = await _classSubjectRepository
            .FirstOrDefaultAsync(cs => cs.Id == id && cs.TenantId == AbpSession.TenantId);

        if (classSubject == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassSubjectNotFound,
                "Class-subject assignment not found.");

        // Validate teacher if changing
        if (input.TeacherId.HasValue)
        {
            var teacher = await _teacherRepository.FirstOrDefaultAsync(t => t.Id == input.TeacherId.Value);
            if (teacher == null)
                throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Teacher not found.");
            if (!teacher.IsActive)
                throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotActive, "Teacher is not active.");

            classSubject.TeacherId = input.TeacherId.Value;
        }

        if (input.PeriodsPerWeek.HasValue)
            classSubject.PeriodsPerWeek = input.PeriodsPerWeek.Value;

        if (input.IsActive.HasValue)
        {
            if (input.IsActive.Value)
                classSubject.Activate();
            else
                classSubject.Deactivate();
        }

        await _classSubjectRepository.UpdateAsync(classSubject);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_ClassSubjects_Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var classSubject = await _classSubjectRepository
            .FirstOrDefaultAsync(cs => cs.Id == id && cs.TenantId == AbpSession.TenantId);

        if (classSubject == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassSubjectNotFound,
                "Class-subject assignment not found.");

        // Cannot delete if assessments reference this ClassSubject
        var hasAssessments = await _assessmentRepository
            .GetAll()
            .AnyAsync(a => a.ClassSubjectId == id);

        if (hasAssessments)
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteClassSubjectWithAssessments,
                "Cannot delete a class-subject assignment that has assessments. Remove assessments first.");

        await _classSubjectRepository.DeleteAsync(classSubject);
    }

    [AbpAuthorize(PermissionNames.Academic_ClassSubjects_Manage)]
    public async Task<ClassSubjectDto> AssignTeacherAsync(Guid id, Guid teacherId)
    {
        var classSubject = await _classSubjectRepository
            .FirstOrDefaultAsync(cs => cs.Id == id && cs.TenantId == AbpSession.TenantId);

        if (classSubject == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassSubjectNotFound,
                "Class-subject assignment not found.");

        if (!classSubject.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassSubjectNotActive,
                "Cannot assign a teacher to an inactive class-subject assignment.");

        var teacher = await _teacherRepository.FirstOrDefaultAsync(t => t.Id == teacherId);
        if (teacher == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Teacher not found.");
        if (!teacher.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotActive, "Teacher is not active.");

        classSubject.AssignTeacher(teacherId);
        await _classSubjectRepository.UpdateAsync(classSubject);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_ClassSubjects_Manage)]
    public async Task<ClassSubjectDto> RemoveTeacherAsync(Guid id)
    {
        var classSubject = await _classSubjectRepository
            .FirstOrDefaultAsync(cs => cs.Id == id && cs.TenantId == AbpSession.TenantId);

        if (classSubject == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassSubjectNotFound,
                "Class-subject assignment not found.");

        classSubject.RemoveTeacher();
        await _classSubjectRepository.UpdateAsync(classSubject);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
