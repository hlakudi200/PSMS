using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.StudentSubjects.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Academic.StudentSubjects;

/// <summary>
/// Service for managing student-subject enrollments.
/// </summary>
[AbpAuthorize(PermissionNames.Academic_Students)]
public class StudentSubjectAppService : ApplicationService, IStudentSubjectAppService
{
    private readonly IRepository<StudentSubject, Guid> _studentSubjectRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<Subject, Guid> _subjectRepository;
    private readonly IRepository<AcademicYear, Guid> _academicYearRepository;
    private readonly psms.Academic.Students.ICurrentStudentResolver _currentStudent;

    public StudentSubjectAppService(
        IRepository<StudentSubject, Guid> studentSubjectRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<Subject, Guid> subjectRepository,
        IRepository<AcademicYear, Guid> academicYearRepository,
        psms.Academic.Students.ICurrentStudentResolver currentStudent)
    {
        _studentSubjectRepository = studentSubjectRepository;
        _studentRepository = studentRepository;
        _subjectRepository = subjectRepository;
        _academicYearRepository = academicYearRepository;
        _currentStudent = currentStudent;
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<ListResultDto<StudentSubjectDto>> GetByStudentAsync(Guid studentId)
    {
        // LC-08: a student may only read their own subject enrollments.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != studentId)
            return new ListResultDto<StudentSubjectDto>(new List<StudentSubjectDto>());

        var enrollments = await _studentSubjectRepository
            .GetAll()
            .Include(ss => ss.Student)
            .Include(ss => ss.Subject)
            .Include(ss => ss.AcademicYear)
            .Where(ss => ss.Student.TenantId == AbpSession.TenantId)
            .Where(ss => ss.StudentId == studentId)
            .OrderBy(ss => ss.Subject.SubjectName)
            .ToListAsync();

        return new ListResultDto<StudentSubjectDto>(
            ObjectMapper.Map<List<StudentSubjectDto>>(enrollments));
    }

    [AbpAuthorize(PermissionNames.Academic_Subjects_View)]
    public async Task<ListResultDto<StudentSubjectDto>> GetBySubjectAsync(Guid subjectId)
    {
        var enrollments = await _studentSubjectRepository
            .GetAll()
            .Include(ss => ss.Student)
            .Include(ss => ss.Subject)
            .Include(ss => ss.AcademicYear)
            .Where(ss => ss.Student.TenantId == AbpSession.TenantId)
            .Where(ss => ss.SubjectId == subjectId && ss.IsActive)
            .OrderBy(ss => ss.Student.LastName)
            .ThenBy(ss => ss.Student.FirstName)
            .ToListAsync();

        return new ListResultDto<StudentSubjectDto>(
            ObjectMapper.Map<List<StudentSubjectDto>>(enrollments));
    }

    [AbpAuthorize(PermissionNames.Academic_Students_View)]
    public async Task<ListResultDto<StudentSubjectDto>> GetByStudentAndYearAsync(Guid studentId, Guid academicYearId)
    {
        // LC-08: a student may only read their own subject enrollments.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != studentId)
            return new ListResultDto<StudentSubjectDto>(new List<StudentSubjectDto>());

        var enrollments = await _studentSubjectRepository
            .GetAll()
            .Include(ss => ss.Student)
            .Include(ss => ss.Subject)
            .Include(ss => ss.AcademicYear)
            .Where(ss => ss.Student.TenantId == AbpSession.TenantId)
            .Where(ss => ss.StudentId == studentId && ss.AcademicYearId == academicYearId)
            .OrderBy(ss => ss.Subject.SubjectName)
            .ToListAsync();

        return new ListResultDto<StudentSubjectDto>(
            ObjectMapper.Map<List<StudentSubjectDto>>(enrollments));
    }

    [AbpAuthorize(PermissionNames.Academic_Students_Edit)]
    public async Task<StudentSubjectDto> EnrollAsync(EnrollStudentSubjectDto input)
    {
        // Validate student exists (join through Student for tenant isolation)
        var student = await _studentRepository.FirstOrDefaultAsync(s => s.Id == input.StudentId);
        if (student == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");
        if (!student.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotActive, "Student is not active.");

        // Validate subject exists and is active
        var subject = await _subjectRepository.FirstOrDefaultAsync(s => s.Id == input.SubjectId);
        if (subject == null)
            throw new UserFriendlyException(AcademicExceptionCodes.SubjectNotFound, "Subject not found.");
        if (!subject.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.SubjectNotActive, "Subject is not active.");

        // Validate academic year exists
        var academicYear = await _academicYearRepository.FirstOrDefaultAsync(ay => ay.Id == input.AcademicYearId);
        if (academicYear == null)
            throw new UserFriendlyException(AcademicExceptionCodes.AcademicYearNotFound, "Academic year not found.");

        // Check for duplicate enrollment (tenant-scoped through validated FKs)
        var existing = await _studentSubjectRepository
            .GetAll()
            .Where(ss => ss.Student.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(ss => ss.StudentId == input.StudentId
                && ss.SubjectId == input.SubjectId
                && ss.AcademicYearId == input.AcademicYearId);

        if (existing != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateStudentSubject,
                "This student is already enrolled in this subject for this academic year.");

        var enrollment = new StudentSubject(
            Guid.NewGuid(),
            input.StudentId,
            input.SubjectId,
            input.AcademicYearId,
            DateTime.UtcNow);

        await _studentSubjectRepository.InsertAsync(enrollment);
        await CurrentUnitOfWork.SaveChangesAsync();

        var saved = await _studentSubjectRepository
            .GetAll()
            .Include(ss => ss.Student)
            .Include(ss => ss.Subject)
            .Include(ss => ss.AcademicYear)
            .Where(ss => ss.Student.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(ss => ss.Id == enrollment.Id);

        return ObjectMapper.Map<StudentSubjectDto>(saved);
    }

    [AbpAuthorize(PermissionNames.Academic_Students_Edit)]
    public async Task UnenrollAsync(Guid id)
    {
        var enrollment = await _studentSubjectRepository
            .GetAll()
            .Include(ss => ss.Student)
            .Where(ss => ss.Student.TenantId == AbpSession.TenantId)
            .FirstOrDefaultAsync(ss => ss.Id == id);

        if (enrollment == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentSubjectNotFound,
                "Student-subject enrollment not found.");

        await _studentSubjectRepository.DeleteAsync(enrollment);
    }
}
