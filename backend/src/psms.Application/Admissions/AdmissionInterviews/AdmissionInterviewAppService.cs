using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.AdmissionInterviews.Dto;
using psms.Admissions.Shared;
using psms.Authorization;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Admissions.AdmissionInterviews;

/// <summary>
/// Service for managing admission interviews.
/// Implements ADM-011 to ADM-013.
/// </summary>
[AbpAuthorize(PermissionNames.Admissions_Interviews)]
public class AdmissionInterviewAppService : ApplicationService, IAdmissionInterviewAppService
{
    private readonly IRepository<AdmissionInterview, Guid> _interviewRepository;
    private readonly IRepository<Application, Guid> _applicationRepository;

    private const int MinNoticeDaysRequired = 2; // ADM-012: 48 hours minimum
    private const int MaxReschedules = 2; // ADM-012: Maximum 2 reschedules

    public AdmissionInterviewAppService(
        IRepository<AdmissionInterview, Guid> interviewRepository,
        IRepository<Application, Guid> applicationRepository)
    {
        _interviewRepository = interviewRepository;
        _applicationRepository = applicationRepository;
    }

    [AbpAuthorize(PermissionNames.Admissions_Interviews_View)]
    public async Task<AdmissionInterviewDto> GetAsync(Guid id)
    {
        var interview = await _interviewRepository
            .GetAll()
            .Include(i => i.Application)
                .ThenInclude(a => a.AppliedGrade)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (interview == null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InterviewNotFound, "Interview not found.");

        return ObjectMapper.Map<AdmissionInterviewDto>(interview);
    }

    [AbpAuthorize(PermissionNames.Admissions_Interviews_View)]
    public async Task<AdmissionInterviewDto> GetByApplicationAsync(Guid applicationId)
    {
        var interview = await _interviewRepository
            .GetAll()
            .Include(i => i.Application)
                .ThenInclude(a => a.AppliedGrade)
            .FirstOrDefaultAsync(i => i.ApplicationId == applicationId);

        if (interview == null)
            return null;

        return ObjectMapper.Map<AdmissionInterviewDto>(interview);
    }

    [AbpAuthorize(PermissionNames.Admissions_Interviews_View)]
    public async Task<PagedResultDto<AdmissionInterviewDto>> GetAllAsync(PagedAndSortedResultRequestDto input)
    {
        var query = _interviewRepository
            .GetAll()
            .Include(i => i.Application)
                .ThenInclude(a => a.AppliedGrade);

        var totalCount = await query.CountAsync();

        var interviews = await query
            .OrderBy(input.Sorting ?? "ScheduledDate DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<AdmissionInterviewDto>(
            totalCount,
            ObjectMapper.Map<List<AdmissionInterviewDto>>(interviews));
    }

    [AbpAuthorize(PermissionNames.Admissions_Interviews_Schedule)]
    public async Task<AdmissionInterviewDto> ScheduleAsync(ScheduleInterviewDto input)
    {
        var application = await _applicationRepository.GetAsync(input.ApplicationId);

        if (application.Status != ApplicationStatus.UnderReview)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidStatusTransition,
                "Application must be under review to schedule an interview.");

        // Check if interview already exists
        var existingInterview = await _interviewRepository
            .FirstOrDefaultAsync(i => i.ApplicationId == input.ApplicationId);

