using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Admissions.AdmissionInterviews.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Admissions.AdmissionInterviews;

/// <summary>
/// Service for managing admission interviews.
/// Implements ADM-011 to ADM-013.
/// </summary>
public interface IAdmissionInterviewAppService : IApplicationService
{
    /// <summary>
    /// Gets an interview by ID.
    /// </summary>
    Task<AdmissionInterviewDto> GetAsync(Guid id);

    /// <summary>
    /// Gets the interview for an application.
    /// </summary>
    Task<AdmissionInterviewDto> GetByApplicationAsync(Guid applicationId);

    /// <summary>
    /// Gets all interviews with filters.
    /// </summary>
    Task<PagedResultDto<AdmissionInterviewDto>> GetAllAsync(GetAdmissionInterviewsInput input);

    /// <summary>
    /// Schedules an interview for an application.
    /// </summary>
    Task<AdmissionInterviewDto> ScheduleAsync(ScheduleInterviewDto input);

    /// <summary>
    /// Reschedules an existing interview.
    /// </summary>
    Task<AdmissionInterviewDto> RescheduleAsync(Guid id, RescheduleInterviewDto input);

    /// <summary>
    /// Cancels an interview.
    /// </summary>
    Task CancelAsync(Guid id, string reason);

    /// <summary>
    /// Completes an interview with outcome.
    /// </summary>
    Task<AdmissionInterviewDto> CompleteAsync(Guid id, CompleteInterviewDto input);

    /// <summary>
    /// Marks interview as no-show.
    /// </summary>
    Task<AdmissionInterviewDto> MarkNoShowAsync(Guid id);

    /// <summary>
    /// Gets available time slots for scheduling.
    /// </summary>
    Task<List<TimeSlotDto>> GetAvailableTimeSlotsAsync(DateTime date, long interviewerUserId);
}
