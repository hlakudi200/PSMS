using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Academic.Attendances.Dto;

public class GetAttendanceInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? TeacherId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public AttendanceStatus? Status { get; set; }
}
