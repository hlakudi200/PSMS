using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Authorization.Users;
using psms.Domain.Academic.Entities;
using psms.Domain.Learning.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Shared.LiveStreaming;
using psms.Domain.Shared.Storage;
using psms.Learning.OnlineLessons.Dto;
using psms.Learning.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Transactions;

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
    private readonly IRepository<Student, Guid> _studentRepository;
    private readonly IRepository<User, long> _userRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly ILiveKitTokenService _liveKit;

    // Supabase bucket for lesson recordings (public-read, like materials).
    private const string RecordingsBucket = "recordings";

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
        IRepository<Teacher, Guid> teacherRepository,
        IRepository<Student, Guid> studentRepository,
        IRepository<User, long> userRepository,
        IFileStorageService fileStorage,
        ILiveKitTokenService liveKit)
    {
        _onlineLessonRepository = onlineLessonRepository;
        _classSubjectRepository = classSubjectRepository;
        _teacherRepository = teacherRepository;
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _fileStorage = fileStorage;
        _liveKit = liveKit;
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
    public async Task<OnlineLessonDto> CreateAsync(CreateOnlineLessonDto input)
    {
        // Serializable isolation so the overlap pre-check + insert are
        // atomic: two parallel CreateAsync calls cannot both pass
        // ValidateScheduleOrThrowAsync and then both insert. On SQL Server
        // this acquires key-range locks on the indexed (TenantId,
        // ClassSubjectId, ScheduledStartTime) query; on Npgsql we rely on
        // serialisation failure → caught below as a friendly retry.
        // NOTE: ABP's [UnitOfWork] attribute can't carry IsolationLevel
        // because the property is `IsolationLevel?` and nullable enums are
        // not valid attribute argument types in C#. Use the programmatic
        // Begin/Complete pattern instead.
        using var uow = UnitOfWorkManager.Begin(new UnitOfWorkOptions
        {
            IsolationLevel = System.Transactions.IsolationLevel.Serializable,
            Scope = TransactionScopeOption.RequiresNew,
        });

        var teacherId = await ResolveCurrentTeacherIdOrThrowAsync();
        await EnsureTeacherOwnsClassSubjectAsync(input.ClassSubjectId, teacherId);

        // In-app (LiveKit) live classes are hosted inside PSMS — no external
        // meeting URL. The MeetingLink column is NOT NULL, so we store a
        // derived in-app join route. External platforms still require a real,
        // scheme-whitelisted URL (the [Url] DataAnnotation alone admits
        // file:// and weird payloads — see ValidateMeetingLinkOrThrow).
        var lessonId = Guid.NewGuid();
        var isInApp = input.Platform == OnlinePlatform.InApp;
        string meetingLink;
        if (isInApp)
        {
            meetingLink = $"/live-class/{lessonId}";
        }
        else
        {
            ValidateMeetingLinkOrThrow(input.MeetingLink);
            meetingLink = input.MeetingLink;
        }

        await ValidateScheduleOrThrowAsync(
            input.ClassSubjectId,
            input.ScheduledStartTime,
            input.ScheduledEndTime,
            excludeLessonId: null);

        var lesson = new OnlineLesson(
            lessonId,
            AbpSession.TenantId,
            input.ClassSubjectId,
            input.Title.Trim(),
            input.Platform,
            meetingLink,
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

        // Commit the inner UoW so the Serializable transaction we opened
        // above is closed before we fetch the lesson back for the DTO.
        await uow.CompleteAsync();

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

        // LC-03: Start is the moment a class begins, and it's host-only. Create
        // the LiveKit room WITH auto-egress recording HERE — before the status
        // is committed to InProgress (so no student, who is only allowed to join
        // once InProgress, can implicitly create the room first and thereby lose
        // the egress config). Best-effort: never blocks starting the lesson.
        if (lesson.Platform == OnlinePlatform.InApp && _liveKit.IsRecordingConfigured)
            await _liveKit.EnsureRecordingRoomAsync(LiveClassRoomName(lesson.Id), LiveClassRecordingKey(lesson.Id));

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

        // LC-03: closing the room stops any active auto-egress so the
        // recording finalizes + uploads promptly (rather than waiting for the
        // room's empty-timeout). Best-effort; the egress_ended webhook (LC-04)
        // is what reliably attaches the resulting recording URL.
        if (lesson.Platform == OnlinePlatform.InApp && _liveKit.IsRecordingConfigured)
            await _liveKit.CloseRoomAsync(LiveClassRoomName(lesson.Id));

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
    public async Task<OnlineLessonDto> RescheduleAsync(Guid id, RescheduleOnlineLessonDto input)
    {
        // Same TOCTOU mitigation as CreateAsync — programmatic UoW with
        // Serializable isolation. See the comment in CreateAsync for why
        // we can't use the [UnitOfWork] attribute for this.
        using var uow = UnitOfWorkManager.Begin(new UnitOfWorkOptions
        {
            IsolationLevel = System.Transactions.IsolationLevel.Serializable,
            Scope = TransactionScopeOption.RequiresNew,
        });

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

        await uow.CompleteAsync();

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

        ValidateRecordingUrlOrThrow(input.RecordingUrl);
        lesson.AddRecording(input.RecordingUrl);

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    /// <summary>
    /// Step 1 of the direct recording upload (SF-02): validate ownership +
    /// Completed status + file extension, then mint a one-time signed URL the
    /// client PUTs the bytes to (bytes never pass through this server — the
    /// scalable path for multi-GB recordings). The client then calls
    /// UploadRecordingAsync with the resulting object key.
    /// </summary>
    [AbpAuthorize(PermissionNames.Learning_Recordings_Upload)]
    public async Task<FileUploadTicket> RequestRecordingUploadUrlAsync(RequestRecordingUploadUrlDto input)
    {
        var lesson = await LoadOwnedLessonOrThrowAsync(input.LessonId, allowHostFallback: true);

        // OL-003: only Completed lessons can have a recording attached. Fail
        // here so the client never wastes a multi-GB upload that Upload would
        // reject afterwards.
        if (lesson.Status != OnlineLessonStatus.Completed)
            throw new UserFriendlyException(LearningExceptionCodes.LessonNotCompleted,
                "Recordings can only be uploaded for completed lessons.");

        ValidateRecordingExtension(input.FileName);

        var key = BuildRecordingObjectKey(lesson.Id, input.FileName);
        return await _fileStorage.CreateUploadTicketAsync(RecordingsBucket, key);
    }

    [AbpAuthorize(PermissionNames.Learning_Recordings_Upload)]
    public async Task<OnlineLessonDto> UploadRecordingAsync(UploadRecordingDto input)
    {
        // Serializable isolation so two parallel uploads on the same
        // lesson cannot both succeed — the second-arriving SaveChanges
        // would otherwise quietly overwrite the first's RecordingUrl,
        // leaking the loser's storage path. We translate a serialisation
        // failure into a friendly retry message via IsConcurrencyRetryable.
        // (See CreateAsync for why we use the programmatic UoW pattern.)
        //
        // The storage HEAD in ResolveRecordingFileAsync runs inside this
        // transaction. On PostgreSQL that is fine: Serializable uses SSI
        // (predicate locks) which do NOT block concurrent writers — a true
        // conflict surfaces as a retryable serialisation failure at commit,
        // already handled below. So the HEAD's latency cannot deadlock or
        // block another upload; it only briefly holds a pooled connection.
        // Loading the lesson here (rather than before Begin) also keeps a
        // single tracked entity, so the GetAsync re-read at the end reflects
        // the saved RecordingUrl.
        using var uow = UnitOfWorkManager.Begin(new UnitOfWorkOptions
        {
            IsolationLevel = System.Transactions.IsolationLevel.Serializable,
            Scope = TransactionScopeOption.RequiresNew,
        });

        var lesson = await LoadOwnedLessonOrThrowAsync(input.LessonId, allowHostFallback: true);

        // OL-003 acceptance criterion: only Completed lessons can have a
        // recording attached. Letting an in-progress lesson accept a file
        // would race with the End flow and pollute attendance reporting.
        if (lesson.Status != OnlineLessonStatus.Completed)
        {
            throw new UserFriendlyException(LearningExceptionCodes.LessonNotCompleted,
                "Recordings can only be uploaded for completed lessons.");
        }

        // The bytes were PUT directly to storage via the RequestRecordingUploadUrl
        // ticket. Validate the key belongs to this tenant/lesson, confirm the file
        // really exists, and enforce OL-003 against the REAL stored size — the
        // server derives the public URL (none of this is trusted from the client).
        var fileUrl = await ResolveRecordingFileAsync(input.ObjectKey, lesson.Id);

        lesson.AddRecording(fileUrl);

        try
        {
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        catch (Exception ex) when (IsConcurrencyRetryable(ex))
        {
            throw new UserFriendlyException(LearningExceptionCodes.RecordingUploadConflict,
                "Another upload completed for this lesson at the same time. Please retry.");
        }

        await uow.CompleteAsync();

        return await GetAsync(lesson.Id);
    }

    // Gated on the dedicated Join permission (held by teachers + students, NOT
    // parents) rather than View — issuing a live-classroom token is a stronger
    // capability than reading lesson metadata. LC-07 further restricts the
    // non-host path to students actually enrolled in the lesson's class.
    [AbpAuthorize(PermissionNames.Learning_Lessons_Join)]
    public async Task<LiveClassJoinDto> GetJoinTokenAsync(Guid id)
    {
        if (!AbpSession.UserId.HasValue)
            throw new UserFriendlyException(LearningExceptionCodes.LiveClassNotAvailable,
                "You must be signed in to join a live class.");

        var lesson = await _onlineLessonRepository
            .GetAll()
            .Include(ol => ol.ClassSubject)
            .FirstOrDefaultAsync(ol => ol.Id == id && ol.TenantId == AbpSession.TenantId);
        if (lesson == null)
            throw new UserFriendlyException(LearningExceptionCodes.OnlineLessonNotFound,
                "Online lesson not found.");

        if (lesson.Platform != OnlinePlatform.InApp)
            throw new UserFriendlyException(LearningExceptionCodes.LiveClassNotAvailable,
                "This lesson is not an in-app live class.");

        if (lesson.Status == OnlineLessonStatus.Completed
            || lesson.Status == OnlineLessonStatus.Cancelled
            || lesson.Status == OnlineLessonStatus.Rescheduled)
            throw new UserFriendlyException(LearningExceptionCodes.LiveClassNotAvailable,
                "This class is no longer live.");

        if (!_liveKit.IsConfigured)
            throw new UserFriendlyException(LearningExceptionCodes.LiveClassNotAvailable,
                "Live classes are not configured on this server yet.");

        // The hosting teacher publishes; everyone else (students, who hold the
        // lesson-view permission required above) joins as a viewer. Ownership
        // mirrors LoadOwnedLessonOrThrowAsync: assigned ClassSubject teacher OR
        // the recorded host user.
        var teacherId = await ResolveCurrentTeacherIdOrNullAsync();
        var isHost = (teacherId.HasValue && lesson.ClassSubject?.TeacherId == teacherId.Value)
                     || (AbpSession.UserId.HasValue && lesson.HostTeacherUserId == AbpSession.UserId.Value);

        // Non-host (student) gate: the class must be live, and the caller must
        // be a student actually enrolled in this lesson's class (LC-07).
        if (!isHost)
        {
            if (lesson.Status != OnlineLessonStatus.InProgress)
                throw new UserFriendlyException(LearningExceptionCodes.LiveClassNotAvailable,
                    "The class hasn't started yet. Please wait for your teacher to start the lesson.");

            var student = await _studentRepository
                .FirstOrDefaultAsync(s => s.UserId == AbpSession.UserId.Value
                                       && s.TenantId == AbpSession.TenantId);
            if (student == null)
                throw new UserFriendlyException(LearningExceptionCodes.LiveClassNotAvailable,
                    "Only the class teacher and enrolled students can join this live class.");

            // Null-safe: if the ClassSubject is somehow unavailable, treat as
            // not-enrolled rather than throwing a raw NRE.
            if (student.CurrentClassId != lesson.ClassSubject?.ClassId)
                throw new UserFriendlyException(LearningExceptionCodes.LiveClassNotAvailable,
                    "You are not enrolled in this class.");
        }

        // Room name is derived from the lesson id — no stored column needed.
        var roomName = LiveClassRoomName(lesson.Id);
        var identity = $"user-{AbpSession.UserId}";

        // LC-04: use the participant's real display name on their tile (falls
        // back to username, then identity). Resolved from AbpUsers; never throws.
        var displayName = identity;
        var user = await _userRepository.FirstOrDefaultAsync(AbpSession.UserId.Value);
        if (user != null)
        {
            var fullName = $"{user.Name} {user.Surname}".Trim();
            displayName = !string.IsNullOrWhiteSpace(fullName) ? fullName
                : !string.IsNullOrWhiteSpace(user.UserName) ? user.UserName
                : identity;
        }

        // LC-03 defence-in-depth: the egress-recorded room is primarily created
        // in StartAsync (before students can join). Re-ensure it on the host's
        // join too, in case the empty room was reaped before anyone arrived.
        // Idempotent (CreateRoom on an existing room is a no-op); best-effort.
        if (isHost && _liveKit.IsRecordingConfigured)
            await _liveKit.EnsureRecordingRoomAsync(roomName, LiveClassRecordingKey(lesson.Id));

        var ticket = _liveKit.CreateJoinToken(new LiveKitJoinRequest
        {
            Identity = identity,
            Name = displayName,
            RoomName = roomName,
            CanPublish = isHost,
            CanPublishData = true,
        });

        return new LiveClassJoinDto
        {
            ServerUrl = ticket.ServerUrl,
            Token = ticket.Token,
            RoomName = roomName,
            Identity = identity,
            CanPublish = isHost,
        };
    }

    #region Private Methods

    /// <summary>LiveKit room name for an in-app live class (derived, not stored).</summary>
    private static string LiveClassRoomName(Guid lessonId) => $"class-{lessonId}";

    /// <summary>
    /// Deterministic object key the live-class recording is egressed to, in the
    /// recordings bucket. One recording per lesson (a re-run overwrites). Kept
    /// deterministic so the egress_ended webhook (LC-04) can locate it.
    /// </summary>
    private string LiveClassRecordingKey(Guid lessonId)
        => $"{AbpSession.TenantId ?? 0}/{lessonId}/recording.mp4";

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

    // OL-003: recording-file whitelist + 5 GB hard cap. The frontend
    // surfaces a friendlier "advisory" cap at the same value so the user
    // gets immediate feedback before the upload begins.
    private const long MaxRecordingFileBytes = 5L * 1024 * 1024 * 1024; // 5 GB
    private static readonly HashSet<string> RecordingFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mov", ".avi", ".webm"
    };

    /// <summary>OL-003 extension whitelist (checked before minting a ticket).</summary>
    private static void ValidateRecordingExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty);
        if (string.IsNullOrEmpty(extension) || !RecordingFileExtensions.Contains(extension))
            throw new UserFriendlyException(LearningExceptionCodes.RecordingFileTypeInvalid,
                "Recording must be one of: MP4, MOV, AVI, WebM.");
    }

    /// <summary>Tenant + lesson-scoped object key for a recording file.</summary>
    private string BuildRecordingObjectKey(Guid lessonId, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return $"{AbpSession.TenantId ?? 0}/{lessonId}/{Guid.NewGuid()}{ext}";
    }

    /// <summary>
    /// Validates a client-supplied recording object key (must sit in this
    /// tenant + lesson prefix, no traversal), confirms the file actually
    /// exists in storage, enforces OL-003 (extension + 5 GB cap) against the
    /// REAL stored size, and returns the server-derived public URL.
    /// </summary>
    private async Task<string> ResolveRecordingFileAsync(string objectKey, Guid lessonId)
    {
        var expectedPrefix = $"{AbpSession.TenantId ?? 0}/{lessonId}/";
        if (string.IsNullOrWhiteSpace(objectKey)
            || !objectKey.StartsWith(expectedPrefix, StringComparison.Ordinal)
            || objectKey.Contains(".."))
            throw new UserFriendlyException(LearningExceptionCodes.InvalidRecordingUpload,
                "Invalid upload reference. Call RequestRecordingUploadUrl and upload the file first.");

        // Validate the extension on the object key — the path that is actually
        // stored and served — not just the cosmetic FileName. (The signed
        // upload URL is already bound to this exact key, so the extension is
        // effectively fixed at request time; this is belt-and-braces.)
        ValidateRecordingExtension(objectKey);

        var info = await _fileStorage.GetObjectInfoAsync(RecordingsBucket, objectKey);
        if (info == null)
            throw new UserFriendlyException(LearningExceptionCodes.InvalidRecordingUpload,
                "The uploaded recording was not found in storage. Please re-upload.");

        if (info.SizeBytes == 0)
            throw new UserFriendlyException(LearningExceptionCodes.RecordingFileMissing,
                "A recording file is required.");

        if (info.SizeBytes > MaxRecordingFileBytes)
            throw new UserFriendlyException(LearningExceptionCodes.RecordingFileTooLarge,
                "Recording file exceeds the 5 GB cap.");

        return _fileStorage.GetPublicUrl(RecordingsBucket, objectKey);
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
        ValidateHttpsUrlOrThrow(
            meetingLink,
            LearningExceptionCodes.InvalidMeetingLink,
            "Meeting link");
    }

    private static void ValidateRecordingUrlOrThrow(string recordingUrl)
    {
        ValidateHttpsUrlOrThrow(
            recordingUrl,
            LearningExceptionCodes.InvalidRecordingUrl,
            "Recording URL");
    }

    /// <summary>
    /// Shared http(s)-scheme allowlist. Takes the error code so callers can
    /// surface a domain-specific code (Meeting vs Recording) instead of
    /// overloading one validation code for two semantically different
    /// fields. Mirrored client-side in TeacherLessonsPageContent /
    /// TeacherHostLessonPageContent's handleJoin / handleOpenMeeting.
    /// </summary>
    private static void ValidateHttpsUrlOrThrow(string value, string errorCode, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new UserFriendlyException(errorCode, $"{label} is required.");

        var trimmed = value.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            throw new UserFriendlyException(errorCode, $"{label} must be an absolute URL.");

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            throw new UserFriendlyException(errorCode, $"{label} must use http:// or https://.");
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
