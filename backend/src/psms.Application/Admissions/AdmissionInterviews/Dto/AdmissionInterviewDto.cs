using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Admissions.AdmissionInterviews.Dto;

/// <summary>
/// DTO for admission interview information.
/// Aligned with AdmissionInterview entity.
/// </summary>
public class AdmissionInterviewDto : CreationAuditedEntityDto<Guid>
{
    public Guid ApplicationId { get; set; }
    public string ApplicationNumber { get; set; }
    public string ApplicantName { get; set; }
    public string GradeName { get; set; }

    // Schedule Information (ADM-012)
    public DateTime ScheduledDate { get; set; }
    public TimeSpan ScheduledTime { get; set; }
    public string Location { get; set; }
    public string MeetingLink { get; set; }

    // Interviewer
    public long InterviewerUserId { get; set; }
    public string InterviewerName { get; set; }

    // Status
    public InterviewStatus Status { get; set; }
    public string StatusDisplayName => Status.ToString();

    // Outcome (ADM-013)
    public DateTime? CompletedDate { get; set; }
    public int? Rating { get; set; }
    public bool? Recommended { get; set; }
    public string Notes { get; set; }

    // Flags
    public bool IsUpcoming => Status == InterviewStatus.Scheduled && ScheduledDate >= DateTime.Today;
    public bool IsPastDue => Status == InterviewStatus.Scheduled && ScheduledDate < DateTime.Today;
    public bool CanComplete => Status == InterviewStatus.Scheduled || Status == InterviewStatus.Rescheduled;
    public bool CanReschedule => Status == InterviewStatus.Scheduled;
    public bool CanCancel => Status == InterviewStatus.Scheduled || Status == InterviewStatus.Rescheduled;
}
