using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.StudentClasses.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Academic.StudentClasses;

[AbpAuthorize(PermissionNames.Academic_Students)]
public class StudentClassAppService : ApplicationService, IStudentClassAppService
{
    private readonly IRepository<StudentClass, Guid> _studentClassRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<Class, Guid> _classRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;
    private readonly psms.Academic.Students.ICurrentStudentResolver _currentStudent;

    public StudentClassAppService(
        IRepository<StudentClass, Guid> studentClassRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<Class, Guid> classRepository,
        IRepository<AcademicYear, Guid> academicYearRepository,
        psms.Academic.Students.ICurrentStudentResolver currentStudent)
    {
        _studentClassRepository = studentClassRepository;
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _academicYearRepository = academicYearRepository;
        _currentStudent = currentStudent;
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<StudentClassDto> GetAsync(Guid id)
    {
        var studentClass = await _studentClassRepository
            .GetAll()
            .Include(sc => sc.Student)
            .Include(sc => sc.Class)
            .Include(sc => sc.AcademicYear)
            .FirstOrDefaultAsync(sc => sc.Id == id && sc.TenantId == AbpSession.TenantId);

        if (studentClass == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentClassNotFound, "Student class enrollment not found.");

        // LC-08: a student may only read their own enrollment rows.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != studentClass.StudentId)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentClassNotFound, "Student class enrollment not found.");

        return ObjectMapper.Map<StudentClassDto>(studentClass);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<ListResultDto<StudentClassListDto>> GetByStudentAsync(Guid studentId)
    {
        // LC-08: a student may only read their own enrollment history.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != studentId)
            return new ListResultDto<StudentClassListDto>(new System.Collections.Generic.List<StudentClassListDto>());

        var records = await _studentClassRepository
            .GetAll()
            .Include(sc => sc.Student)
            .Include(sc => sc.Class)
            .Include(sc => sc.AcademicYear)
            .Where(sc => sc.StudentId == studentId && sc.TenantId == AbpSession.TenantId)
            .OrderByDescending(sc => sc.EnrollmentDate)
            .ToListAsync();

        return new ListResultDto<StudentClassListDto>(
            ObjectMapper.Map<List<StudentClassListDto>>(records));
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<ListResultDto<StudentClassListDto>> GetByClassAsync(Guid classId)
    {
        // LC-08: a student must not enumerate a class roster — restrict the
        // result to their own enrollment row in that class.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();

        var records = await _studentClassRepository
            .GetAll()
            .Include(sc => sc.Student)
            .Include(sc => sc.Class)
            .Include(sc => sc.AcademicYear)
            .Where(sc => sc.ClassId == classId && sc.TenantId == AbpSession.TenantId)
            .WhereIf(selfId.HasValue, sc => sc.StudentId == selfId.Value)
            .OrderBy(sc => sc.Student.LastName)
            .ThenBy(sc => sc.Student.FirstName)
            .ToListAsync();

        return new ListResultDto<StudentClassListDto>(
            ObjectMapper.Map<List<StudentClassListDto>>(records));
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<StudentClassDto> GetCurrentByStudentAsync(Guid studentId)
    {
        // LC-08: a student may only read their own current enrollment.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != studentId)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentClassNotFound, "No current class enrollment found for this student.");

        var current = await _studentClassRepository
            .GetAll()
            .Include(sc => sc.Student)
            .Include(sc => sc.Class)
            .Include(sc => sc.AcademicYear)
            .FirstOrDefaultAsync(sc => sc.StudentId == studentId
                && sc.IsCurrent
                && sc.TenantId == AbpSession.TenantId);

        if (current == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentClassNotFound, "No current class enrollment found for this student.");

        return ObjectMapper.Map<StudentClassDto>(current);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_AssignClass)]
    public async Task<StudentClassDto> EnrollAsync(CreateStudentClassDto input)
    {
        // Validate enrollment date is not in the future
        if (input.EnrollmentDate.Date > DateTime.Today)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidEnrollmentDate,
                "Enrollment date cannot be in the future.");

        // Validate student exists and belongs to tenant
        var student = await _studentRepository
            .FirstOrDefaultAsync(s => s.Id == input.StudentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");

        // Validate class exists and is active
        var cls = await _classRepository
            .FirstOrDefaultAsync(c => c.Id == input.ClassId && c.TenantId == AbpSession.TenantId);

        if (cls == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotFound, "Class not found.");
        if (!cls.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotActive, "Class is not active.");

        // Validate academic year exists
        var academicYear = await _academicYearRepository
            .FirstOrDefaultAsync(ay => ay.Id == input.AcademicYearId && ay.TenantId == AbpSession.TenantId);

        if (academicYear == null)
            throw new UserFriendlyException(AcademicExceptionCodes.AcademicYearNotFound, "Academic year not found.");

        // Check class capacity
        var currentEnrollment = await _studentClassRepository
            .CountAsync(sc => sc.ClassId == input.ClassId
                && sc.IsActive
                && sc.TenantId == AbpSession.TenantId);

        if (currentEnrollment >= cls.MaxCapacity)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassAtCapacity,
                $"Cannot enroll student. Class is at full capacity ({cls.MaxCapacity}).");

        // Check duplicate enrollment (student + class + academic year)
        var duplicate = await _studentClassRepository
            .FirstOrDefaultAsync(sc => sc.StudentId == input.StudentId
                && sc.ClassId == input.ClassId
                && sc.AcademicYearId == input.AcademicYearId
                && sc.TenantId == AbpSession.TenantId);

        if (duplicate != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateStudentClassEnrollment,
                "Student is already enrolled in this class for this academic year.");

        // Check if student is already actively enrolled in another class for this academic year
        var existingActive = await _studentClassRepository
            .FirstOrDefaultAsync(sc => sc.StudentId == input.StudentId
                && sc.AcademicYearId == input.AcademicYearId
                && sc.IsActive
                && sc.IsCurrent
                && sc.TenantId == AbpSession.TenantId);

        var studentClass = new StudentClass(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.StudentId,
            input.ClassId,
            input.AcademicYearId,
            input.EnrollmentDate);

        // If there is an existing current enrollment, unset it
        if (existingActive != null)
        {
            existingActive.IsCurrent = false;
            await _studentClassRepository.UpdateAsync(existingActive);
        }

        await _studentClassRepository.InsertAsync(studentClass);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(studentClass.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_Edit)]
    public async Task<StudentClassDto> UpdateAsync(Guid id, UpdateStudentClassDto input)
    {
        var studentClass = await _studentClassRepository
            .FirstOrDefaultAsync(sc => sc.Id == id && sc.TenantId == AbpSession.TenantId);

        if (studentClass == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentClassNotFound, "Student class enrollment not found.");

        if (input.IsActive.HasValue) studentClass.IsActive = input.IsActive.Value;

        await _studentClassRepository.UpdateAsync(studentClass);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_Edit)]
    public async Task<StudentClassDto> EndEnrollmentAsync(Guid id, DateTime endDate)
    {
        var studentClass = await _studentClassRepository
            .FirstOrDefaultAsync(sc => sc.Id == id && sc.TenantId == AbpSession.TenantId);

        if (studentClass == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentClassNotFound, "Student class enrollment not found.");

        // Validate end date >= enrollment date
        if (endDate.Date < studentClass.EnrollmentDate.Date)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidEndDate,
                "End date cannot be before the enrollment date.");

        studentClass.EndEnrollment(endDate);

        await _studentClassRepository.UpdateAsync(studentClass);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_AssignClass)]
    public async Task<StudentClassDto> SetAsCurrentAsync(Guid id)
    {
        var studentClass = await _studentClassRepository
            .FirstOrDefaultAsync(sc => sc.Id == id && sc.TenantId == AbpSession.TenantId);

        if (studentClass == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentClassNotFound, "Student class enrollment not found.");

        if (studentClass.IsCurrent)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentClassAlreadyCurrent,
                "This enrollment is already marked as current.");

        // Unset all other current records for this student
        var currentRecords = await _studentClassRepository
            .GetAll()
            .Where(sc => sc.StudentId == studentClass.StudentId
                && sc.IsCurrent
                && sc.Id != id
                && sc.TenantId == AbpSession.TenantId)
            .ToListAsync();

        foreach (var record in currentRecords)
        {
            record.IsCurrent = false;
            await _studentClassRepository.UpdateAsync(record);
        }

        studentClass.SetAsCurrent();

        await _studentClassRepository.UpdateAsync(studentClass);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var studentClass = await _studentClassRepository
            .FirstOrDefaultAsync(sc => sc.Id == id && sc.TenantId == AbpSession.TenantId);

        if (studentClass == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentClassNotFound, "Student class enrollment not found.");

        await _studentClassRepository.DeleteAsync(studentClass);
    }
}
