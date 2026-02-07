using System.ComponentModel.DataAnnotations;

namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// Input DTO for adding a recording URL to an online lesson.
/// </summary>
public class AddRecordingDto
{
    [Required]
    [StringLength(500)]
    public string RecordingUrl { get; set; }
}
