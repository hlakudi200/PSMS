using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.TermEvents.Dto;

public class CreateTermEventDto
{
    [Required]
    public Guid TermId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string EventName { get; set; }

    [Required]
    public EventType EventType { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }
}
