using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
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

    public OnlineLessonAppService(
        IRepository<OnlineLesson, Guid> onlineLessonRepository,
        IRepository<ClassSubject, Guid> classSubjectRepository)
    {
        _onlineLessonRepository = onlineLessonRepository;
        _classSubjectRepository = classSubjectRepository;
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

        return ObjectMapper.Map<OnlineLessonDto>(lesson);
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_View)]
    public async Task<PagedResultDto<OnlineLessonListDto>> GetAllAsync(GetOnlineLessonsInput input)
    {
        var query = _onlineLessonRepository
            .GetAll()
            .Include(ol => ol.ClassSubject).ThenInclude(cs => cs.Class)
            .Include(ol => ol.ClassSubject).ThenInclude(cs => cs.Subject)
            .Where(ol => ol.TenantId == AbpSession.TenantId)
            .WhereIf(input.ClassSubjectId.HasValue, ol => ol.ClassSubjectId == input.ClassSubjectId.Value)
            .WhereIf(input.Status.HasValue, ol => ol.Status == input.Status.Value)
            .WhereIf(input.StartDate.HasValue, ol => ol.ScheduledStartTime >= input.StartDate.Value)
            .WhereIf(input.EndDate.HasValue, ol => ol.ScheduledEndTime <= input.EndDate.Value)
            .WhereIf(input.HostTeacherUserId.HasValue, ol => ol.HostTeacherUserId == input.HostTeacherUserId.Value);

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
        // Validate ClassSubject exists
        var classSubject = await _classSubjectRepository
            .GetAll()
            .FirstOrDefaultAsync(cs => cs.Id == input.ClassSubjectId && cs.TenantId == AbpSession.TenantId);

        if (classSubject == null)
            throw new UserFriendlyException(LearningExceptionCodes.ClassSubjectNotFound,
                "Class-subject assignment not found.");

        // Validate times
        if (input.ScheduledEndTime <= input.ScheduledStartTime)
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLessonTimes,
                "Scheduled end time must be after start time.");

        if (input.ScheduledStartTime < DateTime.UtcNow)
            throw new UserFriendlyException(LearningExceptionCodes.LessonStartTimeInPast,
                "Scheduled start time cannot be in the past.");

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
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(lesson.Id);
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_Schedule)]
    public async Task<OnlineLessonDto> UpdateAsync(Guid id, UpdateOnlineLessonDto input)
    {
        var lesson = await _onlineLessonRepository
            .FirstOrDefaultAsync(ol => ol.Id == id && ol.TenantId == AbpSession.TenantId);

        if (lesson == null)
            throw new UserFriendlyException(LearningExceptionCodes.OnlineLessonNotFound,
                "Online lesson not found.");

        if (lesson.Status != OnlineLessonStatus.Scheduled)
            throw new UserFriendlyException(LearningExceptionCodes.CannotUpdateNonScheduledLesson,
                "Only scheduled lessons can be updated.");

        if (input.Title != null) lesson.Title = input.Title.Trim();
        if (input.Description != null) lesson.Description = input.Description;
        if (input.MeetingLink != null) lesson.MeetingLink = input.MeetingLink;
        if (input.MeetingId != null) lesson.MeetingId = input.MeetingId;
        if (input.MeetingPassword != null) lesson.MeetingPassword = input.MeetingPassword;

        await _onlineLessonRepository.UpdateAsync(lesson);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_Cancel)]
    public async Task DeleteAsync(Guid id)
    {
        var lesson = await _onlineLessonRepository
            .FirstOrDefaultAsync(ol => ol.Id == id && ol.TenantId == AbpSession.TenantId);

        if (lesson == null)
            throw new UserFriendlyException(LearningExceptionCodes.OnlineLessonNotFound,
                "Online lesson not found.");

        if (lesson.Status != OnlineLessonStatus.Scheduled && lesson.Status != OnlineLessonStatus.Cancelled)
            throw new UserFriendlyException(LearningExceptionCodes.CannotDeleteActiveLesson,
                "Only scheduled or cancelled lessons can be deleted.");

        await _onlineLessonRepository.DeleteAsync(lesson);
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_Host)]
    public async Task<OnlineLessonDto> StartAsync(Guid id)
    {
        var lesson = await _onlineLessonRepository
            .FirstOrDefaultAsync(ol => ol.Id == id && ol.TenantId == AbpSession.TenantId);

        if (lesson == null)
            throw new UserFriendlyException(LearningExceptionCodes.OnlineLessonNotFound,
                "Online lesson not found.");

        try
        {
            lesson.Start();
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLessonStatusTransition,
                "Only scheduled lessons can be started.");
        }

        await _onlineLessonRepository.UpdateAsync(lesson);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Learning_Lessons_Host)]
    public async Task<OnlineLessonDto> EndAsync(Guid id, int attendeeCount)
    {
        var lesson = await _onlineLessonRepository
            .FirstOrDefaultAsync(ol => ol.Id == id && ol.TenantId == AbpSession.TenantId);

        if (lesson == null)
            throw new UserFriendlyException(LearningExceptionCodes.OnlineLessonNotFound,
                "Online lesson not found.");

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
        var lesson = await _onlineLessonRepository
            .FirstOrDefaultAsync(ol => ol.Id == id && ol.TenantId == AbpSession.TenantId);

        if (lesson == null)
            throw new UserFriendlyException(LearningExceptionCodes.OnlineLessonNotFound,
                "Online lesson not found.");

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
        var lesson = await _onlineLessonRepository
            .FirstOrDefaultAsync(ol => ol.Id == id && ol.TenantId == AbpSession.TenantId);

        if (lesson == null)
            throw new UserFriendlyException(LearningExceptionCodes.OnlineLessonNotFound,
                "Online lesson not found.");

        if (input.NewEndTime <= input.NewStartTime)
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLessonTimes,
                "New end time must be after new start time.");

        try
        {
            lesson.Reschedule(input.NewStartTime, input.NewEndTime);
        }
        catch (InvalidOperationException)
        {
            throw new UserFriendlyException(LearningExceptionCodes.InvalidLessonStatusTransition,
                "Cannot reschedule completed or in-progress lessons.");
        }

        await _onlineLessonRepository.UpdateAsync(lesson);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Learning_Recordings_Upload)]
    public async Task<OnlineLessonDto> AddRecordingAsync(Guid id, AddRecordingDto input)
    {
        var lesson = await _onlineLessonRepository
            .FirstOrDefaultAsync(ol => ol.Id == id && ol.TenantId == AbpSession.TenantId);

        if (lesson == null)
            throw new UserFriendlyException(LearningExceptionCodes.OnlineLessonNotFound,
                "Online lesson not found.");

        lesson.AddRecording(input.RecordingUrl);

        await _onlineLessonRepository.UpdateAsync(lesson);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }
}
