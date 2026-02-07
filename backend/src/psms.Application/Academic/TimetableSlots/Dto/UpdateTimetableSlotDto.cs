using System;

namespace psms.Academic.TimetableSlots.Dto;

public class UpdateTimetableSlotDto
{
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public Guid? SubjectId { get; set; }
    public Guid? TeacherId { get; set; }
    public string RoomNumber { get; set; }
}
