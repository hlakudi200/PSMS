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
    /// Optional teacher-supplied note describing what changed (LM-003).
    /// </summary>
    [StringLength(500)]
    public string ChangeDescription { get; set; }

    /// <summary>
    /// Storage object key from RequestVersionUploadUrl. Validated + measured
    /// server-side; the public URL is derived (not trusted from the client).
    /// </summary>
    [Required]
    [StringLength(500)]
    public string ObjectKey { get; set; }

    [Required]
    [StringLength(260)]
    public string FileName { get; set; }
}
