using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.TimetableSlots.Dto;

public class CreateTimetableSlotDto
{
    [Required]
    public Guid TimetableId { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public int PeriodNumber { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    public Guid SubjectId { get; set; }

    [Required]
    public Guid TeacherId { get; set; }

    public string RoomNumber { get; set; }
}
