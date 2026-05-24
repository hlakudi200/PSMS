using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Academic.Entities;
using psms.Domain.Learning.Entities;
using psms.Domain.Shared.Enums;
using psms.Learning.OnlineLessons.Dto;
using psms.Learning.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Learning.OnlineLessons;

/// <summary>
/// Service for managing online lessons.
/// </summary>
[AbpAuthorize(PermissionNames.Learning_Lessons)]
public class OnlineLessonAppService : ApplicationService, IOnlineLessonAppService
{
    private readonly IRepository<OnlineLesson, Guid> _onlineLessonRepository;
    private readonly IRepository<ClassSubject, Guid> _classSubjectRepository;
    private readonly IRepository<Teacher, Guid> _teacherRepository;

    // OL-001 scheduling guard rails. All times are in UTC; the school-hours
    // window is converted from 07:00-17:00 South Africa Standard Time (SAST,
    // UTC+02:00 year-round, no DST).
    private const int MinAdvanceHours = 24;
    private const int MinDurationMinutes = 30;
    private const int MaxDurationMinutes = 180;
    private const int SchoolStartHourSast = 7;
    private const int SchoolEndHourSast = 17;
    private static readonly TimeSpan SastOffset = TimeSpan.FromHours(2);
    // OL-006 host-window guard: teachers can Start a lesson at most 15
    // minutes before its scheduled start. Joining earlier would surprise
    // students and gives the meeting too long to drift before the lesson
    // really begins. Past the scheduled end time the lesson is considered
    // missed and must be rescheduled.
    private static readonly TimeSpan StartLeadWindow = TimeSpan.FromMinutes(15);

