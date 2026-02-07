using System;

namespace psms.Academic.Timetables.Dto;

public class UpdateTimetableDto
{
    public DateTime? EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
}
