using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Attendances.Dto;
using psms.Academic.Shared;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Academic.Attendances;

[AbpAuthorize(PermissionNames.Academic_Attendance)]
public class AttendanceAppService : ApplicationService, IAttendanceAppService
{
    private readonly IRepository<Attendance, Guid> _attendanceRepository;
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<Class, Guid> _classRepository;
    private readonly IRepository<Teacher, Guid> _teacherRepository;
    private readonly psms.Academic.Students.ICurrentStudentResolver _currentStudent;

    public AttendanceAppService(
        IRepository<Attendance, Guid> attendanceRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<Class, Guid> classRepository,
        IRepository<Teacher, Guid> teacherRepository,
        psms.Academic.Students.ICurrentStudentResolver currentStudent)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _teacherRepository = teacherRepository;
        _currentStudent = currentStudent;
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_View)]
    public async Task<AttendanceDto> GetAsync(Guid id)
    {
        var attendance = await _attendanceRepository
            .GetAll()
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Include(a => a.Teacher)
            .Include(a => a.Subject)
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (attendance == null)
            throw new UserFriendlyException(AcademicExceptionCodes.AttendanceNotFound, "Attendance record not found.");

        // LC-10: a student may only read their own attendance.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != attendance.StudentId)
            throw new UserFriendlyException(AcademicExceptionCodes.AttendanceNotFound, "Attendance record not found.");

        return ObjectMapper.Map<AttendanceDto>(attendance);
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_View)]
    public async Task<PagedResultDto<AttendanceListDto>> GetAllAsync(GetAttendanceInput input)
    {
        // LC-10: a student-portal user only ever sees their own attendance.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();

        var query = _attendanceRepository
            .GetAll()
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Where(a => a.TenantId == AbpSession.TenantId)
            .WhereIf(selfId.HasValue, a => a.StudentId == selfId.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                a => a.Student.FirstName.ToLower().Contains(input.Keyword.ToLower())
                  || a.Student.LastName.ToLower().Contains(input.Keyword.ToLower())
                  || a.Class.ClassName.ToLower().Contains(input.Keyword.ToLower()))
            .WhereIf(input.ClassId.HasValue, a => a.ClassId == input.ClassId.Value)
            .WhereIf(input.StudentId.HasValue, a => a.StudentId == input.StudentId.Value)
            .WhereIf(input.TeacherId.HasValue, a => a.TeacherId == input.TeacherId.Value)
            .WhereIf(input.StartDate.HasValue, a => a.AttendanceDate.Date >= input.StartDate.Value.Date)
            .WhereIf(input.EndDate.HasValue, a => a.AttendanceDate.Date <= input.EndDate.Value.Date)
            .WhereIf(input.Status.HasValue, a => a.Status == input.Status.Value);

        var totalCount = await query.CountAsync();

        var records = await query
            .OrderBy(input.Sorting ?? "AttendanceDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<AttendanceListDto>(
            totalCount,
            ObjectMapper.Map<List<AttendanceListDto>>(records));
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_View)]
    public async Task<ListResultDto<AttendanceListDto>> GetByStudentAsync(Guid studentId, DateTime? startDate, DateTime? endDate)
    {
        // LC-10: a student may only read their own attendance.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        if (selfId.HasValue && selfId.Value != studentId)
            return new ListResultDto<AttendanceListDto>(new System.Collections.Generic.List<AttendanceListDto>());

        var records = await _attendanceRepository
            .GetAll()
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Where(a => a.StudentId == studentId && a.TenantId == AbpSession.TenantId)
            .WhereIf(startDate.HasValue, a => a.AttendanceDate.Date >= startDate.Value.Date)
            .WhereIf(endDate.HasValue, a => a.AttendanceDate.Date <= endDate.Value.Date)
            .OrderByDescending(a => a.AttendanceDate)
            .ToListAsync();

        return new ListResultDto<AttendanceListDto>(
            ObjectMapper.Map<List<AttendanceListDto>>(records));
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_View)]
    public async Task<ListResultDto<AttendanceListDto>> GetByClassAndDateAsync(Guid classId, DateTime date)
    {
        // LC-10: a student must not read a class register — restrict to their own row.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();

        var records = await _attendanceRepository
            .GetAll()
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Where(a => a.ClassId == classId
                && a.AttendanceDate.Date == date.Date
                && a.TenantId == AbpSession.TenantId)
            .WhereIf(selfId.HasValue, a => a.StudentId == selfId.Value)
            .OrderBy(a => a.Student.LastName)
            .ThenBy(a => a.Student.FirstName)
            .ToListAsync();

        return new ListResultDto<AttendanceListDto>(
            ObjectMapper.Map<List<AttendanceListDto>>(records));
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_Capture)]
    public async Task<AttendanceDto> CaptureAsync(CaptureAttendanceDto input)
    {
        await ValidateAttendanceInput(input.StudentId, input.ClassId, input.TeacherId, input.AttendanceDate, input.SubjectId);

        var attendance = new Attendance(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.StudentId,
            input.ClassId,
            input.TeacherId,
            input.AttendanceDate,
            input.Status)
        {
            Notes = input.Notes,
            SubjectId = input.SubjectId
        };

        await _attendanceRepository.InsertAsync(attendance);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(attendance.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_Capture)]
    public async Task<ListResultDto<AttendanceDto>> BulkCaptureAsync(BulkCaptureAttendanceDto input)
    {
        // Date guard: not future, and (AT-002) not a locked past date.
        // Shared with single CaptureAsync so the lock can't be bypassed by
        // posting one student at a time.
        await EnsureDateCapturableOrThrowAsync(input.AttendanceDate);

        // Validate class exists
        var cls = await _classRepository
            .FirstOrDefaultAsync(c => c.Id == input.ClassId && c.TenantId == AbpSession.TenantId);

        if (cls == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotFound, "Class not found.");

        // Validate teacher exists
        var teacher = await _teacherRepository
            .FirstOrDefaultAsync(t => t.Id == input.TeacherId && t.TenantId == AbpSession.TenantId);

        if (teacher == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Teacher not found.");

        // Fail-fast: validate all students belong to this class
        var studentIds = input.Entries.Select(e => e.StudentId).ToList();
        var studentsInClass = await _studentRepository
            .GetAll()
            .Where(s => studentIds.Contains(s.Id)
                && s.CurrentClassId == input.ClassId
                && s.TenantId == AbpSession.TenantId)
            .Select(s => s.Id)
            .ToListAsync();

        var missingStudents = studentIds.Except(studentsInClass).ToList();
        if (missingStudents.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.AttendanceStudentNotInClass,
                $"The following student(s) are not in this class: {string.Join(", ", missingStudents)}.");

        // Batch duplicate check
        var existingRecords = await _attendanceRepository
            .GetAll()
            .Where(a => a.ClassId == input.ClassId
                && a.AttendanceDate.Date == input.AttendanceDate.Date
                && a.SubjectId == input.SubjectId
                && studentIds.Contains(a.StudentId)
                && a.TenantId == AbpSession.TenantId)
            .Select(a => a.StudentId)
            .ToListAsync();

        if (existingRecords.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateAttendance,
                $"Attendance already recorded for {existingRecords.Count} student(s) on this date.");

        var createdIds = new List<Guid>();

        foreach (var entry in input.Entries)
        {
            var attendance = new Attendance(
                Guid.NewGuid(),
                AbpSession.TenantId,
                entry.StudentId,
                input.ClassId,
                input.TeacherId,
                input.AttendanceDate,
                entry.Status)
            {
                Notes = entry.Notes,
                SubjectId = input.SubjectId
            };

            await _attendanceRepository.InsertAsync(attendance);
            createdIds.Add(attendance.Id);
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        var created = await _attendanceRepository
            .GetAll()
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Include(a => a.Teacher)
            .Include(a => a.Subject)
            .Where(a => createdIds.Contains(a.Id) && a.TenantId == AbpSession.TenantId)
            .ToListAsync();

        return new ListResultDto<AttendanceDto>(
            ObjectMapper.Map<List<AttendanceDto>>(created));
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_Edit)]
    public async Task<AttendanceDto> UpdateAsync(Guid id, UpdateAttendanceDto input)
    {
        var attendance = await _attendanceRepository
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (attendance == null)
            throw new UserFriendlyException(AcademicExceptionCodes.AttendanceNotFound, "Attendance record not found.");

        if (input.Status.HasValue) attendance.Status = input.Status.Value;
        if (input.Notes != null) attendance.Notes = input.Notes;

        await _attendanceRepository.UpdateAsync(attendance);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_Edit)]
    public async Task DeleteAsync(Guid id)
    {
        var attendance = await _attendanceRepository
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == AbpSession.TenantId);

        if (attendance == null)
            throw new UserFriendlyException(AcademicExceptionCodes.AttendanceNotFound, "Attendance record not found.");

        await _attendanceRepository.DeleteAsync(attendance);
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_Reports)]
    public async Task<AttendanceSummaryDto> GetStudentSummaryAsync(Guid studentId, DateTime startDate, DateTime endDate)
    {
        var student = await _studentRepository
            .FirstOrDefaultAsync(s => s.Id == studentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");

        var records = await _attendanceRepository
            .GetAll()
            .Where(a => a.StudentId == studentId
                && a.AttendanceDate.Date >= startDate.Date
                && a.AttendanceDate.Date <= endDate.Date
                && a.TenantId == AbpSession.TenantId)
            .ToListAsync();

        return BuildSummary(studentId, student.GetFullName(), records);
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_Reports)]
    public async Task<ListResultDto<AttendanceSummaryDto>> GetClassSummaryAsync(Guid classId, DateTime startDate, DateTime endDate)
    {
        var records = await _attendanceRepository
            .GetAll()
            .Include(a => a.Student)
            .Where(a => a.ClassId == classId
                && a.AttendanceDate.Date >= startDate.Date
                && a.AttendanceDate.Date <= endDate.Date
                && a.TenantId == AbpSession.TenantId)
            .ToListAsync();

        var summaries = records
            .GroupBy(a => new { a.StudentId, StudentName = a.Student.GetFullName() })
            .Select(g => BuildSummary(g.Key.StudentId, g.Key.StudentName, g.ToList()))
            .OrderBy(s => s.StudentName)
            .ToList();

        return new ListResultDto<AttendanceSummaryDto>(summaries);
    }

    /// <summary>
    /// Date guard shared by single and bulk capture: the date may not be in
    /// the future, and (AT-002 midnight lock) once the calendar day has
    /// passed a regular teacher can no longer capture that date — only a user
    /// with the admin-level ViewAll permission may backfill/correct a past
    /// date. (Uses the same server-local DateTime.Today convention as the
    /// rest of this service.)
    /// </summary>
    private async Task EnsureDateCapturableOrThrowAsync(DateTime attendanceDate)
    {
        if (attendanceDate.Date > DateTime.Today)
            throw new UserFriendlyException(AcademicExceptionCodes.AttendanceDateInFuture,
                "Attendance date cannot be in the future.");

        if (attendanceDate.Date < DateTime.Today
            && !await PermissionChecker.IsGrantedAsync(PermissionNames.Academic_Attendance_ViewAll))
            throw new UserFriendlyException(AcademicExceptionCodes.AttendanceLocked,
                "Attendance for a past date is locked. Ask an administrator to capture or correct it.");
    }

    private async Task ValidateAttendanceInput(Guid studentId, Guid classId, Guid teacherId, DateTime attendanceDate, Guid? subjectId)
    {
        // Date guard: not future, and (AT-002) not a locked past date.
        await EnsureDateCapturableOrThrowAsync(attendanceDate);

        // Validate student exists and belongs to class
        var student = await _studentRepository
            .FirstOrDefaultAsync(s => s.Id == studentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");

        if (student.CurrentClassId != classId)
            throw new UserFriendlyException(AcademicExceptionCodes.AttendanceStudentNotInClass,
                "Student does not belong to the specified class.");

        // Validate class exists
        var cls = await _classRepository
            .FirstOrDefaultAsync(c => c.Id == classId && c.TenantId == AbpSession.TenantId);

        if (cls == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotFound, "Class not found.");

        // Validate teacher exists
        var teacher = await _teacherRepository
            .FirstOrDefaultAsync(t => t.Id == teacherId && t.TenantId == AbpSession.TenantId);

        if (teacher == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Teacher not found.");

        // Duplicate prevention: one record per student+date+subjectId
        var duplicate = await _attendanceRepository
            .FirstOrDefaultAsync(a => a.StudentId == studentId
                && a.AttendanceDate.Date == attendanceDate.Date
                && a.SubjectId == subjectId
                && a.TenantId == AbpSession.TenantId);

        if (duplicate != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateAttendance,
                "Attendance has already been recorded for this student on this date.");
    }

    private static AttendanceSummaryDto BuildSummary(Guid studentId, string studentName, List<Attendance> records)
    {
        return new AttendanceSummaryDto
        {
            StudentId = studentId,
            StudentName = studentName,
            TotalDays = records.Count,
            PresentCount = records.Count(r => r.Status == AttendanceStatus.Present),
            AbsentCount = records.Count(r => r.Status == AttendanceStatus.Absent),
            LateCount = records.Count(r => r.Status == AttendanceStatus.Late),
            ExcusedCount = records.Count(r => r.Status == AttendanceStatus.Excused),
            SickLeaveCount = records.Count(r => r.Status == AttendanceStatus.SickLeave),
            AttendancePercentage = records.Count > 0
                ? Math.Round(
                    (decimal)records.Count(r => r.Status == AttendanceStatus.Present || r.Status == AttendanceStatus.Late)
                    / records.Count * 100, 2)
                : 0
        };
    }
}
