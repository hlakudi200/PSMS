using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// Input DTO for rescheduling an online lesson.
/// </summary>
public class RescheduleOnlineLessonDto
{
    [Required]
    public DateTime NewStartTime { get; set; }

    [Required]
    public DateTime NewEndTime { get; set; }
}
