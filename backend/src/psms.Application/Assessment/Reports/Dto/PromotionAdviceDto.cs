using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// RC-16. What the national promotion requirements make of a learner's year.
/// <para>
/// Advice, not a decision. NPPPPR §(2b) requires a special meeting of subject
/// staff, and then a meeting with the parent, before a learner is retained — so
/// this says whether the requirements are met and exactly which are not, and a
/// person records the outcome.
/// </para>
/// </summary>
public class PromotionAdviceDto
{
    public Guid ReportId { get; set; }

    public SouthAfricanGradeLevel GradeLevel { get; set; }

    public string GradeName { get; set; }

    /// <summary>
    /// Whether promotion can be evaluated at all. The decision belongs on the
    /// year-end card, which is the one carrying the composite marks for the
    /// year; there is nothing to evaluate on a term card.
    /// </summary>
    public bool IsEvaluable { get; set; }

    /// <summary>Why not, when it is not.</summary>
    public string NotEvaluableReason { get; set; }

    public bool MeetsRequirements { get; set; }

    /// <summary>
    /// What the rules point to. Progression — moving a learner on despite not
    /// meeting the requirements — is a judgement the rules cannot make, so it is
    /// never recommended, only recorded.
    /// </summary>
    public PromotionDecision? Recommended { get; set; }

    /// <summary>Every requirement for this grade, met or not, in policy order.</summary>
    public List<PromotionRequirementDto> Requirements { get; set; } = new();

    /// <summary>What has actually been recorded on the report, if anything.</summary>
    public PromotionDecision? Recorded { get; set; }

    public Guid? PromotedToGradeId { get; set; }

    public string PromotedToGradeName { get; set; }

    /// <summary>
    /// The reason stored with the recorded decision, so the screen can show it
    /// and send it back unchanged rather than erasing it.
    /// </summary>
    public string PromotionReason { get; set; }

    /// <summary>
    /// The grades the learner could be moved into — the next grade for a
    /// promotion, and the one they are in for a retention.
    /// </summary>
    public List<PromotionGradeOptionDto> GradeOptions { get; set; } = new();
}
