using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// Input DTO for updating an online lesson. All fields nullable for partial updates.
/// Only allowed when Status == Scheduled. Times move through Reschedule, not here,
/// so a time change always goes through OL-001 and notifies students.
/// </summary>
public class UpdateOnlineLessonDto
{
    /// <summary>Moves the lesson to another class-subject the teacher teaches. Null = unchanged.</summary>
    public Guid? ClassSubjectId { get; set; }

    /// <summary>
    /// Changes the platform. Null = unchanged. Switching to an external
    /// platform requires MeetingLink; switching to InApp clears the external details.
    /// </summary>
    [EnumDataType(typeof(OnlinePlatform))]
    public OnlinePlatform? Platform { get; set; }

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
