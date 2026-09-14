using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.Timetables.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Academic.Timetables;

[AbpAuthorize(PermissionNames.Academic_Timetables)]
public class TimetableAppService : ApplicationService, ITimetableAppService
{
    private readonly IRepository<Timetable, Guid> _timetableRepository;
    private readonly IRepository<Class, Guid> _classRepository;
    private readonly IRepository<ClassSubject, Guid> _classSubjectRepository;
    private readonly IRepository<TimetableSlot, Guid> _slotRepository;

    public TimetableAppService(
        IRepository<Timetable, Guid> timetableRepository,
        IRepository<Class, Guid> classRepository,
        IRepository<ClassSubject, Guid> classSubjectRepository,
        IRepository<TimetableSlot, Guid> slotRepository)
    {
        _timetableRepository = timetableRepository;
        _classRepository = classRepository;
        _classSubjectRepository = classSubjectRepository;
        _slotRepository = slotRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_View)]
    public async Task<TimetableDto> GetAsync(Guid id)
    {
        var timetable = await _timetableRepository
            .GetAll()
            .Include(t => t.Class)
            .Include(t => t.TimetableSlots)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "Timetable not found.");

        return ObjectMapper.Map<TimetableDto>(timetable);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_View)]
    public async Task<PagedResultDto<TimetableListDto>> GetAllAsync(GetAcademicEntityInput input)
    {
        var query = _timetableRepository
            .GetAll()
            .Include(t => t.Class)
            .Include(t => t.TimetableSlots)
            .Where(t => t.TenantId == AbpSession.TenantId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                t => t.Class.ClassName.ToLower().Contains(input.Keyword.ToLower()))
            .WhereIf(input.IsActive.HasValue,
                t => t.IsActive == input.IsActive.Value);

        var totalCount = await query.CountAsync();

        var timetables = await query
            .OrderBy(input.Sorting ?? "EffectiveDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<TimetableListDto>(
            totalCount,
            ObjectMapper.Map<List<TimetableListDto>>(timetables));
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_View)]
    public async Task<TimetableDto> GetByClassAsync(Guid classId)
    {
        var timetable = await _timetableRepository
            .GetAll()
            .Include(t => t.Class)
            .Include(t => t.TimetableSlots)
            .FirstOrDefaultAsync(t => t.ClassId == classId
                && t.IsActive
                && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "No active timetable found for this class.");

        return ObjectMapper.Map<TimetableDto>(timetable);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Create)]
    public async Task<TimetableDto> CreateAsync(CreateTimetableDto input)
    {
        // Validate class exists
        var cls = await _classRepository
            .FirstOrDefaultAsync(c => c.Id == input.ClassId && c.TenantId == AbpSession.TenantId);

        if (cls == null)
            throw new UserFriendlyException(AcademicExceptionCodes.ClassNotFound, "Class not found.");

        // Validate dates
        if (input.EndDate.HasValue && input.EndDate.Value.Date <= input.EffectiveDate.Date)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidTimetableDates,
                "End date must be after the effective date.");

        var timetable = new Timetable(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.ClassId,
            input.EffectiveDate)
        {
            EndDate = input.EndDate
        };

        // Create as draft — require explicit ActivateAsync to go live
        // (constructor defaults IsActive=true, but we don't want to silently
        // deactivate the existing active timetable while slots are being added)
        timetable.IsActive = false;

        await _timetableRepository.InsertAsync(timetable);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(timetable.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Edit)]
    public async Task<TimetableDto> UpdateAsync(Guid id, UpdateTimetableDto input)
    {
        var timetable = await _timetableRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "Timetable not found.");

        if (input.EffectiveDate.HasValue) timetable.EffectiveDate = input.EffectiveDate.Value;
        if (input.EndDate.HasValue) timetable.EndDate = input.EndDate.Value;

        // Validate dates after update
        if (timetable.EndDate.HasValue && timetable.EndDate.Value.Date <= timetable.EffectiveDate.Date)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidTimetableDates,
                "End date must be after the effective date.");

        await _timetableRepository.UpdateAsync(timetable);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var timetable = await _timetableRepository
            .GetAll()
            .Include(t => t.TimetableSlots)
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "Timetable not found.");

        if (timetable.TimetableSlots.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.CannotDeleteTimetableWithSlots,
                "Cannot delete a timetable that has time slots. Remove all slots first.");

        await _timetableRepository.DeleteAsync(timetable);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Publish)]
    public async Task<TimetableDto> ActivateAsync(Guid id)
    {
        var timetable = await _timetableRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "Timetable not found.");

        // Deactivate all other timetables for the same class
        var activeTimetables = await _timetableRepository
            .GetAll()
            .Where(t => t.ClassId == timetable.ClassId
                && t.IsActive
                && t.Id != id
                && t.TenantId == AbpSession.TenantId)
            .ToListAsync();

        foreach (var active in activeTimetables)
        {
            active.IsActive = false;
            await _timetableRepository.UpdateAsync(active);
        }

        timetable.IsActive = true;

        await _timetableRepository.UpdateAsync(timetable);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Edit)]
    public async Task<TimetableDto> DeactivateAsync(Guid id)
    {
        var timetable = await _timetableRepository
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "Timetable not found.");

        if (!timetable.IsActive)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotActive, "Timetable is already inactive.");

        timetable.IsActive = false;

        await _timetableRepository.UpdateAsync(timetable);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    /// <summary>
    /// Generates draft timetables for every active class in an academic year.
    /// Greedy, most-constrained-first scheduler: subjects with no teacher
    /// assigned and lessons that can't find a conflict-free slot are reported
    /// back rather than failing the whole run, so the principal can hand-fix
    /// them via the existing slot CRUD before activating.
    /// </summary>
    [AbpAuthorize(PermissionNames.Academic_Timetables_MakeVariations)]
    public async Task<GenerateTimetablesResultDto> GenerateAsync(GenerateTimetablesInput input)
    {
        var workingDays = (input.WorkingDays == null || input.WorkingDays.Count == 0)
            ? new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday }
            : input.WorkingDays.Distinct().ToList();

        var result = new GenerateTimetablesResultDto();

        var classes = await _classRepository
            .GetAll()
            .Where(c => c.AcademicYearId == input.AcademicYearId && c.IsActive && c.TenantId == AbpSession.TenantId)
            .ToListAsync();

        if (classes.Count == 0)
        {
            result.Message = "No active classes found for the selected academic year. Create classes for this year first.";
            return result;
        }

        var classIds = classes.Select(c => c.Id).ToList();

        var classSubjects = await _classSubjectRepository
            .GetAll()
            .Include(cs => cs.Subject)
            .Include(cs => cs.Teacher)
            .Where(cs => classIds.Contains(cs.ClassId) && cs.IsActive && cs.PeriodsPerWeek > 0
                && cs.TenantId == AbpSession.TenantId)
            .ToListAsync();

        if (classSubjects.Count == 0)
        {
            result.Message = "The classes in this academic year have no subjects with periods-per-week assigned. Set up ClassSubjects (with PeriodsPerWeek) before generating.";
            return result;
        }

        // Seed teacher-busy from every currently-active slot tenant-wide so a
        // generated draft never conflicts with a class's timetable that hasn't
        // been replaced yet (the principal may activate drafts one at a time).
        var activeSlots = await _slotRepository
            .GetAll()
            .Include(s => s.Timetable)
            .Where(s => s.Timetable.IsActive && s.Timetable.TenantId == AbpSession.TenantId)
            .Select(s => new { s.TeacherId, s.DayOfWeek, s.PeriodNumber })
            .ToListAsync();

        var teacherBusy = new HashSet<(Guid TeacherId, DayOfWeek Day, int Period)>(
            activeSlots.Select(s => (s.TeacherId, s.DayOfWeek, s.PeriodNumber)));

        var tasks = new List<LessonTask>();
        foreach (var cs in classSubjects)
        {
            var className = classes.First(c => c.Id == cs.ClassId).ClassName;
            result.TotalSlotsRequested += cs.PeriodsPerWeek;

            if (!cs.TeacherId.HasValue)
            {
                result.Unplaced.Add(new UnplacedLessonDto
                {
                    ClassId = cs.ClassId,
                    ClassName = className,
                    SubjectId = cs.SubjectId,
                    SubjectName = cs.Subject?.SubjectName,
                    Reason = "No teacher assigned",
                });
                continue;
            }

            tasks.Add(new LessonTask
            {
                ClassId = cs.ClassId,
                ClassName = className,
                SubjectId = cs.SubjectId,
                SubjectName = cs.Subject?.SubjectName,
                TeacherId = cs.TeacherId.Value,
                TeacherName = cs.Teacher?.GetFullName(),
                PeriodsPerWeek = cs.PeriodsPerWeek,
            });
        }

        // Most-constrained-first: busiest teachers, then heaviest subjects.
        var teacherLoad = tasks
            .GroupBy(t => t.TeacherId)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.PeriodsPerWeek));

        var orderedTasks = tasks
            .OrderByDescending(t => teacherLoad[t.TeacherId])
            .ThenByDescending(t => t.PeriodsPerWeek)
            .ToList();

        var classBusy = new HashSet<(Guid ClassId, DayOfWeek Day, int Period)>();
        var classSubjectDay = new HashSet<(Guid ClassId, Guid SubjectId, DayOfWeek Day)>();
        var placements = new List<Placement>();

        foreach (var task in orderedTasks)
        {
            for (var i = 0; i < task.PeriodsPerWeek; i++)
            {
                var slot = TryPlaceLesson(task, workingDays, input.PeriodsPerDay, teacherBusy, classBusy, classSubjectDay, allowSameDayRepeat: false)
                    ?? TryPlaceLesson(task, workingDays, input.PeriodsPerDay, teacherBusy, classBusy, classSubjectDay, allowSameDayRepeat: true);

                if (slot == null)
                {
                    result.Unplaced.Add(new UnplacedLessonDto
                    {
                        ClassId = task.ClassId,
                        ClassName = task.ClassName,
                        SubjectId = task.SubjectId,
                        SubjectName = task.SubjectName,
                        TeacherId = task.TeacherId,
                        TeacherName = task.TeacherName,
                        Reason = "No free period without a teacher or class conflict",
                    });
                    continue;
                }

                var (day, period) = slot.Value;
                teacherBusy.Add((task.TeacherId, day, period));
                classBusy.Add((task.ClassId, day, period));
                classSubjectDay.Add((task.ClassId, task.SubjectId, day));
                placements.Add(new Placement
                {
                    ClassId = task.ClassId,
                    SubjectId = task.SubjectId,
                    TeacherId = task.TeacherId,
                    Day = day,
                    Period = period,
                });
            }
        }

        var periodTimes = BuildPeriodTimes(
            input.PeriodStartTime, input.PeriodDurationMinutes, input.PeriodsPerDay,
            input.BreakAfterPeriods, input.BreakDurationMinutes);

        var placementsByClass = placements
            .GroupBy(p => p.ClassId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Report every active class in the year, not just ones that got a
        // slot placed — otherwise a class with no ClassSubjects, or one
        // where every lesson failed to place, silently disappears from the
        // results instead of showing "No subjects" / "Not placed".
        foreach (var cls in classes)
        {
            var requestedForClass = classSubjects.Where(cs => cs.ClassId == cls.Id).Sum(cs => cs.PeriodsPerWeek);
            var classPlacements = placementsByClass.TryGetValue(cls.Id, out var list) ? list : new List<Placement>();

            Guid? timetableId = null;
            if (classPlacements.Count > 0)
            {
                // Draft — same as the manual CreateAsync flow: review, then ActivateAsync.
                var timetable = new Timetable(Guid.NewGuid(), AbpSession.TenantId, cls.Id, input.EffectiveDate)
                {
                    IsActive = false
                };
                await _timetableRepository.InsertAsync(timetable);

                foreach (var p in classPlacements)
                {
                    var (start, end) = periodTimes[p.Period];
                    var newSlot = new TimetableSlot(Guid.NewGuid(), timetable.Id, p.Day, p.Period, start, end, p.SubjectId, p.TeacherId);
                    await _slotRepository.InsertAsync(newSlot);
                }

                timetableId = timetable.Id;
                result.TotalSlotsPlaced += classPlacements.Count;
            }

            result.Classes.Add(new GeneratedClassTimetableDto
            {
                ClassId = cls.Id,
                ClassName = cls.ClassName,
                TimetableId = timetableId,
                SlotsPlaced = classPlacements.Count,
                SlotsRequested = requestedForClass,
            });
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return result;
    }

    private static (DayOfWeek Day, int Period)? TryPlaceLesson(
        LessonTask task,
        List<DayOfWeek> workingDays,
        int periodsPerDay,
        HashSet<(Guid TeacherId, DayOfWeek Day, int Period)> teacherBusy,
        HashSet<(Guid ClassId, DayOfWeek Day, int Period)> classBusy,
        HashSet<(Guid ClassId, Guid SubjectId, DayOfWeek Day)> classSubjectDay,
        bool allowSameDayRepeat)
    {
        // Rotate the day scan order per class+subject so different subjects
        // don't all pile onto the first working day.
        var dayCount = workingDays.Count;
        var offset = Math.Abs(task.ClassId.GetHashCode() ^ task.SubjectId.GetHashCode()) % dayCount;

        for (var d = 0; d < dayCount; d++)
        {
            var day = workingDays[(offset + d) % dayCount];

            if (!allowSameDayRepeat && classSubjectDay.Contains((task.ClassId, task.SubjectId, day)))
                continue;

            for (var period = 1; period <= periodsPerDay; period++)
            {
                if (classBusy.Contains((task.ClassId, day, period))) continue;
                if (teacherBusy.Contains((task.TeacherId, day, period))) continue;
                return (day, period);
            }
        }

        return null;
    }

    private static Dictionary<int, (TimeSpan Start, TimeSpan End)> BuildPeriodTimes(
        TimeSpan startTime, int durationMinutes, int periodsPerDay,
        List<int> breakAfterPeriods, int breakDurationMinutes)
    {
        var breaks = new HashSet<int>(breakAfterPeriods ?? new List<int>());
        var times = new Dictionary<int, (TimeSpan, TimeSpan)>();
        var cursor = startTime;
        for (var period = 1; period <= periodsPerDay; period++)
        {
            var end = cursor.Add(TimeSpan.FromMinutes(durationMinutes));
            times[period] = (cursor, end);
            cursor = end;

            // A break after the last period just trails the day — harmless,
            // but skip it so EffectiveDate/day-end times stay tidy.
            if (breaks.Contains(period) && breakDurationMinutes > 0 && period < periodsPerDay)
                cursor = cursor.Add(TimeSpan.FromMinutes(breakDurationMinutes));
        }
        return times;
    }

    private sealed class LessonTask
    {
        public Guid ClassId { get; set; }
        public string ClassName { get; set; }
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; }
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int PeriodsPerWeek { get; set; }
    }

    private sealed class Placement
    {
        public Guid ClassId { get; set; }
        public Guid SubjectId { get; set; }
        public Guid TeacherId { get; set; }
        public DayOfWeek Day { get; set; }
        public int Period { get; set; }
    }
}
