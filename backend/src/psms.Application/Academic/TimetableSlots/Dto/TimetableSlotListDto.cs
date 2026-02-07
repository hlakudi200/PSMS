using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.TimetableSlots.Dto;

public class TimetableSlotListDto : EntityDto<Guid>
{
    public DayOfWeek DayOfWeek { get; set; }
    public int PeriodNumber { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string SubjectName { get; set; }
    public string TeacherName { get; set; }
    public string RoomNumber { get; set; }
}
