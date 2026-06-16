using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// Posted after the recording file has been uploaded directly to storage
/// via a ticket from RequestRecordingUploadUrl (the bytes never pass through
/// this server — see SF-02). Mirrors the LearningMaterials direct-upload
/// shape: the client supplies only the object key; the server validates it,
/// HEADs the real size/type, and derives the public URL.
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
    /// Storage object key from RequestRecordingUploadUrl. Validated +
    /// measured server-side (mp4 / mov / avi / webm, 5 GB cap — see OL-003);
    /// the public URL is derived, not trusted from the client.
    /// </summary>
    [Required]
    [StringLength(500)]
    public string ObjectKey { get; set; }

    /// <summary>Original file name (used for the extension check).</summary>
    [Required]
    [StringLength(260)]
    public string FileName { get; set; }
}
