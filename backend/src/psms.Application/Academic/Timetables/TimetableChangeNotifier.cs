using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
using psms.Communication.Dispatch;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Academic.Timetables;

/// <summary>
/// US-TCH-013: tells teachers when their schedule changes, so nobody finds out
/// by walking to the wrong room. Only changes to ACTIVE timetables matter:
/// drafts (e.g. freshly generated ones) don't affect anyone until they are
/// activated, and activation itself is announced. Goes out through the COMM-01
/// dispatcher (in-app + push, queued), so preferences and consents apply.
/// </summary>
public class TimetableChangeNotifier : ITransientDependency
{
    private const string ScheduleUrl = "/teacher/schedule";

    private static readonly IReadOnlyList<NotificationChannel> Channels =
        new[] { NotificationChannel.InApp, NotificationChannel.Push };

    private readonly IRepository<Teacher, Guid> _teacherRepository;
    private readonly IRepository<Subject, Guid> _subjectRepository;
    private readonly IRepository<Class, Guid> _classRepository;
    private readonly INotificationDispatcher _dispatcher;
    private readonly IAbpSession _session;

    public TimetableChangeNotifier(
        IRepository<Teacher, Guid> teacherRepository,
        IRepository<Subject, Guid> subjectRepository,
        IRepository<Class, Guid> classRepository,
        INotificationDispatcher dispatcher,
        IAbpSession session)
    {
        _teacherRepository = teacherRepository;
        _subjectRepository = subjectRepository;
        _classRepository = classRepository;
        _dispatcher = dispatcher;
        _session = session;
    }

    /// <summary>A period was added to a teacher's active timetable.</summary>
    public async Task SlotAddedAsync(Timetable timetable, TimetableSlot slot)
    {
        if (!timetable.IsActive) return;
        await NotifyAsync(new[] { slot.TeacherId }, "New period on your schedule",
            $"{await DescribeAsync(timetable, slot)} has been added to your schedule.");
    }

    /// <summary>Several periods were added at once: one notice per teacher.</summary>
    public async Task SlotsAddedAsync(Timetable timetable, IEnumerable<TimetableSlot> slots)
    {
        if (!timetable.IsActive) return;
        var className = await ClassNameAsync(timetable.ClassId);
        foreach (var group in slots.GroupBy(s => s.TeacherId))
        {
            var count = group.Count();
            await NotifyAsync(new[] { group.Key }, "New periods on your schedule",
                count == 1
                    ? await DescribeAsync(timetable, group.First()) + " has been added to your schedule."
                    : $"{count} periods with {className} have been added to your schedule.");
        }
    }

    /// <summary>A period was removed from a teacher's active timetable.</summary>
    public async Task SlotRemovedAsync(Timetable timetable, TimetableSlot slot)
    {
        if (!timetable.IsActive) return;
        await NotifyAsync(new[] { slot.TeacherId }, "Period removed from your schedule",
            $"{await DescribeAsync(timetable, slot)} has been removed from your schedule.");
    }

    /// <summary>
    /// A period on an active timetable changed. If it moved to another teacher,
    /// the previous teacher hears it was removed and the new one that it was
    /// added; otherwise the teacher gets the before and after.
    /// </summary>
    public async Task SlotChangedAsync(Timetable timetable, TimetableSlot before, TimetableSlot after)
    {
        if (!timetable.IsActive) return;

        if (before.TeacherId != after.TeacherId)
        {
            await SlotRemovedAsync(timetable, before);
            await SlotAddedAsync(timetable, after);
            return;
        }

        var was = await DescribeAsync(timetable, before);
        var now = await DescribeAsync(timetable, after);
        if (was == now) return;

        await NotifyAsync(new[] { after.TeacherId }, "A period on your schedule changed",
            $"{was} is now {now}.");
    }

    /// <summary>
    /// A whole class timetable was activated or deactivated: one notice per
    /// affected teacher rather than one per period.
    /// </summary>
    public async Task TimetableSwitchedAsync(Timetable timetable, IEnumerable<Guid> teacherIds, bool activated)
    {
        var className = await ClassNameAsync(timetable.ClassId);
        var message = activated
            ? $"A new timetable for {className} is now in use. Check your schedule for any changes."
            : $"The timetable for {className} is no longer in use. Its periods have been removed from your schedule.";
        await NotifyAsync(teacherIds, "Your schedule has changed", message);
    }

    private async Task NotifyAsync(IEnumerable<Guid> teacherIds, string title, string message)
    {
        var ids = teacherIds.Distinct().ToList();
        if (ids.Count == 0) return;

        var userIds = await _teacherRepository
            .GetAll()
            .Where(t => ids.Contains(t.Id) && t.TenantId == _session.TenantId)
            .Select(t => t.UserId)
            .Distinct()
            .ToListAsync();
        if (userIds.Count == 0) return;

        await _dispatcher.EnqueueAsync(new NotificationRequest
        {
            TenantId = _session.TenantId,
            RecipientUserIds = userIds,
            Type = NotificationType.Academic,
            Priority = NotificationPriority.High,
            Title = title,
            Message = message,
            ActionUrl = ScheduleUrl,
            RequestedChannels = Channels,
        });
    }

    /// <summary>e.g. "Tuesday period 3 (08:40–09:20), Mathematics with Grade 3D in room 12".</summary>
    private async Task<string> DescribeAsync(Timetable timetable, TimetableSlot slot)
    {
        var subject = await _subjectRepository
            .GetAll()
            .Where(s => s.Id == slot.SubjectId)
            .Select(s => s.SubjectName)
            .FirstOrDefaultAsync();
        var className = await ClassNameAsync(timetable.ClassId);
        var day = CultureInfo.InvariantCulture.DateTimeFormat.GetDayName(slot.DayOfWeek);
        var room = string.IsNullOrWhiteSpace(slot.RoomNumber) ? string.Empty : $" in room {slot.RoomNumber}";
        return $"{day} period {slot.PeriodNumber} ({slot.StartTime:hh\\:mm}–{slot.EndTime:hh\\:mm}), " +
               $"{subject ?? "a lesson"} with {className}{room}";
    }

    private async Task<string> ClassNameAsync(Guid classId)
        => await _classRepository
               .GetAll()
               .Where(c => c.Id == classId)
               .Select(c => c.ClassName)
               .FirstOrDefaultAsync() ?? "your class";
}
