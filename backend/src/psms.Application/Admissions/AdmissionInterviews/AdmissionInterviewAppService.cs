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
using psms.Authorization.Roles;
using psms.Authorization.Users;
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
    private readonly UserManager _userManager;
    private readonly RoleManager _roleManager;

    private const int MinNoticeDaysRequired = 2; // ADM-012: 48 hours minimum
    private const int MaxReschedules = 2; // ADM-012: Maximum 2 reschedules

    public AdmissionInterviewAppService(
        IRepository<AdmissionInterview, Guid> interviewRepository,
        IRepository<Application, Guid> applicationRepository,
        UserManager userManager,
        RoleManager roleManager)
    {
        _interviewRepository = interviewRepository;
        _applicationRepository = applicationRepository;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// The interviews this person is down to conduct.
    /// <para>
    /// A teacher now owns the Interview step of the admissions workflow, and
    /// had no way to find out they were down for one: the interview could be
    /// read through the application, and a teacher cannot read applications.
    /// This is deliberately only ever the caller's own — it asks nothing about
    /// permissions beyond being able to see an interview at all, because being
    /// named as the interviewer is the authority.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Admissions_Interviews_View)]
    public async Task<ListResultDto<AdmissionInterviewDto>> GetMineAsync(bool includePast = false)
    {
        var userId = AbpSession.UserId;
        if (!userId.HasValue)
            return new ListResultDto<AdmissionInterviewDto>(new List<AdmissionInterviewDto>());

        var today = DateTime.UtcNow.Date;

        var mine = await _interviewRepository
            .GetAll()
            .Include(i => i.Application).ThenInclude(a => a.AppliedGrade)
            .Where(i => i.InterviewerUserId == userId.Value)
            /* Past interviews are hidden unless asked for, but one that has
               come and gone without an outcome is not "past" — it is the thing
               most needing attention, so it stays on the list. */
            .WhereIf(!includePast,
                i => i.ScheduledDate >= today
                     || i.Status == InterviewStatus.Scheduled
                     || i.Status == InterviewStatus.Rescheduled)
            .OrderBy(i => i.ScheduledDate)
            .ThenBy(i => i.ScheduledTime)
            .ToListAsync();

        return new ListResultDto<AdmissionInterviewDto>(
            ObjectMapper.Map<List<AdmissionInterviewDto>>(mine));
    }

    /// <summary>
    /// Who may be put down to conduct an interview: everybody holding a role
    /// the school has granted <c>Admissions.Interviews.Conduct</c>, with how
    /// busy they already are, so whoever schedules can spread the load.
    /// </summary>
    [AbpAuthorize(PermissionNames.Admissions_Interviews_Schedule)]
    public async Task<ListResultDto<InterviewerDto>> GetInterviewersAsync()
    {
        var found = new Dictionary<long, InterviewerDto>();

        foreach (var role in await _roleManager.Roles.ToListAsync())
        {
            if (!await _roleManager.IsGrantedAsync(role.Id, PermissionNames.Admissions_Interviews_Conduct))
                continue;

            foreach (var user in await _userManager.GetUsersInRoleAsync(role.NormalizedName))
            {
                if (!user.IsActive)
                    continue;

                if (found.TryGetValue(user.Id, out var already))
                {
                    already.Roles.Add(role.DisplayName ?? role.Name);
                    continue;
                }

                found[user.Id] = new InterviewerDto
                {
                    UserId = user.Id,
                    Name = user.FullName,
                    EmailAddress = user.EmailAddress,
                    Roles = new List<string> { role.DisplayName ?? role.Name },
                };
            }
        }

        var ids = found.Keys.ToList();
        var loads = await _interviewRepository
            .GetAll()
            .Where(i => ids.Contains(i.InterviewerUserId)
                        && (i.Status == InterviewStatus.Scheduled || i.Status == InterviewStatus.Rescheduled))
            .GroupBy(i => i.InterviewerUserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var load in loads)
            found[load.UserId].UpcomingInterviews = load.Count;

        return new ListResultDto<InterviewerDto>(
            found.Values.OrderBy(i => i.Name).ToList());
    }

    /// <summary>
    /// Resolves the person an interview is being booked against, and refuses
    /// anyone who could not conduct it.
    /// <para>
    /// The id and the name both used to come from the request, unchecked and
    /// unrelated. Scheduling an interview with a colleague who holds no
    /// interview permission produced a booking that looked fine and could
    /// never be completed, and the failure surfaced on the day.
    /// </para>
    /// </summary>
    private async Task<User> ResolveInterviewerAsync(long interviewerUserId)
    {
        var user = await _userManager.FindByIdAsync(interviewerUserId.ToString());

        if (user == null || !user.IsActive)
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidInterviewSchedule,
                "That interviewer is not someone at this school.");

        if (!await _userManager.IsGrantedAsync(user.Id, PermissionNames.Admissions_Interviews_Conduct))
            throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidInterviewSchedule,
                $"{user.FullName} is not able to conduct admission interviews. Choose somebody else, or ask an administrator to give them the role.");

        return user;
    }

    /// <summary>
    /// Recording what came of an interview belongs to the person who conducted
    /// it, or to the admissions staff who arrange them. Anyone else holding the
    /// permission is writing somebody else's judgement under their name.
    /// </summary>
    private async Task AssertMayRecordOutcomeAsync(AdmissionInterview interview)
    {
        if (AbpSession.UserId.HasValue && interview.InterviewerUserId == AbpSession.UserId.Value)
            return;

        if (await PermissionChecker.IsGrantedAsync(PermissionNames.Admissions_Interviews_Schedule))
            return;

        throw new UserFriendlyException(AdmissionsExceptionCodes.InvalidInterviewSchedule,
            $"This interview is {interview.InterviewerName}'s to record.");
    }

    [AbpAuthorize(PermissionNames.Admissions_Interviews_View)]
    public async Task<AdmissionInterviewDto> GetAsync(Guid id)
    {
        var interview = await _interviewRepository
            .GetAll()
            .Include(i => i.Application)
                .ThenInclude(a => a.AppliedGrade)
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == AbpSession.TenantId);

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
            .FirstOrDefaultAsync(i => i.ApplicationId == applicationId && i.TenantId == AbpSession.TenantId);

        if (interview == null)
            return null;

        return ObjectMapper.Map<AdmissionInterviewDto>(interview);
    }

    [AbpAuthorize(PermissionNames.Admissions_Interviews_View)]
    public async Task<PagedResultDto<AdmissionInterviewDto>> GetAllAsync(GetAdmissionInterviewsInput input)
    {
        var query = _interviewRepository
            .GetAll()
            .Include(i => i.Application)
                .ThenInclude(a => a.AppliedGrade)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                i => i.Application.ProspectiveStudentFirstName.ToLower().Contains(input.Keyword.ToLower())
                    || i.Application.ProspectiveStudentLastName.ToLower().Contains(input.Keyword.ToLower()));

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

        // The name is the school's record of this person, not whatever the
        // caller typed — the two used to be able to disagree.
        var interviewer = await ResolveInterviewerAsync(input.InterviewerUserId);

        var interview = new AdmissionInterview(
            Guid.NewGuid(),
            input.ApplicationId,
            input.ScheduledDate,
            input.ScheduledTime,
            interviewer.Id,
            interviewer.FullName)
        {
            TenantId = AbpSession.TenantId,
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

        await AssertMayRecordOutcomeAsync(interview);

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

        await AssertMayRecordOutcomeAsync(interview);

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
            .Where(i => i.TenantId == AbpSession.TenantId
                && i.InterviewerUserId == interviewerUserId
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
