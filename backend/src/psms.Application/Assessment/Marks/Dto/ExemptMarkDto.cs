using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Marks.Dto;

/// <summary>
/// RC-23. Why a learner is excused from a task.
/// <para>
/// National Protocol §8(9): a learner who cannot offer the Physical Education
/// Task "may be exempted … provided a valid medical reason is submitted", and
/// their Life Orientation marks are then "recalculated in terms of four tasks".
/// </para>
/// </summary>
public class ExemptMarkDto
{
    /// <summary>
    /// The school's note of what the exemption was granted on — a reference to
    /// the documentation it holds.
    /// <para>
    /// Not a place to transcribe medical detail: that is special personal
    /// information under POPIA §26 and belongs in the school's own file.
    /// </para>
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Reason { get; set; }
}
