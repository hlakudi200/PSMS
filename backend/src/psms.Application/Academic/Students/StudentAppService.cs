using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.Students.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Validators;
using psms.Domain.Shared.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Academic.Students;

/// <summary>
/// Service for managing students (learners).
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Students)]
public class StudentAppService : ApplicationService, IStudentAppService
{
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<Grade, Guid> _gradeRepository;
    private readonly IRepository<Class, Guid> _classRepository;
    private readonly IStudentLoginProvisioner _loginProvisioner;
    private readonly ICurrentStudentResolver _currentStudent;
    private readonly psms.Academic.Parents.ICurrentParentResolver _currentParent;

    public StudentAppService(
        IRepository<Student, Guid> studentRepository,
        IRepository<Grade, Guid> gradeRepository,
        IRepository<Class, Guid> classRepository,
        IStudentLoginProvisioner loginProvisioner,
        ICurrentStudentResolver currentStudent,
        psms.Academic.Parents.ICurrentParentResolver currentParent)
    {
        _studentRepository = studentRepository;
        _gradeRepository = gradeRepository;
        _classRepository = classRepository;
        _loginProvisioner = loginProvisioner;
        _currentStudent = currentStudent;
        _currentParent = currentParent;
    }

    /// <summary>Returns a single-item list of just the given student (LC-08 self-scope).</summary>
    private async Task<ListResultDto<StudentListDto>> SelfOnlyListAsync(Guid studentId)
    {
        var self = await _studentRepository
            .GetAll()
            .Include(s => s.CurrentGrade)
            .Include(s => s.CurrentClass)
            .Include(s => s.ParentLinks)
            .FirstOrDefaultAsync(s => s.Id == studentId);

        var list = self == null ? new List<Student>() : new List<Student> { self };
        return new ListResultDto<StudentListDto>(ObjectMapper.Map<List<StudentListDto>>(list));
    }

    /// <summary>Returns just the given children (MOB-BE-04 parent-scope).</summary>
    private async Task<ListResultDto<StudentListDto>> ChildrenOnlyListAsync(List<Guid> studentIds)
    {
        var children = await _studentRepository
            .GetAll()
            .Include(s => s.CurrentGrade)
            .Include(s => s.CurrentClass)
            .Include(s => s.ParentLinks)
            .Where(s => studentIds.Contains(s.Id))
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();

        return new ListResultDto<StudentListDto>(ObjectMapper.Map<List<StudentListDto>>(children));
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<StudentDto> GetAsync(Guid id)
    {
        // LC-08: a student-portal user (a user linked to a Student record) may
        // only read their OWN record. Staff have no Student record → unchanged.
        // Return not-found (not forbidden) so existence isn't leaked.
        var selfStudentId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfStudentId.HasValue && selfStudentId.Value != id)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");

        // MOB-BE-04: a parent may only read their own children's records.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(id))
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");

        var student = await _studentRepository
            .GetAll()
            .Include(s => s.CurrentGrade)
            .Include(s => s.CurrentClass)
            .Include(s => s.ParentLinks)
            .Include(s => s.SubjectEnrollments)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");

        return ObjectMapper.Map<StudentDto>(student);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<PagedResultDto<StudentListDto>> GetAllAsync(GetAcademicEntityInput input)
    {
        // LC-08: a student-portal user only ever sees their own record.
        var selfStudentId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfStudentId.HasValue)
        {
            var selfList = (await SelfOnlyListAsync(selfStudentId.Value)).Items.ToList();
            return new PagedResultDto<StudentListDto>(selfList.Count, selfList);
        }

        // MOB-BE-04: a parent-portal user only ever sees their own children.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null)
        {
            var childList = (await ChildrenOnlyListAsync(childIds)).Items.ToList();
            return new PagedResultDto<StudentListDto>(childList.Count, childList);
        }

        var query = _studentRepository
            .GetAll()
            .Include(s => s.CurrentGrade)
            .Include(s => s.CurrentClass)
            .Include(s => s.ParentLinks)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                s => s.FirstName.ToLower().Contains(input.Keyword.ToLower())
                  || s.LastName.ToLower().Contains(input.Keyword.ToLower())
                  || s.AdmissionNumber.ToLower().Contains(input.Keyword.ToLower()))
            .WhereIf(input.IsActive.HasValue,
                s => s.IsActive == input.IsActive.Value)
            .WhereIf(input.Gender.HasValue,
                s => (int)s.Gender == input.Gender.Value);

