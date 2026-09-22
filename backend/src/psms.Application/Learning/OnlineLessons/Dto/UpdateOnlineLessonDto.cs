using System;
using System.Collections.Generic;

namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// Input DTO for updating an online lesson. All fields nullable for partial updates.
/// Only allowed when Status == Scheduled. Times move through Reschedule, not here,
/// so a time change always goes through OL-001 and notifies students.
/// </summary>
public class UpdateOnlineLessonDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string MeetingLink { get; set; }
    public string MeetingId { get; set; }
    public string MeetingPassword { get; set; }

    /// <summary>
    /// Replaces the lesson's pre-lesson materials. Null leaves them unchanged;
    /// an empty list detaches all of them.
    /// </summary>
    public List<Guid> MaterialIds { get; set; }
}
