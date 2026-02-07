using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Timetables.Dto;

public class CreateTimetableDto
{
    [Required]
    public Guid ClassId { get; set; }

    [Required]
    public DateTime EffectiveDate { get; set; }

    public DateTime? EndDate { get; set; }
}
