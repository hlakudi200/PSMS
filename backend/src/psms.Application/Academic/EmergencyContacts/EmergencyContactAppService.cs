using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.EmergencyContacts.Dto;
using psms.Academic.Shared;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Academic.EmergencyContacts;

[AbpAuthorize(PermissionNames.Academic_Students)]
public class EmergencyContactAppService : ApplicationService, IEmergencyContactAppService
{
    private readonly IRepository<EmergencyContact, Guid> _emergencyContactRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly psms.Academic.Students.ICurrentStudentResolver _currentStudent;
    private readonly psms.Academic.Parents.ICurrentParentResolver _currentParent;

    public EmergencyContactAppService(
        IRepository<EmergencyContact, Guid> emergencyContactRepository,
        IRepository<Student, Guid> studentRepository,
        psms.Academic.Students.ICurrentStudentResolver currentStudent,
        psms.Academic.Parents.ICurrentParentResolver currentParent)
    {
        _emergencyContactRepository = emergencyContactRepository;
        _studentRepository = studentRepository;
        _currentStudent = currentStudent;
        _currentParent = currentParent;
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<EmergencyContactDto> GetAsync(Guid id)
    {
        var contact = await _emergencyContactRepository
            .GetAll()
            .Include(ec => ec.Student)
            .FirstOrDefaultAsync(ec => ec.Id == id && ec.TenantId == AbpSession.TenantId);

        if (contact == null)
            throw new UserFriendlyException(AcademicExceptionCodes.EmergencyContactNotFound, "Emergency contact not found.");

        // LC-08: a student may only read their own contacts.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != contact.StudentId)
            throw new UserFriendlyException(AcademicExceptionCodes.EmergencyContactNotFound, "Emergency contact not found.");

        // MOB-BE-04: a parent may only read their own children's contacts.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(contact.StudentId))
            throw new UserFriendlyException(AcademicExceptionCodes.EmergencyContactNotFound, "Emergency contact not found.");

        return ObjectMapper.Map<EmergencyContactDto>(contact);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<ListResultDto<EmergencyContactListDto>> GetByStudentAsync(Guid studentId)
    {
        // LC-08: a student may only read their own contacts.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != studentId)
            throw new UserFriendlyException(AcademicExceptionCodes.EmergencyContactNotFound, "Emergency contact not found.");

        // MOB-BE-04: a parent may only read their own children's contacts.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(studentId))
            throw new UserFriendlyException(AcademicExceptionCodes.EmergencyContactNotFound, "Emergency contact not found.");

        var contacts = await _emergencyContactRepository
            .GetAll()
            .Where(ec => ec.StudentId == studentId && ec.TenantId == AbpSession.TenantId)
            .OrderBy(ec => ec.Priority)
            .ToListAsync();

        return new ListResultDto<EmergencyContactListDto>(
            ObjectMapper.Map<List<EmergencyContactListDto>>(contacts));
    }

    [AbpAuthorize(PermissionNames.Academic_Students_ManageEmergencyContacts)]
    public async Task<EmergencyContactDto> CreateAsync(CreateEmergencyContactDto input)
    {
        // Validate student exists and belongs to tenant
        var student = await _studentRepository
            .FirstOrDefaultAsync(s => s.Id == input.StudentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");

        // Validate priority unique per student
        var existingPriority = await _emergencyContactRepository
            .FirstOrDefaultAsync(ec => ec.StudentId == input.StudentId
                && ec.Priority == input.Priority
                && ec.TenantId == AbpSession.TenantId);

        if (existingPriority != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateEmergencyContactPriority,
                $"An emergency contact with priority {input.Priority} already exists for this student.");

        var contact = new EmergencyContact(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.StudentId,
            input.FirstName,
            input.LastName,
            input.Relationship,
            input.PrimaryPhone,
            input.Priority)
        {
            SecondaryPhone = input.SecondaryPhone,
            WorkPhone = input.WorkPhone,
            Email = input.Email?.Trim().ToLowerInvariant(),
            Address = input.Address,
            CanPickUp = input.CanPickUp,
            CanMakeMedicalDecisions = input.CanMakeMedicalDecisions
        };

        await _emergencyContactRepository.InsertAsync(contact);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(contact.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_ManageEmergencyContacts)]
    public async Task<EmergencyContactDto> UpdateAsync(Guid id, UpdateEmergencyContactDto input)
    {
        var contact = await _emergencyContactRepository
            .FirstOrDefaultAsync(ec => ec.Id == id && ec.TenantId == AbpSession.TenantId);

        if (contact == null)
            throw new UserFriendlyException(AcademicExceptionCodes.EmergencyContactNotFound, "Emergency contact not found.");

        // Validate priority unique per student if changing
        if (input.Priority.HasValue && input.Priority.Value != contact.Priority)
        {
            var existingPriority = await _emergencyContactRepository
                .FirstOrDefaultAsync(ec => ec.StudentId == contact.StudentId
                    && ec.Priority == input.Priority.Value
                    && ec.Id != id
                    && ec.TenantId == AbpSession.TenantId);

            if (existingPriority != null)
                throw new UserFriendlyException(AcademicExceptionCodes.DuplicateEmergencyContactPriority,
                    $"An emergency contact with priority {input.Priority.Value} already exists for this student.");
        }

        if (input.FirstName != null) contact.FirstName = input.FirstName;
        if (input.LastName != null) contact.LastName = input.LastName;
        if (input.Relationship.HasValue) contact.Relationship = input.Relationship.Value;
        if (input.PrimaryPhone != null) contact.PrimaryPhone = input.PrimaryPhone;
        if (input.SecondaryPhone != null) contact.SecondaryPhone = input.SecondaryPhone;
        if (input.WorkPhone != null) contact.WorkPhone = input.WorkPhone;
        if (input.Email != null) contact.Email = input.Email.Trim().ToLowerInvariant();
        if (input.Address != null) contact.Address = input.Address;
        if (input.Priority.HasValue) contact.Priority = input.Priority.Value;
        if (input.CanPickUp.HasValue) contact.CanPickUp = input.CanPickUp.Value;
        if (input.CanMakeMedicalDecisions.HasValue) contact.CanMakeMedicalDecisions = input.CanMakeMedicalDecisions.Value;
        if (input.IsActive.HasValue) contact.IsActive = input.IsActive.Value;

        await _emergencyContactRepository.UpdateAsync(contact);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_ManageEmergencyContacts)]
    public async Task DeleteAsync(Guid id)
    {
        var contact = await _emergencyContactRepository
            .FirstOrDefaultAsync(ec => ec.Id == id && ec.TenantId == AbpSession.TenantId);

        if (contact == null)
            throw new UserFriendlyException(AcademicExceptionCodes.EmergencyContactNotFound, "Emergency contact not found.");

        await _emergencyContactRepository.DeleteAsync(contact);
    }
}
