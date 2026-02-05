using System;

namespace psms.Admissions.AdmissionInterviews.Dto;

/// <summary>
/// DTO representing an available time slot for interviews.
/// </summary>
public class TimeSlotDto
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsAvailable { get; set; }
    public string DisplayTime => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
}
