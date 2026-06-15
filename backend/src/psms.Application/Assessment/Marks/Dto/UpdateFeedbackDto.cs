using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Marks.Dto;

/// <summary>
/// Input for editing a mark's feedback after publish (TF-004/TF-005).
/// </summary>
public class UpdateFeedbackDto
{
    [StringLength(2000)]
    public string Feedback { get; set; }
}

/// <summary>
/// One entry in a mark's feedback edit history (serialized to
/// Mark.FeedbackHistory as a JSON array).
/// </summary>
public class FeedbackEditEntry
{
    public DateTime At { get; set; }
    public long? ByUserId { get; set; }
    public string Previous { get; set; }
}
