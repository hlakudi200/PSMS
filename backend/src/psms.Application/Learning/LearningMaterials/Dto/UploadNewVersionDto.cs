using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Learning.LearningMaterials.Dto;

/// <summary>
/// Input for "Upload new version" of an existing learning material, posted
/// after the new file has been uploaded directly to storage via an upload
/// ticket (bytes do not pass through the server).
/// </summary>
public class UploadNewVersionDto
{
    [Required]
    public Guid LearningMaterialId { get; set; }

    /// <summary>
    /// Required teacher-supplied note describing what changed (LM-003).
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string ChangeDescription { get; set; }

    /// <summary>Public URL of the already-uploaded new file.</summary>
    [Required]
    [StringLength(1000)]
    public string FileUrl { get; set; }

    [Required]
    [StringLength(260)]
    public string FileName { get; set; }

    public long FileSizeBytes { get; set; }

    [StringLength(150)]
    public string ContentType { get; set; }
}
