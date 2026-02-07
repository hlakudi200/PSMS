using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.TeacherClasses.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Academic.TeacherClasses;

/// <summary>
/// Service for managing teacher-class assignments (which teacher teaches which subject to which class).
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Teachers)]
public class TeacherClassAppService : ApplicationService, ITeacherClassAppService
{
    private readonly IRepository<TeacherClass, Guid> _teacherClassRepository;
    private readonly IRepository<Teacher, Guid> _teacherRepository;
    private readonly IRepository<Class, Guid> _classRepository;
    private readonly IRepository<Subject, Guid> _subjectRepository;

    public TeacherClassAppService(
        IRepository<TeacherClass, Guid> teacherClassRepository,
        IRepository<Teacher, Guid> teacherRepository,
        IRepository<Class, Guid> classRepository,
        IRepository<Subject, Guid> subjectRepository)
    {
        _teacherClassRepository = teacherClassRepository;
        _teacherRepository = teacherRepository;
        _classRepository = classRepository;
        _subjectRepository = subjectRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_View)]
    public async Task<ListResultDto<TeacherClassDto>> GetByTeacherAsync(Guid teacherId)
    {
        var assignments = await _teacherClassRepository
            .GetAll()
            .Include(tc => tc.Teacher)
            .Include(tc => tc.Class)
            .Include(tc => tc.Subject)
            .Where(tc => tc.Teacher.TenantId == AbpSession.TenantId)
            .Where(tc => tc.TeacherId == teacherId)
            .OrderBy(tc => tc.Class.ClassName)
            .ThenBy(tc => tc.Subject.SubjectName)
            .ToListAsync();

        return new ListResultDto<TeacherClassDto>(
            ObjectMapper.Map<List<TeacherClassDto>>(assignments));
    }

    [AbpAuthorize(PermissionNames.Academic_Classes_View)]
    public async Task<ListResultDto<TeacherClassDto>> GetByClassAsync(Guid classId)
    {
        var assignments = await _teacherClassRepository
            .GetAll()
            .Include(tc => tc.Teacher)
            .Include(tc => tc.Class)
            .Include(tc => tc.Subject)
            .Where(tc => tc.Class.TenantId == AbpSession.TenantId)
            .Where(tc => tc.ClassId == classId)
            .OrderBy(tc => tc.Subject.SubjectName)
            .ThenBy(tc => tc.Teacher.LastName)
            .ToListAsync();

        return new ListResultDto<TeacherClassDto>(
            ObjectMapper.Map<List<TeacherClassDto>>(assignments));
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_Edit)]
    public async Task<TeacherClassDto> AssignAsync(AssignTeacherClassDto input)
    {
        // Validate teacher exists and is active
        var teacher = await _teacherRepository.FirstOrDefaultAsync(t => t.Id == input.TeacherId);
        if (teacher == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Teacher not found.");
        if (!teacher.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotActive, "Teacher is not active.");

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

        // Check for duplicate assignment (tenant-scoped through validated FKs)
        var existing = await _teacherClassRepository
            .GetAll()
            .Where(tc => tc.Class.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(tc => tc.TeacherId == input.TeacherId
                && tc.ClassId == input.ClassId
                && tc.SubjectId == input.SubjectId);

        if (existing != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateTeacherClass,
                "This teacher is already assigned to teach this subject in this class.");

        var assignment = new TeacherClass(
            Guid.NewGuid(),
            input.TeacherId,
            input.ClassId,
            input.SubjectId)
        {
            IsClassTeacher = input.IsClassTeacher
        };

        await _teacherClassRepository.InsertAsync(assignment);
        await CurrentUnitOfWork.SaveChangesAsync();

        var saved = await _teacherClassRepository
            .GetAll()
            .Include(tc => tc.Teacher)
            .Include(tc => tc.Class)
            .Include(tc => tc.Subject)
            .Where(tc => tc.Class.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(tc => tc.Id == assignment.Id);

        return ObjectMapper.Map<TeacherClassDto>(saved);
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_Edit)]
    public async Task UnassignAsync(Guid id)
    {
        var assignment = await _teacherClassRepository
            .GetAll()
            .Include(tc => tc.Class)
            .Where(tc => tc.Class.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(tc => tc.Id == id);

        if (assignment == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherClassNotFound,
                "Teacher-class assignment not found.");

        await _teacherClassRepository.DeleteAsync(assignment);
    }
}
