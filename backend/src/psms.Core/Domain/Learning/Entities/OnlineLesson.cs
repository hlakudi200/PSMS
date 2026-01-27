using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Learning.Entities
{
    /// <summary>
    /// Represents an online/virtual lesson session
    /// </summary>
    [Table("OnlineLessons")]
    public class OnlineLesson : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
    {
        public const int MaxTitleLength = 200;
        public const int MaxDescriptionLength = 2000;
        public const int MaxMeetingLinkLength = 500;
        public const int MaxMeetingIdLength = 100;
        public const int MaxPasswordLength = 50;
        public const int MaxRecordingUrlLength = 500;

        /// <summary>
        /// Tenant identifier for multi-tenancy
        /// </summary>
        public int? TenantId { get; set; }

        /// <summary>
        /// Reference to the class-subject
        /// </summary>
        [Required]
        public Guid ClassSubjectId { get; set; }

        /// <summary>
        /// Title of the lesson
        /// </summary>
        [Required]
        [StringLength(MaxTitleLength)]
        public string Title { get; set; }

        /// <summary>
        /// Description of the lesson
        /// </summary>
        [StringLength(MaxDescriptionLength)]
        public string Description { get; set; }

        /// <summary>
        /// Platform used (Zoom, Teams, Google Meet, etc.)
        /// </summary>
        [Required]
        public OnlinePlatform Platform { get; set; }

        /// <summary>
        /// Meeting link URL
        /// </summary>
        [Required]
        [StringLength(MaxMeetingLinkLength)]
        public string MeetingLink { get; set; }

        /// <summary>
        /// Meeting ID (platform-specific)
        /// </summary>
        [StringLength(MaxMeetingIdLength)]
        public string MeetingId { get; set; }

        /// <summary>
        /// Meeting password if required
        /// </summary>
        [StringLength(MaxPasswordLength)]
        public string MeetingPassword { get; set; }

        /// <summary>
        /// Scheduled start date and time
        /// </summary>
        [Required]
        public DateTime ScheduledStartTime { get; set; }

        /// <summary>
        /// Scheduled end date and time
        /// </summary>
        [Required]
        public DateTime ScheduledEndTime { get; set; }

        /// <summary>
        /// Duration in minutes
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// Actual start time
        /// </summary>
        public DateTime? ActualStartTime { get; set; }

        /// <summary>
        /// Actual end time
        /// </summary>
        public DateTime? ActualEndTime { get; set; }

        /// <summary>
        /// Status of the lesson
        /// </summary>
        [Required]
        public OnlineLessonStatus Status { get; set; }

        /// <summary>
        /// Teacher hosting the lesson
        /// </summary>
        [Required]
        public long HostTeacherUserId { get; set; }

        /// <summary>
        /// Number of students who attended
        /// </summary>
        public int? AttendeeCount { get; set; }

        /// <summary>
        /// URL to the recording if available
        /// </summary>
        [StringLength(MaxRecordingUrlLength)]
        public string RecordingUrl { get; set; }

        /// <summary>
        /// Whether recording is available
        /// </summary>
        public bool HasRecording { get; set; }

        /// <summary>
        /// Whether the lesson is recurring
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        /// Recurrence pattern (JSON)
        /// </summary>
        public string RecurrencePattern { get; set; }

        /// <summary>
        /// Soft delete flag
        /// </summary>
        public bool IsDeleted { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(ClassSubjectId))]
        public virtual ClassSubject ClassSubject { get; set; }

        /// <summary>
        /// Protected constructor for EF Core
        /// </summary>
        protected OnlineLesson()
        {
        }

        /// <summary>
        /// Public constructor with required parameters
        /// </summary>
        public OnlineLesson(
            Guid id,
            int? tenantId,
            Guid classSubjectId,
            string title,
            OnlinePlatform platform,
            string meetingLink,
            DateTime scheduledStartTime,
            DateTime scheduledEndTime,
            long hostTeacherUserId) : this()
        {
            Id = id;
            TenantId = tenantId;
            ClassSubjectId = classSubjectId;
            Title = title;
            Platform = platform;
            MeetingLink = meetingLink;
            ScheduledStartTime = scheduledStartTime;
            ScheduledEndTime = scheduledEndTime;
            HostTeacherUserId = hostTeacherUserId;
            DurationMinutes = (int)(scheduledEndTime - scheduledStartTime).TotalMinutes;
            Status = OnlineLessonStatus.Scheduled;
            HasRecording = false;
            IsRecurring = false;
            IsDeleted = false;
        }

        /// <summary>
        /// Starts the lesson
        /// </summary>
        public void Start()
        {
            if (Status != OnlineLessonStatus.Scheduled)
                throw new InvalidOperationException("Only scheduled lessons can be started.");

            Status = OnlineLessonStatus.InProgress;
            ActualStartTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Ends the lesson
        /// </summary>
        public void End(int attendeeCount)
        {
            if (Status != OnlineLessonStatus.InProgress)
                throw new InvalidOperationException("Only in-progress lessons can be ended.");

            Status = OnlineLessonStatus.Completed;
            ActualEndTime = DateTime.UtcNow;
            AttendeeCount = attendeeCount;
        }

        /// <summary>
        /// Cancels the lesson
        /// </summary>
        public void Cancel()
        {
            if (Status == OnlineLessonStatus.Completed)
                throw new InvalidOperationException("Completed lessons cannot be cancelled.");

            Status = OnlineLessonStatus.Cancelled;
        }

        /// <summary>
        /// Reschedules the lesson
        /// </summary>
        public void Reschedule(DateTime newStartTime, DateTime newEndTime)
        {
            if (Status == OnlineLessonStatus.Completed || Status == OnlineLessonStatus.InProgress)
                throw new InvalidOperationException("Cannot reschedule completed or in-progress lessons.");

            ScheduledStartTime = newStartTime;
            ScheduledEndTime = newEndTime;
            DurationMinutes = (int)(newEndTime - newStartTime).TotalMinutes;
        }

        /// <summary>
        /// Adds recording URL
        /// </summary>
        public void AddRecording(string recordingUrl)
        {
            RecordingUrl = recordingUrl;
            HasRecording = true;
        }
    }
}
