using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.TeacherSubjects.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Academic.TeacherSubjects;

/// <summary>
/// Service for managing teacher-subject assignments per grade.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Teachers)]
public class TeacherSubjectAppService : ApplicationService, ITeacherSubjectAppService
{
    private readonly IRepository<TeacherSubject, Guid> _teacherSubjectRepository;
    private readonly IRepository<Teacher, Guid> _teacherRepository;
    private readonly IRepository<Subject, Guid> _subjectRepository;
    private readonly IRepository<Grade, Guid> _gradeRepository;

    public TeacherSubjectAppService(
        IRepository<TeacherSubject, Guid> teacherSubjectRepository,
        IRepository<Teacher, Guid> teacherRepository,
        IRepository<Subject, Guid> subjectRepository,
        IRepository<Grade, Guid> gradeRepository)
    {
        _teacherSubjectRepository = teacherSubjectRepository;
        _teacherRepository = teacherRepository;
        _subjectRepository = subjectRepository;
        _gradeRepository = gradeRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_View)]
    public async Task<ListResultDto<TeacherSubjectDto>> GetByTeacherAsync(Guid teacherId)
    {
        var assignments = await _teacherSubjectRepository
            .GetAll()
            .Include(ts => ts.Teacher)
            .Include(ts => ts.Subject)
            .Include(ts => ts.Grade)
            .Where(ts => ts.Teacher.TenantId == AbpSession.TenantId)
            .Where(ts => ts.TeacherId == teacherId)
            .OrderBy(ts => ts.Grade.GradeLevel)
            .ThenBy(ts => ts.Subject.SubjectName)
            .ToListAsync();

        return new ListResultDto<TeacherSubjectDto>(
            ObjectMapper.Map<List<TeacherSubjectDto>>(assignments));
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_View)]
    public async Task<ListResultDto<TeacherSubjectDto>> GetBySubjectAsync(Guid subjectId)
    {
        var assignments = await _teacherSubjectRepository
            .GetAll()
            .Include(ts => ts.Teacher)
            .Include(ts => ts.Subject)
            .Include(ts => ts.Grade)
            .Where(ts => ts.Teacher.TenantId == AbpSession.TenantId)
            .Where(ts => ts.SubjectId == subjectId)
            .OrderBy(ts => ts.Grade.GradeLevel)
            .ThenBy(ts => ts.Teacher.LastName)
            .ToListAsync();

        return new ListResultDto<TeacherSubjectDto>(
            ObjectMapper.Map<List<TeacherSubjectDto>>(assignments));
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_View)]
    public async Task<ListResultDto<TeacherSubjectDto>> GetByGradeAsync(Guid gradeId)
    {
        var assignments = await _teacherSubjectRepository
            .GetAll()
            .Include(ts => ts.Teacher)
            .Include(ts => ts.Subject)
            .Include(ts => ts.Grade)
            .Where(ts => ts.Grade.TenantId == AbpSession.TenantId)
            .Where(ts => ts.GradeId == gradeId)
            .OrderBy(ts => ts.Subject.SubjectName)
            .ThenBy(ts => ts.Teacher.LastName)
            .ToListAsync();

        return new ListResultDto<TeacherSubjectDto>(
            ObjectMapper.Map<List<TeacherSubjectDto>>(assignments));
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_Edit)]
    public async Task<TeacherSubjectDto> AssignAsync(AssignTeacherSubjectDto input)
    {
        // Validate teacher exists and is active
        var teacher = await _teacherRepository.FirstOrDefaultAsync(t => t.Id == input.TeacherId);
        if (teacher == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Teacher not found.");
        if (!teacher.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotActive, "Teacher is not active.");

        // Validate subject exists and is active
        var subject = await _subjectRepository.FirstOrDefaultAsync(s => s.Id == input.SubjectId);
        if (subject == null)
            throw new UserFriendlyException(AcademicExceptionCodes.SubjectNotFound, "Subject not found.");
        if (!subject.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.SubjectNotActive, "Subject is not active.");

        // Validate grade exists and is active
        var grade = await _gradeRepository.FirstOrDefaultAsync(g => g.Id == input.GradeId);
        if (grade == null)
            throw new UserFriendlyException(AcademicExceptionCodes.GradeNotFound, "Grade not found.");
        if (!grade.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.GradeNotActive, "Grade is not active.");

        // Check for duplicate assignment (tenant-scoped through validated FKs)
        var existing = await _teacherSubjectRepository
            .GetAll()
            .Where(ts => ts.Teacher.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(ts => ts.TeacherId == input.TeacherId
                && ts.SubjectId == input.SubjectId
                && ts.GradeId == input.GradeId);

        if (existing != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateTeacherSubject,
                "This teacher is already assigned to this subject for this grade.");

        var assignment = new TeacherSubject(
            Guid.NewGuid(),
            input.TeacherId,
            input.SubjectId,
            input.GradeId)
        {
            IsPrimary = input.IsPrimary
        };

        await _teacherSubjectRepository.InsertAsync(assignment);
        await CurrentUnitOfWork.SaveChangesAsync();

        var saved = await _teacherSubjectRepository
            .GetAll()
            .Include(ts => ts.Teacher)
            .Include(ts => ts.Subject)
            .Include(ts => ts.Grade)
            .Where(ts => ts.Teacher.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(ts => ts.Id == assignment.Id);

        return ObjectMapper.Map<TeacherSubjectDto>(saved);
    }

    [AbpAuthorize(PermissionNames.Academic_Teachers_Edit)]
    public async Task UnassignAsync(Guid id)
    {
        var assignment = await _teacherSubjectRepository
            .GetAll()
            .Include(ts => ts.Teacher)
            .Where(ts => ts.Teacher.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(ts => ts.Id == id);

        if (assignment == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherSubjectNotFound,
                "Teacher-subject assignment not found.");

        await _teacherSubjectRepository.DeleteAsync(assignment);
    }
}
