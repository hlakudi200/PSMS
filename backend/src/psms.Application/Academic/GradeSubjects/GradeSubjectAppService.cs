using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.GradeSubjects.Dto;
using psms.Academic.Shared;
using psms.Academic.Subjects.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Academic.GradeSubjects;

/// <summary>
/// Service for managing grade-subject assignments (junction table).
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Subjects)]
public class GradeSubjectAppService : ApplicationService, IGradeSubjectAppService
{
    private readonly IRepository<GradeSubject, Guid> _gradeSubjectRepository;
    private readonly IRepository<Grade, Guid> _gradeRepository;
    private readonly IRepository<Subject, Guid> _subjectRepository;

    public GradeSubjectAppService(
        IRepository<GradeSubject, Guid> gradeSubjectRepository,
        IRepository<Grade, Guid> gradeRepository,
        IRepository<Subject, Guid> subjectRepository)
    {
        _gradeSubjectRepository = gradeSubjectRepository;
        _gradeRepository = gradeRepository;
        _subjectRepository = subjectRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_View)]
    public async Task<ListResultDto<GradeSubjectDto>> GetByGradeAsync(Guid gradeId)
    {
        var assignments = await _gradeSubjectRepository
            .GetAll()
            .Include(gs => gs.Grade)
            .Include(gs => gs.Subject)
            .Where(gs => gs.Grade.TenantId == AbpSession.TenantId)
            .Where(gs => gs.GradeId == gradeId)
            .OrderBy(gs => gs.Subject.SubjectName)
            .ToListAsync();

        return new ListResultDto<GradeSubjectDto>(
            ObjectMapper.Map<List<GradeSubjectDto>>(assignments));
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_View)]
    public async Task<ListResultDto<GradeSubjectDto>> GetBySubjectAsync(Guid subjectId)
    {
        var assignments = await _gradeSubjectRepository
            .GetAll()
            .Include(gs => gs.Grade)
            .Include(gs => gs.Subject)
            .Where(gs => gs.Grade.TenantId == AbpSession.TenantId)
            .Where(gs => gs.SubjectId == subjectId)
            .OrderBy(gs => gs.Grade.GradeLevel)
            .ToListAsync();

        return new ListResultDto<GradeSubjectDto>(
            ObjectMapper.Map<List<GradeSubjectDto>>(assignments));
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_Manage)]
    public async Task<GradeSubjectDto> AssignAsync(AssignSubjectToGradeDto input)
    {
        // Validate grade exists and is active
        var grade = await _gradeRepository.GetAsync(input.GradeId);
        if (!grade.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.GradeNotActive,
                "Cannot assign subjects to an inactive grade.");

        // Validate subject exists and is active
        var subject = await _subjectRepository.GetAsync(input.SubjectId);
        if (!subject.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.SubjectNotActive,
                "Cannot assign an inactive subject to a grade.");

        // Check for duplicate assignment (tenant-scoped through validated FKs)
        var existing = await _gradeSubjectRepository
            .GetAll()
            .Where(gs => gs.Grade.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(gs => gs.GradeId == input.GradeId && gs.SubjectId == input.SubjectId);

        if (existing != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateGradeSubject,
                $"Subject '{subject.SubjectName}' is already assigned to grade '{grade.GradeName}'.");

        var gradeSubject = new GradeSubject(
            Guid.NewGuid(),
            input.GradeId,
            input.SubjectId,
            input.IsRequired);

        await _gradeSubjectRepository.InsertAsync(gradeSubject);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Reload with navigation properties
        var saved = await _gradeSubjectRepository
            .GetAll()
            .Include(gs => gs.Grade)
            .Include(gs => gs.Subject)
            .Where(gs => gs.Grade.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(gs => gs.Id == gradeSubject.Id);

        return ObjectMapper.Map<GradeSubjectDto>(saved);
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_Manage)]
    public async Task UnassignAsync(Guid id)
    {
        // Join through Grade to ensure tenant isolation (GradeSubject has no IMayHaveTenant)
        var gradeSubject = await _gradeSubjectRepository
            .GetAll()
            .Include(gs => gs.Grade)
            .Where(gs => gs.Grade.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(gs => gs.Id == id);

        if (gradeSubject == null)
            throw new UserFriendlyException(AcademicExceptionCodes.GradeSubjectNotFound,
                "Grade-subject assignment not found.");

        await _gradeSubjectRepository.DeleteAsync(gradeSubject);
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_View)]
    public async Task<ListResultDto<SubjectListDto>> GetUnassignedSubjectsForGradeAsync(Guid gradeId)
    {
        var assignedSubjectIds = await _gradeSubjectRepository
            .GetAll()
            .Where(gs => gs.Grade.TenantId == AbpSession.TenantId)
            .Where(gs => gs.GradeId == gradeId)
            .Select(gs => gs.SubjectId)
            .ToListAsync();

        var unassignedSubjects = await _subjectRepository
            .GetAll()
            .Include(s => s.GradeSubjects)
            .Where(s => s.IsActive && !assignedSubjectIds.Contains(s.Id))
            .OrderBy(s => s.SubjectName)
            .ToListAsync();

        return new ListResultDto<SubjectListDto>(
            ObjectMapper.Map<List<SubjectListDto>>(unassignedSubjects));
    }
}
