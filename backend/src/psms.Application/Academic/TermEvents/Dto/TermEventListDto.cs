using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Academic.TermEvents.Dto;

public class TermEventListDto : EntityDto<Guid>
{
    public Guid TermId { get; set; }
    public string TermName { get; set; }
    public string EventName { get; set; }
    public EventType EventType { get; set; }
    public DateTime EventDate { get; set; }
}
