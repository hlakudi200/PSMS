using System;

namespace psms.Academic.Attendances.Dto;

public class AttendanceSummaryDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public int TotalDays { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int ExcusedCount { get; set; }
    public int SickLeaveCount { get; set; }
    public decimal AttendancePercentage { get; set; }
}
