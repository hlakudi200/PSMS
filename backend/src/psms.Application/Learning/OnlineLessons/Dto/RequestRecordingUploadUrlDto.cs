using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// Step 1 of the direct recording upload (SF-02): the client asks the server
/// for a one-time signed upload URL. The server validates lesson ownership +
/// status + file extension before minting the ticket; the bytes are then PUT
/// straight to storage by the browser.
/// </summary>
public class RequestRecordingUploadUrlDto
{
    [Required]
    public Guid LessonId { get; set; }

    /// <summary>
    /// Original file name — used for the extension whitelist check and to
    /// preserve the extension on the generated object key.
    /// </summary>
    [Required]
    [StringLength(260)]
    public string FileName { get; set; }
}
