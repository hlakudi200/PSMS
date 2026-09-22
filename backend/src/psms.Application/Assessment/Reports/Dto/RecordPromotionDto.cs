using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// RC-16. Input for recording the promotion decision on a year-end report card.
/// </summary>
public class RecordPromotionDto
{
    [Required]
    public Guid ReportId { get; set; }

    [Required]
    public PromotionDecision Decision { get; set; }

    /// <summary>
    /// The grade the learner moves into. Required when the learner is promoted
    /// or progressed; ignored when they are retained, since they stay where they
    /// are.
    /// </summary>
    public Guid? PromotedToGradeId { get; set; }

    /// <summary>
    /// Why, when the decision does not follow the requirements. NPPPPR §(2b)
    /// puts a retention behind a staff meeting and then a meeting with the
    /// parent, and §(2b)(c) requires the decision to be reflected on the report
    /// card — a decision that departs from the rules should say what it was
    /// based on.
    /// </summary>
    [StringLength(MaxReasonLength)]
    public string Reason { get; set; }

    public const int MaxReasonLength = 1000;
}