    public OnlineLessonAppService(
        IRepository<OnlineLesson, Guid> onlineLessonRepository,
        IRepository<ClassSubject, Guid> classSubjectRepository,
        IRepository<Teacher, Guid> teacherRepository)
    {
        _onlineLessonRepository = onlineLessonRepository;
        _classSubjectRepository = classSubjectRepository;
        _teacherRepository = teacherRepository;
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_View)]
    public async Task<OnlineLessonDto> GetAsync(Guid id)
    {
        var lesson = await _onlineLessonRepository
            .GetAll()
            .Include(ol => ol.ClassSubject).ThenInclude(cs => cs.Class)
            .Include(ol => ol.ClassSubject).ThenInclude(cs => cs.Subject)
            .FirstOrDefaultAsync(ol => ol.Id == id && ol.TenantId == AbpSession.TenantId);

        if (lesson == null)
            throw new UserFriendlyException(LearningExceptionCodes.OnlineLessonNotFound,
                "Online lesson not found.");

        // Teacher-ownership scope on the read path too — without this a
        // teacher could navigate to /teacher/lessons/{anotherTeachersId}
        // and see the title, description, meeting link / password of a
        // lesson they don't host. Mirrors LoadOwnedLessonOrThrowAsync with
        // host fallback; Principals/Admins (no Teacher record) get the
        // pass-through so oversight UI keeps working.
        var teacherId = await ResolveCurrentTeacherIdOrNullAsync();
        if (teacherId.HasValue)
        {
            var ownsViaClassSubject = lesson.ClassSubject?.TeacherId == teacherId.Value;
            var ownsViaHost = AbpSession.UserId.HasValue
                && lesson.HostTeacherUserId == AbpSession.UserId.Value;
            if (!ownsViaClassSubject && !ownsViaHost)
            {
                // SOC tooling needs to tell probe traffic apart from
                // genuine 404s; the user-facing message stays the same so
                // we don't leak existence, but we log the attempted access.
                Logger.Warn(
                    $"Teacher {teacherId.Value} (user {AbpSession.UserId}) tried to read lesson {id} without ownership.");
                throw new UserFriendlyException(LearningExceptionCodes.OnlineLessonNotFound,
                    "Online lesson not found.");
            }
        }

        return ObjectMapper.Map<OnlineLessonDto>(lesson);
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_View)]
    public async Task<PagedResultDto<OnlineLessonListDto>> GetAllAsync(GetOnlineLessonsInput input)
    {
        // Server-side teacher scoping. The Teacher portal sets MineOnly so
        // totalCount + pagination are correct and a teacher cannot enumerate
        // other teachers' lessons even via direct API calls — the client-side
        // filter is now belt-and-braces, not the only barrier.
        Guid? teacherId = null;
        if (input.MineOnly)
        {
            teacherId = await ResolveCurrentTeacherIdOrNullAsync();
            if (teacherId == null)
            {
                // Non-teachers (Principal, Admin) passing MineOnly is a UX
                // pass-through, not an error — return an empty page so the
                // caller sees the filter applied without a 400. Real teachers
                // still get the strict filter via teacherId.Value below.
                return new PagedResultDto<OnlineLessonListDto>(0, new List<OnlineLessonListDto>());
            }
        }

        var keyword = string.IsNullOrWhiteSpace(input.Keyword) ? null : input.Keyword.ToLower();

        var query = _onlineLessonRepository
            .GetAll()
            .Include(ol => ol.ClassSubject).ThenInclude(cs => cs.Class)
            .Include(ol => ol.ClassSubject).ThenInclude(cs => cs.Subject)
            .Where(ol => ol.TenantId == AbpSession.TenantId)
            .WhereIf(teacherId.HasValue, ol => ol.ClassSubject.TeacherId == teacherId.Value)
            .WhereIf(input.ClassSubjectId.HasValue, ol => ol.ClassSubjectId == input.ClassSubjectId.Value)
            .WhereIf(input.Status.HasValue, ol => ol.Status == input.Status.Value)
            .WhereIf(input.StartDate.HasValue, ol => ol.ScheduledStartTime >= input.StartDate.Value)
            .WhereIf(input.EndDate.HasValue, ol => ol.ScheduledEndTime <= input.EndDate.Value)
            .WhereIf(input.HostTeacherUserId.HasValue, ol => ol.HostTeacherUserId == input.HostTeacherUserId.Value)
            .WhereIf(keyword != null,
                ol => ol.Title.ToLower().Contains(keyword)
                    || ol.ClassSubject.Class.ClassName.ToLower().Contains(keyword)
                    || ol.ClassSubject.Subject.SubjectName.ToLower().Contains(keyword));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "ScheduledStartTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<OnlineLessonListDto>(
            totalCount,
            ObjectMapper.Map<List<OnlineLessonListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_View)]
    public async Task<ListResultDto<OnlineLessonListDto>> GetByClassSubjectAsync(Guid classSubjectId)
    {
        // Teacher-ownership scope: a teacher can only enumerate lessons for
        // a class-subject they themselves teach. Principals/Admins (who
        // hold the view permission but have no Teacher record) are *not*
        // gated — they need the cross-class visibility for oversight UI.
        var teacherId = await ResolveCurrentTeacherIdOrNullAsync();
        if (teacherId.HasValue)
        {
            await EnsureTeacherOwnsClassSubjectAsync(classSubjectId, teacherId.Value);
        }

        var items = await _onlineLessonRepository
            .GetAll()
            .Include(ol => ol.ClassSubject).ThenInclude(cs => cs.Class)
            .Include(ol => ol.ClassSubject).ThenInclude(cs => cs.Subject)
            .Where(ol => ol.TenantId == AbpSession.TenantId && ol.ClassSubjectId == classSubjectId)
            .OrderByDescending(ol => ol.ScheduledStartTime)
            .ToListAsync();

        return new ListResultDto<OnlineLessonListDto>(
            ObjectMapper.Map<List<OnlineLessonListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_View)]
    public async Task<ListResultDto<OnlineLessonListDto>> GetUpcomingAsync()
    {
        var now = DateTime.UtcNow;

        var items = await _onlineLessonRepository
            .GetAll()
            .Include(ol => ol.ClassSubject).ThenInclude(cs => cs.Class)
            .Include(ol => ol.ClassSubject).ThenInclude(cs => cs.Subject)
            .Where(ol => ol.TenantId == AbpSession.TenantId
                && ol.Status == OnlineLessonStatus.Scheduled
                && ol.ScheduledStartTime >= now)
            .OrderBy(ol => ol.ScheduledStartTime)
            .ToListAsync();

        return new ListResultDto<OnlineLessonListDto>(
            ObjectMapper.Map<List<OnlineLessonListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_Schedule)]
    [UnitOfWork(IsolationLevel = IsolationLevel.Serializable)]
    public async Task<OnlineLessonDto> CreateAsync(CreateOnlineLessonDto input)
    {
        // Serializable isolation so the overlap pre-check + insert are
        // atomic: two parallel CreateAsync calls cannot both pass
        // ValidateScheduleOrThrowAsync and then both insert. On SQL Server
        // this acquires key-range locks on the indexed (TenantId,
        // ClassSubjectId, ScheduledStartTime) query, blocking phantom
        // inserts in the overlap window. On Npgsql we rely on
        // serialisation failure → retry, which ABP surfaces as a normal
        // exception (callers retry the API call). See iter-1 senior review.
        var teacherId = await ResolveCurrentTeacherIdOrThrowAsync();
        await EnsureTeacherOwnsClassSubjectAsync(input.ClassSubjectId, teacherId);
        // Scheme whitelist is a security guard: `[Url]` on the DTO admits
        // file:// and weird payloads. See ValidateMeetingLinkOrThrow.
        ValidateMeetingLinkOrThrow(input.MeetingLink);
        await ValidateScheduleOrThrowAsync(
            input.ClassSubjectId,
            input.ScheduledStartTime,
            input.ScheduledEndTime,
            excludeLessonId: null);

        var lesson = new OnlineLesson(
            Guid.NewGuid(),
            AbpSession.TenantId,
            input.ClassSubjectId,
            input.Title.Trim(),
            input.Platform,
            input.MeetingLink,
            input.ScheduledStartTime,
            input.ScheduledEndTime,
            AbpSession.UserId.Value)
        {
            Description = input.Description,
            MeetingId = input.MeetingId,
            MeetingPassword = input.MeetingPassword,
            IsRecurring = input.IsRecurring,
            RecurrencePattern = input.RecurrencePattern
        };

        await _onlineLessonRepository.InsertAsync(lesson);
        try
        {
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        catch (Exception ex) when (IsConcurrencyRetryable(ex))
        {
            throw new UserFriendlyException(LearningExceptionCodes.LessonOverlapsExisting,
                "This time slot was just booked by another request. Please retry.");
        }

        return await GetAsync(lesson.Id);
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_Schedule)]
    public async Task<OnlineLessonDto> UpdateAsync(Guid id, UpdateOnlineLessonDto input)
    {
        var lesson = await LoadOwnedLessonOrThrowAsync(id);

        if (lesson.Status != OnlineLessonStatus.Scheduled)
            throw new UserFriendlyException(LearningExceptionCodes.CannotUpdateNonScheduledLesson,
                "Only scheduled lessons can be updated.");

        if (input.Title != null) lesson.Title = input.Title.Trim();
        if (input.Description != null) lesson.Description = input.Description;
        // Apply the same scheme whitelist on update — the iter-2 review
        // surfaced an XSS path via Update→handleJoin→window.open(link).
        if (input.MeetingLink != null)
        {
            ValidateMeetingLinkOrThrow(input.MeetingLink);
            lesson.MeetingLink = input.MeetingLink;
        }
        if (input.MeetingId != null) lesson.MeetingId = input.MeetingId;
        if (input.MeetingPassword != null) lesson.MeetingPassword = input.MeetingPassword;

        await _onlineLessonRepository.UpdateAsync(lesson);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_Cancel)]
    public async Task DeleteAsync(Guid id)
    {
        var lesson = await LoadOwnedLessonOrThrowAsync(id);

        if (lesson.Status != OnlineLessonStatus.Scheduled && lesson.Status != OnlineLessonStatus.Cancelled)
            throw new UserFriendlyException(LearningExceptionCodes.CannotDeleteActiveLesson,
                "Only scheduled or cancelled lessons can be deleted.");

        await _onlineLessonRepository.DeleteAsync(lesson);
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_Host)]
    public async Task<OnlineLessonDto> StartAsync(Guid id)
    {
        var lesson = await LoadOwnedLessonOrThrowAsync(id, allowHostFallback: true);

        // Status check first so a Cancelled or Completed lesson "5 min
        // before start" returns the right error class instead of the
        // (technically true but unhelpful) time-window message.
        if (lesson.Status != OnlineLessonStatus.Scheduled)
        {
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLessonStatusTransition,
                "Only scheduled lessons can be started.");
        }

        // OL-006: only allow Start within [scheduledStart - 15 min,
        // scheduledEnd). Before the lead window we tell the teacher to
        // wait; after the scheduled end we treat the lesson as missed and
        // force a reschedule so the schedule grid stays truthful.
        var now = DateTime.UtcNow;
        var earliest = lesson.ScheduledStartTime - StartLeadWindow;
        if (now < earliest)
        {
            throw new UserFriendlyException(LearningExceptionCodes.LessonStartTooEarly,
                $"This lesson can only be started from {(int)StartLeadWindow.TotalMinutes} minutes before its scheduled start.");
        }
        if (now >= lesson.ScheduledEndTime)
        {
            throw new UserFriendlyException(LearningExceptionCodes.LessonStartTooLate,
                "This lesson's scheduled window has passed. Reschedule it before starting.");
        }

        // Status was checked above, so this only catches concurrent
        // mutations that flipped Status mid-call. Keep the guard.
        try
        {
            lesson.Start();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLessonStatusTransition,
                "Only scheduled lessons can be started.");
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_Host)]
    public async Task<OnlineLessonDto> EndAsync(Guid id, int attendeeCount)
    {
        var lesson = await LoadOwnedLessonOrThrowAsync(id, allowHostFallback: true);

        // OL-002 (max 100 participants) is a meeting-platform capability —
        // PSMS cannot enforce it on the live call. What we CAN enforce is
        // sane bounds on the *recorded* attendee count so a typo doesn't
        // permanently poison reporting. 0 ≤ count ≤ MaxAttendeeCount.
        // TODO(settings): surface MaxAttendeeCount via ISettingManager so
        // a tenant with a 100-seat license can tighten without a redeploy.
        const int MaxAttendeeCount = 1000;
        if (attendeeCount < 0 || attendeeCount > MaxAttendeeCount)
        {
            throw new UserFriendlyException(LearningExceptionCodes.AttendeeCountOutOfRange,
                $"Attendee count must be between 0 and {MaxAttendeeCount}.");
        }

        try
        {
            lesson.End(attendeeCount);
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLessonStatusTransition,
                "Only in-progress lessons can be ended.");
        }

        await _onlineLessonRepository.UpdateAsync(lesson);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_Cancel)]
    public async Task<OnlineLessonDto> CancelAsync(Guid id)
    {
        var lesson = await LoadOwnedLessonOrThrowAsync(id);

        try
        {
            lesson.Cancel();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLessonStatusTransition,
                "Completed lessons cannot be cancelled.");
        }

        await _onlineLessonRepository.UpdateAsync(lesson);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_Schedule)]
    [UnitOfWork(IsolationLevel = IsolationLevel.Serializable)]
    public async Task<OnlineLessonDto> RescheduleAsync(Guid id, RescheduleOnlineLessonDto input)
    {
        // Same TOCTOU mitigation as CreateAsync — see comment there.
        var lesson = await LoadOwnedLessonOrThrowAsync(id);

        // Same OL-001 guard rails as scheduling, applied to the new times.
        // Exclude `id` from overlap detection so a lesson moved forward 30
        // minutes is not flagged as colliding with itself.
        await ValidateScheduleOrThrowAsync(
            lesson.ClassSubjectId,
            input.NewStartTime,
            input.NewEndTime,
            excludeLessonId: lesson.Id);

        try
        {
            lesson.Reschedule(input.NewStartTime, input.NewEndTime);
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLessonStatusTransition,
                "Cannot reschedule completed or in-progress lessons.");
        }

        try
        {
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        catch (Exception ex) when (IsConcurrencyRetryable(ex))
        {
            throw new UserFriendlyException(LearningExceptionCodes.LessonOverlapsExisting,
                "This time slot was just booked by another request. Please retry.");
        }

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Learning_Recordings_Upload)]
    public async Task<OnlineLessonDto> AddRecordingAsync(Guid id, AddRecordingDto input)
    {
        var lesson = await LoadOwnedLessonOrThrowAsync(id, allowHostFallback: true);

        // Recordings only make sense on lessons that actually happened.
        // Without this guard a teacher could attach a recording URL to a
        // Scheduled or Cancelled lesson (and re-attach repeatedly).
        if (lesson.Status != OnlineLessonStatus.Completed
            && lesson.Status != OnlineLessonStatus.InProgress)
        {
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLessonStatusTransition,
                "Recordings can only be attached to in-progress or completed lessons.");
        }

        lesson.AddRecording(input.RecordingUrl);

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    #region Private Methods

    /// <summary>
    /// Resolves the Teacher.Id for the active session user. Used to scope
    /// class-subject lookups so a teacher can only act on lessons attached
    /// to subjects they are assigned to (US-TCH-004 acceptance criterion).
    /// </summary>
    private async Task<Guid> ResolveCurrentTeacherIdOrThrowAsync()
    {
        var teacherId = await ResolveCurrentTeacherIdOrNullAsync();
        if (teacherId == null)
            throw new UserFriendlyException(LearningExceptionCodes.TeacherNotFoundForCurrentUser,
                "No teacher profile linked to the current user.");
        return teacherId.Value;
    }

    /// <summary>
    /// Non-throwing variant. Used by view paths where a Principal/Admin —
    /// who has the view permission but no Teacher record — should still be
    /// able to read lesson lists. Callers decide whether the absence is an
    /// error (mutations) or a degenerate-but-valid case (return empty).
    /// Result is memoised on the ambient UoW so the same request firing
    /// multiple lesson operations (e.g. Get + Cancel) only pays one DB hit.
    /// </summary>
    private async Task<Guid?> ResolveCurrentTeacherIdOrNullAsync()
    {
        if (AbpSession.UserId == null) return null;

        // UoW.Items is a per-request dictionary that ABP carries with the
        // ambient unit of work; safe to share across calls inside the same
        // HTTP request, isolated across requests.
        var cacheKey = $"OnlineLessonAppService.TeacherIdForUser:{AbpSession.UserId.Value}";
        var uow = CurrentUnitOfWork;
        if (uow != null && uow.Items.TryGetValue(cacheKey, out var cached))
        {
            return cached as Guid?;
        }

        var teacher = await _teacherRepository
            .GetAll()
            .FirstOrDefaultAsync(t => t.UserId == AbpSession.UserId.Value
                                   && t.TenantId == AbpSession.TenantId);
        var teacherId = teacher?.Id;
        if (uow != null)
        {
            uow.Items[cacheKey] = teacherId;
        }
        return teacherId;
    }

    private async Task EnsureTeacherOwnsClassSubjectAsync(Guid classSubjectId, Guid teacherId)
    {
        var owns = await _classSubjectRepository
            .GetAll()
            .AnyAsync(cs => cs.Id == classSubjectId
                         && cs.TenantId == AbpSession.TenantId
                         && cs.TeacherId == teacherId);
        if (!owns)
            throw new UserFriendlyException(LearningExceptionCodes.ClassSubjectNotFound,
                "Class-subject assignment not found for the current teacher.");
    }

    /// <summary>
    /// Loads a lesson and asserts the calling teacher is allowed to act on
    /// it. Ownership is satisfied when EITHER the calling teacher is the
    /// current TeacherId on the underlying ClassSubject OR (when
    /// <paramref name="allowHostFallback"/> is true) the lesson's
    /// HostTeacherUserId matches the active user. The fallback exists so
    /// that an in-progress lesson whose ClassSubject was reassigned to a
    /// new teacher can still be ended/recorded by its original host —
    /// otherwise an admin reshuffle mid-period would lock the lesson out.
    /// </summary>
    private async Task<OnlineLesson> LoadOwnedLessonOrThrowAsync(
        Guid lessonId,
        bool allowHostFallback = false)
    {
        var teacherId = await ResolveCurrentTeacherIdOrThrowAsync();

        var lesson = await _onlineLessonRepository
            .GetAll()
            .Include(ol => ol.ClassSubject)
            .FirstOrDefaultAsync(ol => ol.Id == lessonId
                                    && ol.TenantId == AbpSession.TenantId);
        if (lesson == null)
            throw new UserFriendlyException(LearningExceptionCodes.OnlineLessonNotFound,
                "Online lesson not found.");

        var ownsViaClassSubject = lesson.ClassSubject?.TeacherId == teacherId;
        var ownsViaHost = allowHostFallback
            && AbpSession.UserId.HasValue
            && lesson.HostTeacherUserId == AbpSession.UserId.Value;

        if (!ownsViaClassSubject && !ownsViaHost)
            throw new UserFriendlyException(LearningExceptionCodes.LessonNotOwnedByTeacher,
                "You may only act on lessons for class-subjects you teach.");

        return lesson;
    }

    /// <summary>
    /// Whitelists meeting-link schemes. `[Url]` on the DTO only requires the
    /// value to look like a URL (and admits `file://`, `ftp://`, padded
    /// strings, etc.); a malicious payload like `javascript:alert(1)` could
    /// slip through and then execute when a teacher clicks "Open" which
    /// calls window.open(link). Reject anything that isn't an absolute
    /// http(s) URL. Mirrored client-side in TeacherLessonsPageContent's
    /// handleJoin, but the server stays authoritative.
    /// </summary>
    private static void ValidateMeetingLinkOrThrow(string meetingLink)
    {
        if (string.IsNullOrWhiteSpace(meetingLink))
            throw new UserFriendlyException(LearningExceptionCodes.InvalidMeetingLink,
                "Meeting link is required.");

        var trimmed = meetingLink.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            throw new UserFriendlyException(LearningExceptionCodes.InvalidMeetingLink,
                "Meeting link must be an absolute URL.");

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            throw new UserFriendlyException(LearningExceptionCodes.InvalidMeetingLink,
                "Meeting link must use http:// or https://.");
    }

    /// <summary>
    /// True if the exception (or any inner exception) is a unique-index
    /// violation OR a serialisation/deadlock failure raised inside the
    /// transaction. Used to translate concurrency races into a friendly
    /// "another change saved at the same time — please retry" message
    /// instead of letting the raw 500 bubble. Mirrors the helper of the
    /// same shape on LearningMaterialAppService.
    /// </summary>
    private static bool IsConcurrencyRetryable(Exception ex)
    {
        for (var inner = ex; inner != null; inner = inner.InnerException)
        {
            var typeName = inner.GetType().FullName;
            if (typeName == "Microsoft.Data.SqlClient.SqlException"
                || typeName == "System.Data.SqlClient.SqlException")
            {
                var number = inner.GetType().GetProperty("Number")?.GetValue(inner);
                if (number is int n && (n == 2601 || n == 2627 || n == 1205))
                    return true; // unique violation (2601/2627) or deadlock (1205)
            }
            else if (typeName == "Npgsql.PostgresException")
            {
                var sqlState = inner.GetType().GetProperty("SqlState")?.GetValue(inner);
                if (sqlState is string s && (s == "23505" || s == "40001" || s == "40P01"))
                    return true; // unique violation / serialisation_failure / deadlock_detected
            }
        }
        return false;
    }

    /// <summary>
    /// Enforces OL-001 scheduling rules: start strictly after now+24h,
    /// times within 07:00-17:00 SAST, duration between 30 and 180 minutes,
    /// and no overlap with any other scheduled lesson on the same
    /// class-subject (excluding <paramref name="excludeLessonId"/>, used
    /// when rescheduling a lesson onto its own slot ± a few minutes).
    /// </summary>
    private async Task ValidateScheduleOrThrowAsync(
        Guid classSubjectId,
        DateTime scheduledStartUtc,
        DateTime scheduledEndUtc,
        Guid? excludeLessonId)
    {
        if (scheduledEndUtc <= scheduledStartUtc)
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLessonTimes,
                "End time must be after start time.");

        if (scheduledStartUtc < DateTime.UtcNow.AddHours(MinAdvanceHours))
            throw new UserFriendlyException(LearningExceptionCodes.LessonTooSoon,
                $"Lessons must be scheduled at least {MinAdvanceHours} hours in advance.");

        var durationMinutes = (int)(scheduledEndUtc - scheduledStartUtc).TotalMinutes;
        if (durationMinutes < MinDurationMinutes || durationMinutes > MaxDurationMinutes)
            throw new UserFriendlyException(LearningExceptionCodes.LessonDurationOutOfRange,
                $"Lesson duration must be between {MinDurationMinutes} and {MaxDurationMinutes} minutes.");

        // School-hours window: convert each UTC instant to SAST and assert
        // it falls within [07:00, 17:00]. Inclusive at the end so a
        // 16:00-17:00 lesson is admitted; 16:30-17:30 is rejected.
        // Comparing TimeSpan-vs-TimeSpan avoids floating-point brittleness
        // on the boundary that a `TotalHours > 17.0` check has when the
        // input has any sub-minute component.
        var startSast = scheduledStartUtc + SastOffset;
        var endSast = scheduledEndUtc + SastOffset;
        var schoolStart = TimeSpan.FromHours(SchoolStartHourSast);
        var schoolEnd = TimeSpan.FromHours(SchoolEndHourSast);
        var sameDay = startSast.Date == endSast.Date;
        if (!sameDay
            || startSast.TimeOfDay < schoolStart
            || endSast.TimeOfDay > schoolEnd)
        {
            throw new UserFriendlyException(LearningExceptionCodes.LessonOutsideSchoolHours,
                $"Lessons must be scheduled within school hours ({SchoolStartHourSast:D2}:00-{SchoolEndHourSast:D2}:00 SAST) on a single day.");
        }

        // Overlap check: any *active* (not cancelled / not completed)
        // lesson on the same class-subject whose [start, end) intersects
        // [scheduledStart, scheduledEnd). Cancelled/completed rows are
        // ignored — they cannot be revived in place.
        var overlapQuery = _onlineLessonRepository
            .GetAll()
            .Where(ol => ol.TenantId == AbpSession.TenantId
                      && ol.ClassSubjectId == classSubjectId
                      && (ol.Status == OnlineLessonStatus.Scheduled
                          || ol.Status == OnlineLessonStatus.InProgress)
                      && ol.ScheduledStartTime < scheduledEndUtc
                      && ol.ScheduledEndTime > scheduledStartUtc);
        if (excludeLessonId.HasValue)
            overlapQuery = overlapQuery.Where(ol => ol.Id != excludeLessonId.Value);

        var hasOverlap = await overlapQuery.AnyAsync();
        if (hasOverlap)
            throw new UserFriendlyException(LearningExceptionCodes.LessonOverlapsExisting,
                "This time slot overlaps an existing lesson on this class-subject. Pick a different slot.");
    }

    #endregion
}
