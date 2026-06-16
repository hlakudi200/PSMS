using System;
using System.Collections.Generic;

namespace psms.Learning.OnlineLessons.Dto;

/// <summary>Distinct attendee roll-call for an in-app live class (LC-05).</summary>
public class LiveClassAttendanceDto
{
    public int DistinctAttendeeCount { get; set; }
    public List<LiveClassAttendeeDto> Attendees { get; set; } = new List<LiveClassAttendeeDto>();
}

public class LiveClassAttendeeDto
{
    public string Identity { get; set; }
    public string DisplayName { get; set; }
    public DateTime FirstJoinedAt { get; set; }
    public DateTime? LastLeftAt { get; set; }
}
