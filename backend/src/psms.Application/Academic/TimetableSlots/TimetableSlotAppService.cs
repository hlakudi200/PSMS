using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Academic.Shared;
using psms.Academic.TimetableSlots.Dto;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Academic.TimetableSlots;

[AbpAuthorize(PermissionNames.Academic_Timetables)]
public class TimetableSlotAppService : ApplicationService, ITimetableSlotAppService
{
    private readonly IRepository<TimetableSlot, Guid> _slotRepository;
    private readonly IRepository<Timetable, Guid> _timetableRepository;
    private readonly IRepository<Subject, Guid> _subjectRepository;
    private readonly IRepository<Teacher, Guid> _teacherRepository;

    public TimetableSlotAppService(
        IRepository<TimetableSlot, Guid> slotRepository,
        IRepository<Timetable, Guid> timetableRepository,
        IRepository<Subject, Guid> subjectRepository,
        IRepository<Teacher, Guid> teacherRepository)
    {
        _slotRepository = slotRepository;
        _timetableRepository = timetableRepository;
        _subjectRepository = subjectRepository;
        _teacherRepository = teacherRepository;
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_View)]
    public async Task<TimetableSlotDto> GetAsync(Guid id)
    {
        var slot = await _slotRepository
            .GetAll()
            .Include(ts => ts.Timetable)
            .Include(ts => ts.Subject)
            .Include(ts => ts.Teacher)
            .FirstOrDefaultAsync(ts => ts.Id == id && ts.Timetable.TenantId == AbpSession.TenantId);

        if (slot == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableSlotNotFound, "Timetable slot not found.");

        return ObjectMapper.Map<TimetableSlotDto>(slot);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_View)]
    public async Task<ListResultDto<TimetableSlotListDto>> GetByTimetableAsync(Guid timetableId)
    {
        var slots = await _slotRepository
            .GetAll()
            .Include(ts => ts.Timetable)
            .Include(ts => ts.Subject)
            .Include(ts => ts.Teacher)
            .Where(ts => ts.TimetableId == timetableId && ts.Timetable.TenantId == AbpSession.TenantId)
            .OrderBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.PeriodNumber)
            .ToListAsync();

        return new ListResultDto<TimetableSlotListDto>(
            ObjectMapper.Map<List<TimetableSlotListDto>>(slots));
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_View)]
    public async Task<ListResultDto<TimetableSlotListDto>> GetByDayAsync(Guid timetableId, DayOfWeek day)
    {
        var slots = await _slotRepository
            .GetAll()
            .Include(ts => ts.Timetable)
            .Include(ts => ts.Subject)
            .Include(ts => ts.Teacher)
            .Where(ts => ts.TimetableId == timetableId
                && ts.DayOfWeek == day
                && ts.Timetable.TenantId == AbpSession.TenantId)
            .OrderBy(ts => ts.PeriodNumber)
            .ToListAsync();

        return new ListResultDto<TimetableSlotListDto>(
            ObjectMapper.Map<List<TimetableSlotListDto>>(slots));
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_View)]
    public async Task<ListResultDto<TimetableSlotListDto>> GetByTeacherAsync(Guid teacherId)
    {
        var slots = await _slotRepository
            .GetAll()
            .Include(ts => ts.Timetable)
            .Include(ts => ts.Subject)
            .Include(ts => ts.Teacher)
            .Where(ts => ts.TeacherId == teacherId
                && ts.Timetable.IsActive
                && ts.Timetable.TenantId == AbpSession.TenantId)
            .OrderBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.PeriodNumber)
            .ToListAsync();

        return new ListResultDto<TimetableSlotListDto>(
            ObjectMapper.Map<List<TimetableSlotListDto>>(slots));
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Create)]
    public async Task<TimetableSlotDto> CreateAsync(CreateTimetableSlotDto input)
    {
        await ValidateSlotInput(input.TimetableId, input.DayOfWeek, input.PeriodNumber,
            input.StartTime, input.EndTime, input.SubjectId, input.TeacherId);

        var slot = new TimetableSlot(
            Guid.NewGuid(),
            input.TimetableId,
            input.DayOfWeek,
            input.PeriodNumber,
            input.StartTime,
            input.EndTime,
            input.SubjectId,
            input.TeacherId)
        {
            RoomNumber = input.RoomNumber
        };

        await _slotRepository.InsertAsync(slot);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(slot.Id);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Edit)]
    public async Task<TimetableSlotDto> UpdateAsync(Guid id, UpdateTimetableSlotDto input)
    {
        var slot = await _slotRepository
            .GetAll()
            .Include(ts => ts.Timetable)
            .FirstOrDefaultAsync(ts => ts.Id == id && ts.Timetable.TenantId == AbpSession.TenantId);

        if (slot == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableSlotNotFound, "Timetable slot not found.");

        // Validate referenced entities exist BEFORE applying changes
        if (input.SubjectId.HasValue)
        {
            var subject = await _subjectRepository
                .FirstOrDefaultAsync(s => s.Id == input.SubjectId.Value && s.TenantId == AbpSession.TenantId);
            if (subject == null)
                throw new UserFriendlyException(AcademicExceptionCodes.SubjectNotFound, "Subject not found.");
        }

        if (input.TeacherId.HasValue)
        {
            var teacher = await _teacherRepository
                .FirstOrDefaultAsync(t => t.Id == input.TeacherId.Value && t.TenantId == AbpSession.TenantId);
            if (teacher == null)
                throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Teacher not found.");
        }

        if (input.StartTime.HasValue) slot.StartTime = input.StartTime.Value;
        if (input.EndTime.HasValue) slot.EndTime = input.EndTime.Value;
        if (input.SubjectId.HasValue) slot.SubjectId = input.SubjectId.Value;
        if (input.TeacherId.HasValue) slot.TeacherId = input.TeacherId.Value;
        if (input.RoomNumber != null) slot.RoomNumber = input.RoomNumber;

        // Validate times after update
        if (slot.StartTime >= slot.EndTime)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidSlotTimes,
                "Start time must be before end time.");

        // Check time overlap after update (exclude self)
        var hasOverlap = await _slotRepository
            .GetAll()
            .Include(ts => ts.Timetable)
            .AnyAsync(ts => ts.TimetableId == slot.TimetableId
                && ts.DayOfWeek == slot.DayOfWeek
                && ts.Id != id
                && ts.StartTime < slot.EndTime
                && ts.EndTime > slot.StartTime
                && ts.Timetable.TenantId == AbpSession.TenantId);

        if (hasOverlap)
            throw new UserFriendlyException(AcademicExceptionCodes.OverlappingTimetableSlot,
                "This time slot overlaps with another slot on the same day.");

        // Check teacher schedule conflict after update (exclude self)
        if (input.TeacherId.HasValue || input.StartTime.HasValue || input.EndTime.HasValue)
        {
            var hasTeacherConflict = await _slotRepository
                .GetAll()
                .Include(ts => ts.Timetable)
                .AnyAsync(ts => ts.TeacherId == slot.TeacherId
                    && ts.DayOfWeek == slot.DayOfWeek
                    && ts.Id != id
                    && ts.StartTime < slot.EndTime
                    && ts.EndTime > slot.StartTime
                    && ts.Timetable.IsActive
                    && ts.Timetable.TenantId == AbpSession.TenantId);

            if (hasTeacherConflict)
                throw new UserFriendlyException(AcademicExceptionCodes.TeacherScheduleConflict,
                    "The teacher has a schedule conflict with another active timetable slot at this time.");
        }

        await _slotRepository.UpdateAsync(slot);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var slot = await _slotRepository
            .GetAll()
            .Include(ts => ts.Timetable)
            .FirstOrDefaultAsync(ts => ts.Id == id && ts.Timetable.TenantId == AbpSession.TenantId);

        if (slot == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableSlotNotFound, "Timetable slot not found.");

        await _slotRepository.DeleteAsync(slot);
    }

    [AbpAuthorize(PermissionNames.Academic_Timetables_Create)]
    public async Task<ListResultDto<TimetableSlotDto>> BulkCreateAsync(Guid timetableId, List<CreateTimetableSlotDto> input)
    {
        // Validate timetable exists and belongs to tenant
        var timetable = await _timetableRepository
            .FirstOrDefaultAsync(t => t.Id == timetableId && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "Timetable not found.");

        // Fail-fast: validate all slots before inserting any
        foreach (var slotInput in input)
        {
            slotInput.TimetableId = timetableId;

            await ValidateSlotInput(slotInput.TimetableId, slotInput.DayOfWeek, slotInput.PeriodNumber,
                slotInput.StartTime, slotInput.EndTime, slotInput.SubjectId, slotInput.TeacherId);
        }

        // Check for duplicates within the batch itself
        var batchDuplicates = input
            .GroupBy(s => new { s.DayOfWeek, s.PeriodNumber })
            .Where(g => g.Count() > 1)
            .ToList();

        if (batchDuplicates.Any())
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateSlotPeriod,
                $"Duplicate period found in batch: {batchDuplicates.First().Key.DayOfWeek} Period {batchDuplicates.First().Key.PeriodNumber}.");

        // Check for time overlaps within the batch
        foreach (var dayGroup in input.GroupBy(s => s.DayOfWeek))
        {
            var sorted = dayGroup.OrderBy(s => s.StartTime).ToList();
            for (int i = 1; i < sorted.Count; i++)
            {
                if (sorted[i].StartTime < sorted[i - 1].EndTime)
                    throw new UserFriendlyException(AcademicExceptionCodes.OverlappingTimetableSlot,
                        $"Overlapping time slots in batch on {dayGroup.Key}: Period {sorted[i - 1].PeriodNumber} and Period {sorted[i].PeriodNumber}.");
            }
        }

        var createdSlots = new List<TimetableSlot>();

        foreach (var slotInput in input)
        {
            var slot = new TimetableSlot(
                Guid.NewGuid(),
                timetableId,
                slotInput.DayOfWeek,
                slotInput.PeriodNumber,
                slotInput.StartTime,
                slotInput.EndTime,
                slotInput.SubjectId,
                slotInput.TeacherId)
            {
                RoomNumber = slotInput.RoomNumber
            };

            await _slotRepository.InsertAsync(slot);
            createdSlots.Add(slot);
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        // Reload with navigation properties
        var slotIds = createdSlots.Select(s => s.Id).ToList();
        var slots = await _slotRepository
            .GetAll()
            .Include(ts => ts.Timetable)
            .Include(ts => ts.Subject)
            .Include(ts => ts.Teacher)
            .Where(ts => slotIds.Contains(ts.Id) && ts.Timetable.TenantId == AbpSession.TenantId)
            .ToListAsync();

        return new ListResultDto<TimetableSlotDto>(
            ObjectMapper.Map<List<TimetableSlotDto>>(slots));
    }

    private async System.Threading.Tasks.Task ValidateSlotInput(
        Guid timetableId, DayOfWeek dayOfWeek, int periodNumber,
        TimeSpan startTime, TimeSpan endTime, Guid subjectId, Guid teacherId)
    {
        // Validate timetable exists and belongs to tenant
        var timetable = await _timetableRepository
            .FirstOrDefaultAsync(t => t.Id == timetableId && t.TenantId == AbpSession.TenantId);

        if (timetable == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TimetableNotFound, "Timetable not found.");

        // Validate start time < end time
        if (startTime >= endTime)
            throw new UserFriendlyException(AcademicExceptionCodes.InvalidSlotTimes,
                "Start time must be before end time.");

        // Validate subject exists
        var subject = await _subjectRepository
            .FirstOrDefaultAsync(s => s.Id == subjectId && s.TenantId == AbpSession.TenantId);

        if (subject == null)
            throw new UserFriendlyException(AcademicExceptionCodes.SubjectNotFound, "Subject not found.");

        // Validate teacher exists
        var teacher = await _teacherRepository
            .FirstOrDefaultAsync(t => t.Id == teacherId && t.TenantId == AbpSession.TenantId);

        if (teacher == null)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherNotFound, "Teacher not found.");

        // Check for duplicate period (same timetable + day + period)
        var hasDuplicate = await _slotRepository
            .GetAll()
            .Include(ts => ts.Timetable)
            .AnyAsync(ts => ts.TimetableId == timetableId
                && ts.DayOfWeek == dayOfWeek
                && ts.PeriodNumber == periodNumber
                && ts.Timetable.TenantId == AbpSession.TenantId);

        if (hasDuplicate)
            throw new UserFriendlyException(AcademicExceptionCodes.DuplicateSlotPeriod,
                $"A slot already exists for {dayOfWeek} Period {periodNumber} in this timetable.");

        // Check for time overlap within same timetable + day
        var hasOverlap = await _slotRepository
            .GetAll()
            .Include(ts => ts.Timetable)
            .AnyAsync(ts => ts.TimetableId == timetableId
                && ts.DayOfWeek == dayOfWeek
                && ts.StartTime < endTime
                && ts.EndTime > startTime
                && ts.Timetable.TenantId == AbpSession.TenantId);

        if (hasOverlap)
            throw new UserFriendlyException(AcademicExceptionCodes.OverlappingTimetableSlot,
                "This time slot overlaps with another slot on the same day.");

        // Check teacher schedule conflict across all active timetables
        var hasTeacherConflict = await _slotRepository
            .GetAll()
            .Include(ts => ts.Timetable)
            .AnyAsync(ts => ts.TeacherId == teacherId
                && ts.DayOfWeek == dayOfWeek
                && ts.StartTime < endTime
                && ts.EndTime > startTime
                && ts.Timetable.IsActive
                && ts.Timetable.TenantId == AbpSession.TenantId);

        if (hasTeacherConflict)
            throw new UserFriendlyException(AcademicExceptionCodes.TeacherScheduleConflict,
                "The teacher has a schedule conflict with another active timetable slot at this time.");
    }
}
