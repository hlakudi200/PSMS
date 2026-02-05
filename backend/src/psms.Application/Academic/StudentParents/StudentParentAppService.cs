using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.StudentParents.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Academic.StudentParents;

/// <summary>
/// Service for managing student-parent/guardian links.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Students)]
public class StudentParentAppService : ApplicationService, IStudentParentAppService
{
    private readonly IRepository<StudentParent, Guid> _studentParentRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<Parent, Guid> _parentRepository;

    public StudentParentAppService(
        IRepository<StudentParent, Guid> studentParentRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<Parent, Guid> parentRepository)
    {
        _studentParentRepository = studentParentRepository;
        _studentRepository = studentRepository;
        _parentRepository = parentRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<ListResultDto<StudentParentDto>> GetByStudentAsync(Guid studentId)
    {
        var links = await _studentParentRepository
            .GetAll()
            .Include(sp => sp.Student)
            .Include(sp => sp.Parent)
            .Where(sp => sp.StudentId == studentId)
            .OrderBy(sp => sp.RelationshipType)
            .ToListAsync();

        return new ListResultDto<StudentParentDto>(
            ObjectMapper.Map<List<StudentParentDto>>(links));
    }

    [AbpAuthorize(PermissionNames.Academic_Parents_View)]
    public async Task<ListResultDto<StudentParentDto>> GetByParentAsync(Guid parentId)
    {
        var links = await _studentParentRepository
            .GetAll()
            .Include(sp => sp.Student)
            .Include(sp => sp.Parent)
            .Where(sp => sp.ParentId == parentId)
            .OrderBy(sp => sp.Student.LastName)
            .ThenBy(sp => sp.Student.FirstName)
            .ToListAsync();

        return new ListResultDto<StudentParentDto>(
            ObjectMapper.Map<List<StudentParentDto>>(links));
    }

    [AbpAuthorize(PermissionNames.Academic_Students_Edit)]
    public async Task<StudentParentDto> LinkAsync(LinkStudentParentDto input)
    {
        // Validate student exists (join through Student for tenant isolation)
        var student = await _studentRepository.FirstOrDefaultAsync(s => s.Id == input.StudentId);
        if (student == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");

        // Validate parent exists
        var parent = await _parentRepository.FirstOrDefaultAsync(p => p.Id == input.ParentId);
        if (parent == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ParentNotFound, "Parent not found.");

        // Check for duplicate link
        var existing = await _studentParentRepository
            .FirstOrDefaultAsync(sp => sp.StudentId == input.StudentId && sp.ParentId == input.ParentId);

        if (existing != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateStudentParentLink,
                "This student-parent link already exists.");

        var link = new StudentParent(
            Guid.NewGuid(),
            input.StudentId,
            input.ParentId,
            input.RelationshipType)
        {
            IsPrimaryContact = input.IsPrimaryContact,
            IsFinanciallyResponsible = input.IsFinanciallyResponsible,
            CanPickupStudent = input.CanPickupStudent,
            LivesWithStudent = input.LivesWithStudent
        };

        await _studentParentRepository.InsertAsync(link);
        await CurrentUnitOfWork.SaveChangesAsync();

        var saved = await _studentParentRepository
            .GetAll()
            .Include(sp => sp.Student)
            .Include(sp => sp.Parent)
            .FirstOrDefaultAsync(sp => sp.Id == link.Id);

        return ObjectMapper.Map<StudentParentDto>(saved);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_Edit)]
    public async Task<StudentParentDto> UpdateLinkAsync(Guid id, LinkStudentParentDto input)
    {
        // Join through Student for tenant isolation
        var link = await _studentParentRepository
            .GetAll()
            .Include(sp => sp.Student)
            .FirstOrDefaultAsync(sp => sp.Id == id);

        if (link == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentParentLinkNotFound,
                "Student-parent link not found.");

        link.RelationshipType = input.RelationshipType;
        link.IsPrimaryContact = input.IsPrimaryContact;
        link.IsFinanciallyResponsible = input.IsFinanciallyResponsible;
        link.CanPickupStudent = input.CanPickupStudent;
        link.LivesWithStudent = input.LivesWithStudent;

        await _studentParentRepository.UpdateAsync(link);
        await CurrentUnitOfWork.SaveChangesAsync();

        var saved = await _studentParentRepository
            .GetAll()
            .Include(sp => sp.Student)
            .Include(sp => sp.Parent)
            .FirstOrDefaultAsync(sp => sp.Id == id);

        return ObjectMapper.Map<StudentParentDto>(saved);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_Edit)]
    public async Task UnlinkAsync(Guid id)
    {
        // Join through Student for tenant isolation
        var link = await _studentParentRepository
            .GetAll()
            .Include(sp => sp.Student)
            .FirstOrDefaultAsync(sp => sp.Id == id);

        if (link == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentParentLinkNotFound,
                "Student-parent link not found.");

        await _studentParentRepository.DeleteAsync(link);
    }
}
