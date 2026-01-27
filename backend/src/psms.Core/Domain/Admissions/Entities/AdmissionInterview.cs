using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Admissions.Entities
{
    /// <summary>
    /// Represents an admission interview for an application
    /// </summary>
    [Table("AdmissionInterviews")]
    public class AdmissionInterview : CreationAuditedEntity<Guid>
    {
        public const int MaxNameLength = 100;
        public const int MaxLocationLength = 200;
        public const int MaxMeetingLinkLength = 500;
        public const int MaxNotesLength = 2000;

        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public DateTime ScheduledDate { get; set; }

        [Required]
        public TimeSpan ScheduledTime { get; set; }

        [Required]
        public long InterviewerUserId { get; set; }

        [StringLength(MaxNameLength)]
        public string InterviewerName { get; set; }

        [StringLength(MaxLocationLength)]
        public string Location { get; set; }

        [StringLength(MaxMeetingLinkLength)]
        public string MeetingLink { get; set; }

        [Required]
        public InterviewStatus Status { get; set; }

        [StringLength(MaxNotesLength)]
        public string Notes { get; set; }

        /// <summary>
        /// Rating from 1-5
        /// </summary>
        public int? Rating { get; set; }

        public bool? Recommended { get; set; }

        public DateTime? CompletedDate { get; set; }

        [ForeignKey(nameof(ApplicationId))]
        public virtual Application Application { get; set; }

        protected AdmissionInterview() { }

        public AdmissionInterview(Guid id, Guid applicationId, DateTime scheduledDate,
            TimeSpan scheduledTime, long interviewerUserId, string interviewerName)
        {
            Id = id;
            ApplicationId = applicationId;
            ScheduledDate = scheduledDate;
            ScheduledTime = scheduledTime;
            InterviewerUserId = interviewerUserId;
            InterviewerName = interviewerName;
            Status = InterviewStatus.Scheduled;
        }

        public void Complete(int rating, bool recommended, string notes = null)
        {
            Status = InterviewStatus.Completed;
            Rating = rating;
            Recommended = recommended;
            Notes = notes;
            CompletedDate = DateTime.UtcNow;
        }

        public void Reschedule(DateTime newDate, TimeSpan newTime)
        {
            ScheduledDate = newDate;
            ScheduledTime = newTime;
            Status = InterviewStatus.Rescheduled;
        }

        public void Cancel() => Status = InterviewStatus.Cancelled;

        public void MarkNoShow() => Status = InterviewStatus.NoShow;
    }
}
