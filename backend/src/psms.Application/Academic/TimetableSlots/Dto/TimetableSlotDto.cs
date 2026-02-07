using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.TimetableSlots.Dto;

public class TimetableSlotDto : EntityDto<Guid>
{
    public Guid TimetableId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public int PeriodNumber { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; }
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; }
    public string RoomNumber { get; set; }
}