        var totalCount = await query.CountAsync();

        var students = await query
            .OrderBy(input.Sorting ?? "LastName ASC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<StudentListDto>(
            totalCount,
            ObjectMapper.Map<List<StudentListDto>>(students));
    }

    [AbpAuthorize(PermissionNames.Academic_Students_Create)]
    public async Task<StudentDto> CreateAsync(CreateStudentDto input)
    {
        // Validate date of birth is in the past
        if (input.DateOfBirth.Date >= DateTime.Today)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidDateOfBirth,
                "Date of birth must be in the past.");

        // Validate admission date is not in the future
        if (input.AdmissionDate.Date > DateTime.Today)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidAdmissionDate,
                "Admission date cannot be in the future.");

        // Validate unique admission number (scoped to tenant via ABP filter)
        var existingByNumber = await _studentRepository
            .FirstOrDefaultAsync(s => s.AdmissionNumber == input.AdmissionNumber);

        if (existingByNumber != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateAdmissionNumber,
                $"A student with admission number '{input.AdmissionNumber}' already exists.");

        // ER-002: Validate SA ID number if provided
        if (!string.IsNullOrWhiteSpace(input.IdNumber) && !SAIdNumberValidator.IsValid(input.IdNumber))
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidSAIdNumber,
                "Invalid South African ID number. Must be 13 digits and pass Luhn validation.");

        // Validate grade exists and is active
        var grade = await _gradeRepository.FirstOrDefaultAsync(g => g.Id == input.CurrentGradeId);
        if (grade == null)
            throw new UserFriendlyException(AcademicExceptionCodes.GradeNotFound, "Grade not found.");
        if (!grade.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.GradeNotActive, "Grade is not active.");

        // Validate class exists, is active, and belongs to the grade
        var cls = await _classRepository.FirstOrDefaultAsync(c => c.Id == input.CurrentClassId);
        if (cls == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotFound, "Class not found.");
        if (!cls.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotActive, "Class is not active.");
        if (cls.GradeId != input.CurrentGradeId)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassGradeMismatch,
                "The selected class does not belong to the selected grade.");

        // AR-006: Validate class capacity
        var currentEnrollment = await _studentRepository
            .CountAsync(s => s.CurrentClassId == input.CurrentClassId);
        if (currentEnrollment >= cls.MaxCapacity)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassAtCapacity,
                $"Cannot enroll student. Class is at full capacity ({cls.MaxCapacity}).");

        var student = new Student(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.FirstName,
            input.LastName,
            input.DateOfBirth,
            input.Gender,
            input.AdmissionNumber,
            input.AdmissionDate,
            input.CurrentGradeId,
            input.CurrentClassId)
        {
            MiddleName = input.MiddleName,
            IdNumber = input.IdNumber,
            PassportNumber = input.PassportNumber,
            IsSACitizen = input.IsSACitizen,
            Phone = input.Phone,
            Email = input.Email?.Trim().ToLowerInvariant(),
            EmergencyContactName = input.EmergencyContactName,
            EmergencyContactPhone = input.EmergencyContactPhone,
            MedicalConditions = input.MedicalConditions,
            POPIAConsentGiven = input.POPIAConsentGiven,
            POPIAConsentDate = input.POPIAConsentGiven ? DateTime.UtcNow : null
        };

        if (input.PhysicalAddress != null)
        {
            student.PhysicalAddress = new Address(
                input.PhysicalAddress.StreetAddress,
                input.PhysicalAddress.Suburb,
                input.PhysicalAddress.City,
                input.PhysicalAddress.Province,
                input.PhysicalAddress.PostalCode,
                input.PhysicalAddress.Country);
        }

        if (input.PostalAddress != null)
        {
            student.PostalAddress = new Address(
                input.PostalAddress.StreetAddress,
                input.PostalAddress.Suburb,
                input.PostalAddress.City,
                input.PostalAddress.Province,
                input.PostalAddress.PostalCode,
                input.PostalAddress.Country);
        }

        await _studentRepository.InsertAsync(student);

        // LC-07: provision a portal login account for the student and link it,
        // in the same unit of work so a provisioning failure (e.g. a duplicate
        // username) rolls back the whole create — every student ends up with
        // exactly one linked account.
        var login = await _loginProvisioner.ProvisionAsync(student);

        await CurrentUnitOfWork.SaveChangesAsync();

        var dto = await GetAsync(student.Id);
        // Surface the freshly-minted credentials ONCE so the admin can hand
        // them to the student (never returned on reads).
        dto.LoginUserName = login.UserName;
        dto.TemporaryPassword = login.TemporaryPassword;
        return dto;
    }

    [AbpAuthorize(PermissionNames.Academic_Students_Edit)]
    public async Task<StudentDto> UpdateAsync(Guid id, UpdateStudentDto input)
    {
        var student = await _studentRepository.GetAsync(id);

        // ER-002: Validate SA ID number if changing
        if (input.IdNumber != null && !string.IsNullOrWhiteSpace(input.IdNumber)
            && !SAIdNumberValidator.IsValid(input.IdNumber))
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidSAIdNumber,
                "Invalid South African ID number. Must be 13 digits and pass Luhn validation.");

        if (input.DateOfBirth.HasValue && input.DateOfBirth.Value.Date >= DateTime.Today)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidDateOfBirth,
                "Date of birth must be in the past.");

        if (input.FirstName != null) student.FirstName = input.FirstName;
        if (input.LastName != null) student.LastName = input.LastName;
        if (input.MiddleName != null) student.MiddleName = input.MiddleName;
        if (input.DateOfBirth.HasValue) student.DateOfBirth = input.DateOfBirth.Value;
        if (input.Gender.HasValue) student.Gender = input.Gender.Value;
        if (input.IdNumber != null) student.IdNumber = input.IdNumber;
        if (input.PassportNumber != null) student.PassportNumber = input.PassportNumber;
        if (input.IsSACitizen.HasValue) student.IsSACitizen = input.IsSACitizen.Value;
        if (input.Phone != null) student.Phone = input.Phone;
        if (input.Email != null) student.Email = input.Email.Trim().ToLowerInvariant();
        if (input.ProfilePhotoUrl != null) student.ProfilePhotoUrl = input.ProfilePhotoUrl;
        if (input.EmergencyContactName != null) student.EmergencyContactName = input.EmergencyContactName;
        if (input.EmergencyContactPhone != null) student.EmergencyContactPhone = input.EmergencyContactPhone;
        if (input.MedicalConditions != null) student.MedicalConditions = input.MedicalConditions;
        if (input.IsActive.HasValue) student.IsActive = input.IsActive.Value;

        if (input.POPIAConsentGiven.HasValue)
        {
            student.POPIAConsentGiven = input.POPIAConsentGiven.Value;
            if (input.POPIAConsentGiven.Value && !student.POPIAConsentDate.HasValue)
                student.POPIAConsentDate = DateTime.UtcNow;
        }

        if (input.PhysicalAddress != null)
        {
            student.PhysicalAddress = new Address(
                input.PhysicalAddress.StreetAddress,
                input.PhysicalAddress.Suburb,
                input.PhysicalAddress.City,
                input.PhysicalAddress.Province,
                input.PhysicalAddress.PostalCode,
                input.PhysicalAddress.Country);
        }

        if (input.PostalAddress != null)
        {
            student.PostalAddress = new Address(
                input.PostalAddress.StreetAddress,
                input.PostalAddress.Suburb,
                input.PostalAddress.City,
                input.PostalAddress.Province,
                input.PostalAddress.PostalCode,
                input.PostalAddress.Country);
        }

        await _studentRepository.UpdateAsync(student);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var student = await _studentRepository
            .GetAll()
            .Include(s => s.ParentLinks)
            .Include(s => s.SubjectEnrollments)
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");

        if (student.ParentLinks.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteStudentWithParents,
                "Cannot delete a student that has linked parents. Remove parent links first.");

        if (student.SubjectEnrollments.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteStudentWithSubjects,
                "Cannot delete a student that has subject enrollments. Remove enrollments first.");

        await _studentRepository.DeleteAsync(student);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<ListResultDto<StudentListDto>> GetActiveStudentsAsync()
    {
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue) return await SelfOnlyListAsync(selfId.Value);

        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null) return await ChildrenOnlyListAsync(childIds);

        var students = await _studentRepository
            .GetAll()
            .Include(s => s.CurrentGrade)
            .Include(s => s.CurrentClass)
            .Include(s => s.ParentLinks)
            .Where(s => s.IsActive)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();

        return new ListResultDto<StudentListDto>(
            ObjectMapper.Map<List<StudentListDto>>(students));
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<ListResultDto<StudentListDto>> GetStudentsByGradeAsync(Guid gradeId)
    {
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue) return await SelfOnlyListAsync(selfId.Value);

        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null) return await ChildrenOnlyListAsync(childIds);

        var students = await _studentRepository
            .GetAll()
            .Include(s => s.CurrentGrade)
            .Include(s => s.CurrentClass)
            .Include(s => s.ParentLinks)
            .Where(s => s.CurrentGradeId == gradeId)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();

        return new ListResultDto<StudentListDto>(
            ObjectMapper.Map<List<StudentListDto>>(students));
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<ListResultDto<StudentListDto>> GetStudentsByClassAsync(Guid classId)
    {
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue) return await SelfOnlyListAsync(selfId.Value);

        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null) return await ChildrenOnlyListAsync(childIds);

        var students = await _studentRepository
            .GetAll()
            .Include(s => s.CurrentGrade)
            .Include(s => s.CurrentClass)
            .Include(s => s.ParentLinks)
            .Where(s => s.CurrentClassId == classId)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();

        return new ListResultDto<StudentListDto>(
            ObjectMapper.Map<List<StudentListDto>>(students));
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<ListResultDto<StudentListDto>> SearchAsync(string searchTerm)
    {
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue) return await SelfOnlyListAsync(selfId.Value);

        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null) return await ChildrenOnlyListAsync(childIds);

        IQueryable<Student> query = _studentRepository
            .GetAll()
            .Include(s => s.CurrentGrade)
            .Include(s => s.CurrentClass)
            .Include(s => s.ParentLinks);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(s =>
                s.FirstName.ToLower().Contains(term) ||
                s.LastName.ToLower().Contains(term) ||
                s.AdmissionNumber.ToLower().Contains(term));
        }

        var students = await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Take(50)
            .ToListAsync();

        return new ListResultDto<StudentListDto>(
            ObjectMapper.Map<List<StudentListDto>>(students));
    }

    [AbpAuthorize(PermissionNames.Academic_Students_AssignClass)]
    public async Task<StudentDto> AssignClassAsync(Guid id, Guid classId)
    {
        var student = await _studentRepository.GetAsync(id);

        var cls = await _classRepository.FirstOrDefaultAsync(c => c.Id == classId);
        if (cls == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotFound, "Class not found.");
        if (!cls.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotActive, "Class is not active.");
        if (cls.GradeId != student.CurrentGradeId)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassGradeMismatch,
                "The selected class does not belong to the student's current grade.");

        // AR-006: Validate class capacity
        var currentEnrollment = await _studentRepository
            .CountAsync(s => s.CurrentClassId == classId && s.Id != id);
        if (currentEnrollment >= cls.MaxCapacity)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassAtCapacity,
                $"Cannot assign student. Class is at full capacity ({cls.MaxCapacity}).");

        student.CurrentClassId = classId;
        await _studentRepository.UpdateAsync(student);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_Edit)]
    public async Task ActivateAsync(Guid id)
    {
        var student = await _studentRepository.GetAsync(id);
        student.IsActive = true;
        await _studentRepository.UpdateAsync(student);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_Edit)]
    public async Task DeactivateAsync(Guid id)
    {
        var student = await _studentRepository.GetAsync(id);
        student.IsActive = false;
        await _studentRepository.UpdateAsync(student);
    }
}
