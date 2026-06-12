using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// Multipart upload payload for attaching a recording to a completed
/// lesson. Mirrors the LearningMaterials upload shape so the same
/// pre-signed-blob future migration path applies to both.
/// </summary>
public class UploadRecordingDto
{
    /// <summary>
    /// Lesson the recording belongs to. Must be in Completed status and
    /// owned by the calling teacher.
    /// </summary>
    [Required]
    public Guid LessonId { get; set; }

    /// <summary>
    /// The recording file. Extension and size are validated server-side
    /// (mp4 / mov / avi / webm, 5 GB cap — see OL-003).
    /// </summary>
    [Required]
    public IFormFile File { get; set; }
}