        if (existingInterview != null)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InterviewAlreadyScheduled,
                "An interview has already been scheduled for this application.");

        // Validate minimum notice (ADM-012: 48 hours)
        if (input.ScheduledDate < DateTime.UtcNow.AddDays(MinNoticeDaysRequired))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InsufficientInterviewNotice,
                $"Interview must be scheduled at least {MinNoticeDaysRequired} days in advance.");

        var interview = new AdmissionInterview(
            Guid.NewGuid(),
            input.ApplicationId,
            input.ScheduledDate,
            input.ScheduledTime,
            input.InterviewerUserId,
            input.InterviewerName)
        {
            Location = input.Location,
            MeetingLink = input.MeetingLink,
            Notes = input.Notes
        };

        await _interviewRepository.InsertAsync(interview);

        // Update application status
        application.ScheduleInterview();
        await _applicationRepository.UpdateAsync(application);

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(interview.Id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Interviews_Reschedule)]
    public async Task<AdmissionInterviewDto> RescheduleAsync(Guid id, RescheduleInterviewDto input)
    {
        var interview = await _interviewRepository.GetAsync(id);

        if (interview.Status != InterviewStatus.Scheduled && interview.Status != InterviewStatus.Rescheduled)
            throw new UserFriendlyException(AdmissionsExceptionCodes.CannotRescheduleInterview,
                "Only scheduled interviews can be rescheduled.");

        // Enforce maximum reschedule limit (ADM-012: max 2)
        if (interview.RescheduleCount >= MaxReschedules)
            throw new UserFriendlyException(AdmissionsExceptionCodes.MaxReschedulesExceeded,
                $"Maximum of {MaxReschedules} reschedules allowed. Mark as NoShow instead.");

        // Validate minimum notice
        if (input.NewScheduledDate < DateTime.UtcNow.AddDays(MinNoticeDaysRequired))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InsufficientInterviewNotice,
                $"Interview must be scheduled at least {MinNoticeDaysRequired} days in advance.");

        // Use entity method which increments RescheduleCount
        interview.Reschedule(input.NewScheduledDate, input.NewScheduledTime);

        if (!string.IsNullOrWhiteSpace(input.Location))
            interview.Location = input.Location;

        if (!string.IsNullOrWhiteSpace(input.MeetingLink))
            interview.MeetingLink = input.MeetingLink;

        await _interviewRepository.UpdateAsync(interview);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Interviews_Cancel)]
    public async Task CancelAsync(Guid id, string reason)
    {
        var interview = await _interviewRepository.GetAsync(id);

        if (interview.Status == InterviewStatus.Completed || interview.Status == InterviewStatus.Cancelled)
            throw new UserFriendlyException(AdmissionsExceptionCodes.CannotCancelInterview,
                "This interview cannot be cancelled.");

        interview.Cancel();
        interview.Notes = reason;

        // Revert application status back to UnderReview
        var application = await _applicationRepository.GetAsync(interview.ApplicationId);
        if (application.Status == ApplicationStatus.InterviewScheduled)
        {
            application.RevertToUnderReview();
            await _applicationRepository.UpdateAsync(application);
        }

        await _interviewRepository.UpdateAsync(interview);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Admissions_Interviews_RecordOutcome)]
    public async Task<AdmissionInterviewDto> CompleteAsync(Guid id, CompleteInterviewDto input)
    {
        var interview = await _interviewRepository.GetAsync(id);

        if (interview.Status != InterviewStatus.Scheduled && interview.Status != InterviewStatus.Rescheduled)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InterviewNotScheduled,
                "Interview is not in a schedulable state.");

        interview.Status = InterviewStatus.Completed;
        interview.CompletedDate = DateTime.UtcNow;
        interview.Rating = input.Rating;
        interview.Recommended = input.Recommended;
        interview.Notes = input.Notes;

        await _interviewRepository.UpdateAsync(interview);

        // Update application status
        var application = await _applicationRepository.GetAsync(interview.ApplicationId);
        application.CompleteInterview();
        await _applicationRepository.UpdateAsync(application);

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Interviews_RecordOutcome)]
    public async Task<AdmissionInterviewDto> MarkNoShowAsync(Guid id)
    {
        var interview = await _interviewRepository.GetAsync(id);

        if (interview.Status != InterviewStatus.Scheduled && interview.Status != InterviewStatus.Rescheduled)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InterviewNotScheduled,
                "Interview is not in a schedulable state.");

        interview.MarkNoShow();
        interview.CompletedDate = DateTime.UtcNow;

        await _interviewRepository.UpdateAsync(interview);

        // Revert application status back to UnderReview so it can be rescheduled or decided
        var application = await _applicationRepository.GetAsync(interview.ApplicationId);
        if (application.Status == ApplicationStatus.InterviewScheduled)
        {
            application.RevertToUnderReview();
            await _applicationRepository.UpdateAsync(application);
        }

        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.Admissions_Interviews_View)]
    public async Task<List<TimeSlotDto>> GetAvailableTimeSlotsAsync(DateTime date, long interviewerUserId)
    {
        // Get existing interviews for this interviewer on this date
        var existingInterviews = await _interviewRepository
            .GetAll()
            .Where(i => i.InterviewerUserId == interviewerUserId
                && i.ScheduledDate.Date == date.Date
                && i.Status != InterviewStatus.Cancelled)
            .Select(i => i.ScheduledTime)
            .ToListAsync();

        // Generate time slots (e.g., 30-minute slots from 8:00 to 16:00)
        var slots = new List<TimeSlotDto>();
        var startTime = new TimeSpan(8, 0, 0);
        var endTime = new TimeSpan(16, 0, 0);
        var slotDuration = TimeSpan.FromMinutes(30);

        for (var time = startTime; time < endTime; time = time.Add(slotDuration))
        {
            var isAvailable = !existingInterviews.Any(t =>
                t >= time && t < time.Add(slotDuration) ||
                time >= t && time < t.Add(slotDuration));

            slots.Add(new TimeSlotDto
            {
                Date = date,
                StartTime = time,
                EndTime = time.Add(slotDuration),
                DurationMinutes = (int)slotDuration.TotalMinutes,
                IsAvailable = isAvailable
            });
        }

        return slots;
    }
}
