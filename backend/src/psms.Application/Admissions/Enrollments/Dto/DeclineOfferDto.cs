using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.Enrollments.Dto;

/// <summary>
/// A family turning down a place (ADM-026).
/// <para>
/// Worth asking for and worth recording: a school that knows a place has been
/// turned down can give it to somebody on the waiting list, and a school that
/// knows why can do something about the reason.
/// </para>
/// </summary>
public class DeclineOfferDto
{
    [Required]
    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Said plainly, because declining ends the application: the family cannot
    /// undo it themselves.
    /// </summary>
    [Required]
    public bool ConfirmDecline { get; set; }

    /// <summary>Why, in the family's words. Optional — nobody owes a reason.</summary>
    [MaxLength(1000)]
    public string Reason { get; set; }
}
