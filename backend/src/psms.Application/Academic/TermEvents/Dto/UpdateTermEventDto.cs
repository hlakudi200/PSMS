using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.TermEvents.Dto;

public class UpdateTermEventDto
{
    [StringLength(200, MinimumLength = 2)]
    public string EventName { get; set; }

    public EventType? EventType { get; set; }

    public DateTime? EventDate { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }
}
