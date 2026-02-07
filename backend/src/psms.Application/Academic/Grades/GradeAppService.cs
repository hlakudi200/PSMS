using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Grades.Dto;
using psms.Academic.Shared;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Academic.Grades;

/// <summary>
/// Service for managing grades. Implements AR-005.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Grades)]
public class GradeAppService : ApplicationService, IGradeAppService
{
    private readonly IRepository<Grade, Guid> _gradeRepository;

    public GradeAppService(IRepository<Grade, Guid> gradeRepository)
    {
        _gradeRepository = gradeRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Grades_View)]
    public async Task<GradeDto> GetAsync(Guid id)
    {
        var grade = await _gradeRepository
            .GetAll()
            .Include(g => g.Classes)
            .Include(g => g.Students)
            .Include(g => g.GradeSubjects)
            .FirstOrDefaultAsync(g => g.Id == id && g.TenantId == AbpSession.TenantId);

        if (grade == null)
            throw new UserFriendlyException(AcademicExceptionCodes.GradeNotFound, "Grade not found.");

        return ObjectMapper.Map<GradeDto>(grade);
    }

    [AbpAuthorize(PermissionNames.Academic_Grades_View)]
    public async Task<PagedResultDto<GradeListDto>> GetAllAsync(PagedAndSortedResultRequestDto input)
    {
        var query = _gradeRepository
            .GetAll()
            .Include(g => g.Classes)
            .Include(g => g.Students);

        var totalCount = await query.CountAsync();

        var grades = await query
            .OrderBy(input.Sorting ?? "GradeLevel ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<GradeListDto>(
            totalCount,
            ObjectMapper.Map<List<GradeListDto>>(grades));
    }

    [AbpAuthorize(PermissionNames.Academic_Grades_Manage)]
    public async Task<GradeDto> CreateAsync(CreateGradeDto input)
    {
        // AR-005: Validate school phase matches grade level
        ValidateSchoolPhaseForGrade(input.GradeLevel, input.SchoolPhase);

        // Check for duplicate grade level in this tenant
        var existingGrade = await _gradeRepository
            .FirstOrDefaultAsync(g => g.GradeLevel == input.GradeLevel);

        if (existingGrade != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateGradeLevel,
                $"A grade with level '{input.GradeLevel}' already exists.");

        var grade = new Grade(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.GradeLevel,
            input.GradeName,
            input.SchoolPhase)
        {
            Description = input.Description
        };

        await _gradeRepository.InsertAsync(grade);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(grade.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Grades_Manage)]
    public async Task<GradeDto> UpdateAsync(Guid id, UpdateGradeDto input)
    {
        var grade = await _gradeRepository.GetAsync(id);

        // Validate phase if provided
        if (input.SchoolPhase.HasValue)
            ValidateSchoolPhaseForGrade(grade.GradeLevel, input.SchoolPhase.Value);

        if (input.GradeName != null) grade.GradeName = input.GradeName;
        if (input.SchoolPhase.HasValue) grade.SchoolPhase = input.SchoolPhase.Value;
        if (input.Description != null) grade.Description = input.Description;
        if (input.IsActive.HasValue) grade.IsActive = input.IsActive.Value;

        await _gradeRepository.UpdateAsync(grade);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Grades_Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var grade = await _gradeRepository
            .GetAll()
            .Include(g => g.Students)
            .Include(g => g.Classes)
            .FirstOrDefaultAsync(g => g.Id == id && g.TenantId == AbpSession.TenantId);

        if (grade == null)
            throw new UserFriendlyException(AcademicExceptionCodes.GradeNotFound, "Grade not found.");

        if (grade.Students.Any(s => !s.IsDeleted))
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteGradeWithStudents,
                "Cannot delete a grade that has students assigned.");

        if (grade.Classes.Any(c => !c.IsDeleted))
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteGradeWithClasses,
                "Cannot delete a grade that has classes assigned.");

        await _gradeRepository.DeleteAsync(grade);
    }

    [AbpAuthorize(PermissionNames.Academic_Grades_View)]
    public async Task<ListResultDto<GradeListDto>> GetActiveGradesAsync()
    {
        var grades = await _gradeRepository
            .GetAll()
            .Include(g => g.Classes)
            .Include(g => g.Students)
            .Where(g => g.IsActive)
            .OrderBy(g => g.GradeLevel)
            .ToListAsync();

        return new ListResultDto<GradeListDto>(
            ObjectMapper.Map<List<GradeListDto>>(grades));
    }

    [AbpAuthorize(PermissionNames.Academic_Grades_View)]
    public async Task<ListResultDto<GradeListDto>> GetByPhaseAsync(SouthAfricanSchoolPhase phase)
    {
        var grades = await _gradeRepository
            .GetAll()
            .Include(g => g.Classes)
            .Include(g => g.Students)
            .Where(g => g.SchoolPhase == phase)
            .OrderBy(g => g.GradeLevel)
            .ToListAsync();

        return new ListResultDto<GradeListDto>(
            ObjectMapper.Map<List<GradeListDto>>(grades));
    }

    [AbpAuthorize(PermissionNames.Academic_Grades_Manage)]
    public async Task ActivateAsync(Guid id)
    {
        var grade = await _gradeRepository.GetAsync(id);
        grade.IsActive = true;
        await _gradeRepository.UpdateAsync(grade);
    }

    [AbpAuthorize(PermissionNames.Academic_Grades_Manage)]
    public async Task DeactivateAsync(Guid id)
    {
        var grade = await _gradeRepository.GetAsync(id);
        grade.IsActive = false;
        await _gradeRepository.UpdateAsync(grade);
    }

    /// <summary>
    /// AR-005: Validates that school phase matches grade level.
    /// Foundation (R-3), Intermediate (4-6), Senior (7-9), FET (10-12).
    /// </summary>
    private static void ValidateSchoolPhaseForGrade(SouthAfricanGradeLevel level, SouthAfricanSchoolPhase phase)
    {
        var expectedPhase = level switch
        {
            <= SouthAfricanGradeLevel.Grade3 => SouthAfricanSchoolPhase.Foundation,
            <= SouthAfricanGradeLevel.Grade6 => SouthAfricanSchoolPhase.Intermediate,
            <= SouthAfricanGradeLevel.Grade9 => SouthAfricanSchoolPhase.Senior,
            _ => SouthAfricanSchoolPhase.FET
        };

        if (phase != expectedPhase)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidSchoolPhaseForGrade,
                $"Grade {level} should be in {expectedPhase} phase, not {phase}.");
    }
}
