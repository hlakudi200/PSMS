using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.Subjects.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Academic.Subjects;

/// <summary>
/// Service for managing academic subjects.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Subjects)]
public class SubjectAppService : ApplicationService, ISubjectAppService
{
    private readonly IRepository<Subject, Guid> _subjectRepository;

    public SubjectAppService(IRepository<Subject, Guid> subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_View)]
    public async Task<SubjectDto> GetAsync(Guid id)
    {
        var subject = await _subjectRepository
            .GetAll()
            .Include(s => s.GradeSubjects)
            .Include(s => s.TeacherSubjects)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == AbpSession.TenantId);

        if (subject == null)
            throw new UserFriendlyException(AcademicExceptionCodes.SubjectNotFound, "Subject not found.");

        return ObjectMapper.Map<SubjectDto>(subject);
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_View)]
    public async Task<PagedResultDto<SubjectListDto>> GetAllAsync(GetAcademicEntityInput input)
    {
        var query = _subjectRepository
            .GetAll()
            .Include(s => s.GradeSubjects)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                s => s.SubjectName.ToLower().Contains(input.Keyword.ToLower())
                  || s.SubjectCode.ToLower().Contains(input.Keyword.ToLower()))
            .WhereIf(input.IsActive.HasValue,
                s => s.IsActive == input.IsActive.Value)
            .WhereIf(input.IsCore.HasValue,
                s => s.IsCore == input.IsCore.Value);

        var totalCount = await query.CountAsync();

        var subjects = await query
            .OrderBy(input.Sorting ?? "SubjectName ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<SubjectListDto>(
            totalCount,
            ObjectMapper.Map<List<SubjectListDto>>(subjects));
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_Manage)]
    public async Task<SubjectDto> CreateAsync(CreateSubjectDto input)
    {
        // Validate unique subject code within tenant
        var existingSubject = await _subjectRepository
            .FirstOrDefaultAsync(s => s.SubjectCode == input.SubjectCode);

        if (existingSubject != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateSubjectCode,
                $"A subject with code '{input.SubjectCode}' already exists.");

        var subject = new Subject(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.SubjectName,
            input.SubjectCode,
            input.IsCore)
        {
            Description = input.Description
        };

        await _subjectRepository.InsertAsync(subject);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(subject.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_Manage)]
    public async Task<SubjectDto> UpdateAsync(Guid id, UpdateSubjectDto input)
    {
        var subject = await _subjectRepository.GetAsync(id);

        // Validate unique subject code if changing
        if (input.SubjectCode != null && input.SubjectCode != subject.SubjectCode)
        {
            var existingSubject = await _subjectRepository
                .FirstOrDefaultAsync(s => s.SubjectCode == input.SubjectCode && s.Id != id);

            if (existingSubject != null)
                throw new UserFriendlyException(AcademicExceptionCodes.DuplicateSubjectCode,
                    $"A subject with code '{input.SubjectCode}' already exists.");
        }

        if (input.SubjectName != null) subject.SubjectName = input.SubjectName;
        if (input.SubjectCode != null) subject.SubjectCode = input.SubjectCode;
        if (input.Description != null) subject.Description = input.Description;
        if (input.IsCore.HasValue) subject.IsCore = input.IsCore.Value;
        if (input.IsActive.HasValue) subject.IsActive = input.IsActive.Value;

        await _subjectRepository.UpdateAsync(subject);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var subject = await _subjectRepository
            .GetAll()
            .Include(s => s.GradeSubjects)
            .Include(s => s.TeacherSubjects)
            .Include(s => s.StudentSubjects)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == AbpSession.TenantId);

        if (subject == null)
            throw new UserFriendlyException(AcademicExceptionCodes.SubjectNotFound, "Subject not found.");

        if (subject.GradeSubjects.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteSubjectWithGrades,
                "Cannot delete a subject that is assigned to grades. Remove grade assignments first.");

        if (subject.TeacherSubjects.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteSubjectWithTeachers,
                "Cannot delete a subject that has teachers assigned.");

        if (subject.StudentSubjects.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteSubjectWithStudents,
                "Cannot delete a subject that has students enrolled.");

        await _subjectRepository.DeleteAsync(subject);
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_View)]
    public async Task<ListResultDto<SubjectListDto>> GetActiveSubjectsAsync()
    {
        var subjects = await _subjectRepository
            .GetAll()
            .Include(s => s.GradeSubjects)
            .Where(s => s.IsActive)
            .OrderBy(s => s.SubjectName)
            .ToListAsync();

        return new ListResultDto<SubjectListDto>(
            ObjectMapper.Map<List<SubjectListDto>>(subjects));
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_View)]
    public async Task<ListResultDto<SubjectListDto>> GetCoreSubjectsAsync()
    {
        var subjects = await _subjectRepository
            .GetAll()
            .Include(s => s.GradeSubjects)
            .Where(s => s.IsCore && s.IsActive)
            .OrderBy(s => s.SubjectName)
            .ToListAsync();

        return new ListResultDto<SubjectListDto>(
            ObjectMapper.Map<List<SubjectListDto>>(subjects));
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_Manage)]
    public async Task ActivateAsync(Guid id)
    {
        var subject = await _subjectRepository.GetAsync(id);
        subject.IsActive = true;
        await _subjectRepository.UpdateAsync(subject);
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_Manage)]
    public async Task DeactivateAsync(Guid id)
    {
        var subject = await _subjectRepository.GetAsync(id);
        subject.IsActive = false;
        await _subjectRepository.UpdateAsync(subject);
    }
}
