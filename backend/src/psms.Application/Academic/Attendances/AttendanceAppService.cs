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
    private readonly IRepository<TeacherClass, Guid> _teacherClassRepository;
    private readonly psms.Academic.Students.ICurrentStudentResolver _currentStudent;
    private readonly psms.Academic.Parents.ICurrentParentResolver _currentParent;

    public AttendanceAppService(
        IRepository<Attendance, Guid> attendanceRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<Class, Guid> classRepository,
        IRepository<Teacher, Guid> teacherRepository,
        IRepository<TeacherClass, Guid> teacherClassRepository,
        psms.Academic.Students.ICurrentStudentResolver currentStudent,
        psms.Academic.Parents.ICurrentParentResolver currentParent)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _teacherRepository = teacherRepository;
        _teacherClassRepository = teacherClassRepository;
        _currentStudent = currentStudent;
        _currentParent = currentParent;
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

        // MOB-BE-04: a parent may only read their own children's attendance.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(attendance.StudentId))
            throw new UserFriendlyException(AcademicExceptionCodes.AttendanceNotFound, "Attendance record not found.");

        return ObjectMapper.Map<AttendanceDto>(attendance);
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_View)]
    public async Task<PagedResultDto<AttendanceListDto>> GetAllAsync(GetAttendanceInput input)
    {
        // LC-10: a student-portal user only ever sees their own attendance.
        var selfId = await _currentStudent.GetCurrentStudentIdAsync();
        // MOB-BE-04: a parent only ever sees their own children's attendance.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();

        DateTime? startUtc = input.StartDate.HasValue ? LocalDayRangeUtc(input.StartDate.Value).StartUtc : null;
        DateTime? endUtcExclusive = input.EndDate.HasValue ? LocalDayRangeUtc(input.EndDate.Value).EndUtc : null;

        var query = _attendanceRepository
            .GetAll()
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Where(a => a.TenantId == AbpSession.TenantId)
            .WhereIf(selfId.HasValue, a => a.StudentId == selfId.Value)
            .WhereIf(childIds != null, a => childIds.Contains(a.StudentId))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                a => a.Student.FirstName.ToLower().Contains(input.Keyword.ToLower())
                  || a.Student.LastName.ToLower().Contains(input.Keyword.ToLower())
                  || a.Class.ClassName.ToLower().Contains(input.Keyword.ToLower()))
            .WhereIf(input.ClassId.HasValue, a => a.ClassId == input.ClassId.Value)
            .WhereIf(input.StudentId.HasValue, a => a.StudentId == input.StudentId.Value)
            .WhereIf(input.TeacherId.HasValue, a => a.TeacherId == input.TeacherId.Value)
            .WhereIf(startUtc.HasValue, a => a.AttendanceDate >= startUtc!.Value)
            .WhereIf(endUtcExclusive.HasValue, a => a.AttendanceDate < endUtcExclusive!.Value)
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

        // MOB-BE-04: a parent may only read their own children's attendance.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(studentId))
            return new ListResultDto<AttendanceListDto>(new System.Collections.Generic.List<AttendanceListDto>());

        DateTime? startUtc = startDate.HasValue ? LocalDayRangeUtc(startDate.Value).StartUtc : null;
        DateTime? endUtcExclusive = endDate.HasValue ? LocalDayRangeUtc(endDate.Value).EndUtc : null;

        var records = await _attendanceRepository
            .GetAll()
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Where(a => a.StudentId == studentId && a.TenantId == AbpSession.TenantId)
            .WhereIf(startUtc.HasValue, a => a.AttendanceDate >= startUtc!.Value)
            .WhereIf(endUtcExclusive.HasValue, a => a.AttendanceDate < endUtcExclusive!.Value)
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
        // MOB-BE-04: a parent must not read a class register — restrict to
        // their own children's rows.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();

        var (startUtc, endUtc) = LocalDayRangeUtc(date);

        var records = await _attendanceRepository
            .GetAll()
            .Include(a => a.Student)
            .Include(a => a.Class)
            .Where(a => a.ClassId == classId
                && a.AttendanceDate >= startUtc && a.AttendanceDate < endUtc
                && a.TenantId == AbpSession.TenantId)
            .WhereIf(selfId.HasValue, a => a.StudentId == selfId.Value)
            .WhereIf(childIds != null, a => childIds.Contains(a.StudentId))
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
        // The app-level duplicate check above is a plain read-then-write —
        // two near-simultaneous requests for the same student+date can both
        // pass it. IX_Attendances_StudentId_AttendanceDate is the real
        // backstop; catch its violation here so a race surfaces the same
        // friendly message as the check that usually catches it first,
        // instead of a raw 500.
        try
        {
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateAttendance,
                "Attendance has already been recorded for this student on this date.");
        }

        return await GetAsync(attendance.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_Capture)]
    public async Task<ListResultDto<AttendanceDto>> BulkCaptureAsync(BulkCaptureAttendanceDto input)
    {
        // Date guard: not future, and (AT-002) not a locked past date.
        // Shared with single CaptureAsync so the lock can't be bypassed by
        // posting one student at a time.
        await EnsureDateCapturableOrThrowAsync(input.AttendanceDate);

        // T-T21: the calling teacher must actually be assigned to this class.
        await EnsureTeacherOwnsClassAsync(input.ClassId);

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
        var (bulkDayStartUtc, bulkDayEndUtc) = LocalDayRangeUtc(input.AttendanceDate);
        var existingRecords = await _attendanceRepository
            .GetAll()
            .Where(a => a.ClassId == input.ClassId
                && a.AttendanceDate >= bulkDayStartUtc && a.AttendanceDate < bulkDayEndUtc
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

        // Same TOCTOU race as CaptureAsync — see the comment there.
        try
        {
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateAttendance,
                "Attendance was recorded for one or more of these students in the moment between the check and save. Refresh and try again.");
        }

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

        // T-T21: same-day correction is allowed (AttendanceDate == Today
        // never trips this), but from the next day onward only an admin
        // with ViewAll may correct it — the same boundary Capture uses, so
        // a row can't be edited after its lock via this endpoint either.
        await EnsureDateCapturableOrThrowAsync(attendance.AttendanceDate);

        // T-T21: and only for a class the calling teacher is assigned to —
        // otherwise Edit alone would let any teacher correct any other
        // teacher's class register.
        await EnsureTeacherOwnsClassAsync(attendance.ClassId);

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

        // Same lock + ownership boundaries as UpdateAsync — see T-T21 comments there.
        await EnsureDateCapturableOrThrowAsync(attendance.AttendanceDate);
        await EnsureTeacherOwnsClassAsync(attendance.ClassId);

        await _attendanceRepository.DeleteAsync(attendance);
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_Reports)]
    public async Task<AttendanceSummaryDto> GetStudentSummaryAsync(Guid studentId, DateTime startDate, DateTime endDate)
    {
        var student = await _studentRepository
            .FirstOrDefaultAsync(s => s.Id == studentId && s.TenantId == AbpSession.TenantId);

        if (student == null)
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");

        // MOB-BE-04: this report is reachable by the Parent role
        // (Academic_Attendance_Reports) — a parent may only summarise their
        // own children's attendance, not an arbitrary student's.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        if (childIds != null && !childIds.Contains(studentId))
            throw new UserFriendlyException(AcademicExceptionCodes.StudentNotFound, "Student not found.");

        var summaryStartUtc = LocalDayRangeUtc(startDate).StartUtc;
        var summaryEndUtcExclusive = LocalDayRangeUtc(endDate).EndUtc;
        var records = await _attendanceRepository
            .GetAll()
            .Where(a => a.StudentId == studentId
                && a.AttendanceDate >= summaryStartUtc
                && a.AttendanceDate < summaryEndUtcExclusive
                && a.TenantId == AbpSession.TenantId)
            .ToListAsync();

        return BuildSummary(studentId, student.GetFullName(), records);
    }

    [AbpAuthorize(PermissionNames.Academic_Attendance_Reports)]
    public async Task<ListResultDto<AttendanceSummaryDto>> GetClassSummaryAsync(Guid classId, DateTime startDate, DateTime endDate)
    {
        var classSummaryStartUtc = LocalDayRangeUtc(startDate).StartUtc;
        var classSummaryEndUtcExclusive = LocalDayRangeUtc(endDate).EndUtc;

        // MOB-BE-04: this report is reachable by the Parent role
        // (Academic_Attendance_Reports) — a parent must not enumerate a whole
        // class's attendance, only their own children's rows within it.
        var childIds = await _currentParent.GetCurrentChildStudentIdsAsync();
        var records = await _attendanceRepository
            .GetAll()
            .Include(a => a.Student)
            .Where(a => a.ClassId == classId
                && a.AttendanceDate >= classSummaryStartUtc
                && a.AttendanceDate < classSummaryEndUtcExclusive
                && a.TenantId == AbpSession.TenantId)
            .WhereIf(childIds != null, a => childIds.Contains(a.StudentId))
            .ToListAsync();

        var summaries = records
            .GroupBy(a => new { a.StudentId, StudentName = a.Student.GetFullName() })
            .Select(g => BuildSummary(g.Key.StudentId, g.Key.StudentName, g.ToList()))
            .OrderBy(s => s.StudentName)
            .ToList();

        return new ListResultDto<AttendanceSummaryDto>(summaries);
    }

    /// <summary>
    /// Date guard shared by single/bulk capture AND update/delete (T-T21):
    /// the date may not be in the future, and (AT-002 midnight lock) once
    /// the calendar day has passed a regular teacher can no longer touch
    /// that date — only a user with the admin-level ViewAll permission may
    /// backfill/correct a past date. Same-day is always capturable, so
    /// same-day correction (T-T21) falls out of this for free.
    /// </summary>
    private async Task EnsureDateCapturableOrThrowAsync(DateTime attendanceDate)
    {
        var localDate = NormalizeToLocalDate(attendanceDate);

        if (localDate > DateTime.Today)
            throw new UserFriendlyException(AcademicExceptionCodes.AttendanceDateInFuture,
                "Attendance date cannot be in the future.");

        if (localDate < DateTime.Today
            && !await PermissionChecker.IsGrantedAsync(PermissionNames.Academic_Attendance_ViewAll))
            throw new UserFriendlyException(AcademicExceptionCodes.AttendanceLocked,
                "Attendance for a past date is locked. Ask an administrator to capture or correct it.");
    }

    /// <summary>
    /// T-T21: a plain teacher may only capture/correct attendance for a
    /// class they are actually assigned to (as a subject teacher or the
    /// class/register teacher) — Capture/Edit alone say nothing about
    /// *which* class, so without this any teacher holding either
    /// permission could tamper with a class they have no relationship to.
    /// Admins/principals with the ViewAll permission are exempt — they
    /// legitimately need to backfill/correct any class's register.
    /// </summary>
    private async Task EnsureTeacherOwnsClassAsync(Guid classId)
    {
        if (await PermissionChecker.IsGrantedAsync(PermissionNames.Academic_Attendance_ViewAll))
            return;

        if (AbpSession.UserId == null) return;

        var teacher = await _teacherRepository
            .FirstOrDefaultAsync(t => t.UserId == AbpSession.UserId.Value && t.TenantId == AbpSession.TenantId);
        // Not a teacher-linked account (e.g. a student/parent portal user
        // who somehow holds the permission) — nothing to scope by class.
        if (teacher == null) return;

        var assigned = await _teacherClassRepository
            .GetAll()
            .AnyAsync(tc => tc.TeacherId == teacher.Id && tc.ClassId == classId);

        if (!assigned)
            throw new UserFriendlyException(AcademicExceptionCodes.AttendanceClassNotAssigned,
                "You are not assigned to this class.");
    }

    /// <summary>
    /// AttendanceDate is stored in a `timestamptz` column, so an entity
    /// freshly built from a client's date-only string (Kind=Unspecified,
    /// e.g. Capture/BulkCapture's input) and one just reloaded from the
    /// database (Kind=Utc, shifted by the server's local offset — T-T21's
    /// Update/Delete guard hits this) are not directly comparable via a
    /// bare `.Date`. Converting only the Utc-kind case back to local before
    /// truncating keeps both call sites correct without special-casing.
    /// </summary>
    private static DateTime NormalizeToLocalDate(DateTime value)
        => value.Kind == DateTimeKind.Utc ? value.ToLocalTime().Date : value.Date;

    /// <summary>
    /// AttendanceDate is stored in a `timestamptz` column. EF Core
    /// translates `a.AttendanceDate.Date` (an entity property access)
    /// into a Postgres-side date truncation in the session's timezone
    /// (UTC) rather than the server's local offset the date was captured
    /// against — while a client-supplied calendar date on the other side
    /// of the comparison is evaluated as a plain local value. The two
    /// sides silently stop matching (a historical register lookup for a
    /// date that has real data comes back empty). Converting the
    /// requested local calendar date into its UTC instant range and
    /// comparing the raw column against that range sidesteps the
    /// translation ambiguity entirely — used by every query below that
    /// filters on AttendanceDate.
    /// </summary>
    private static (DateTime StartUtc, DateTime EndUtc) LocalDayRangeUtc(DateTime localDate)
    {
        var startLocal = DateTime.SpecifyKind(localDate.Date, DateTimeKind.Local);
        var startUtc = startLocal.ToUniversalTime();
        return (startUtc, startUtc.AddDays(1));
    }

    private async Task ValidateAttendanceInput(Guid studentId, Guid classId, Guid teacherId, DateTime attendanceDate, Guid? subjectId)
    {
        // Date guard: not future, and (AT-002) not a locked past date.
        await EnsureDateCapturableOrThrowAsync(attendanceDate);

        // T-T21: the calling teacher must actually be assigned to this class.
        await EnsureTeacherOwnsClassAsync(classId);

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
        var (dupDayStartUtc, dupDayEndUtc) = LocalDayRangeUtc(attendanceDate);
        var duplicate = await _attendanceRepository
            .FirstOrDefaultAsync(a => a.StudentId == studentId
                && a.AttendanceDate >= dupDayStartUtc && a.AttendanceDate < dupDayEndUtc
                && a.SubjectId == subjectId
                && a.TenantId == AbpSession.TenantId);

        if (duplicate != null)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateAttendance,
                "Attendance has already been recorded for this student on this date.");
    }

    // Mirrors LearningMaterialAppService's helper of the same name/shape —
    // this codebase doesn't have a shared place for it yet, so each service
    // that needs it keeps its own copy.
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
        {
            var typeName = inner.GetType().FullName;
            if (typeName == "Microsoft.Data.SqlClient.SqlException"
                || typeName == "System.Data.SqlClient.SqlException")
            {
                var numberValue = inner.GetType().GetProperty("Number")?.GetValue(inner);
                if (numberValue is int number && (number == 2601 || number == 2627))
                    return true;
            }
            else if (typeName == "Npgsql.PostgresException")
            {
                var sqlStateValue = inner.GetType().GetProperty("SqlState")?.GetValue(inner);
                if (sqlStateValue is string sqlState && sqlState == "23505")
                    return true;
            }
        }
        return false;
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
