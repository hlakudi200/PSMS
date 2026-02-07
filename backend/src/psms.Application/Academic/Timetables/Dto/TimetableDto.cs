using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.Timetables.Dto;

public class TimetableDto : FullAuditedEntityDto<Guid>
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public int SlotCount { get; set; }
}
