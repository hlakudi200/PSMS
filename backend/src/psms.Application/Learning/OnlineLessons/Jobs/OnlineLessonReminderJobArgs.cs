using System;

namespace psms.Learning.OnlineLessons.Jobs;

/// <summary>
/// A pending student reminder for one online lesson. The job runs without a
/// session, so TenantId travels in the args. ExpectedStartUtc is the start
/// time the reminder was scheduled against: if the lesson has since been
/// rescheduled the stored start no longer matches and the job does nothing,
/// because the reschedule queued fresh reminders for the new time.
/// </summary>
[Serializable]
public class OnlineLessonReminderJobArgs
{
    public int? TenantId { get; set; }
    public Guid LessonId { get; set; }
    public DateTime ExpectedStartUtc { get; set; }
    public int MinutesBefore { get; set; }
}
