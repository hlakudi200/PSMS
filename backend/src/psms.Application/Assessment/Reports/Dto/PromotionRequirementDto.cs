namespace psms.Assessment.Reports.Dto;

/// <summary>
/// RC-16. One clause of a grade's promotion requirements, and whether this
/// learner satisfies it.
/// </summary>
public class PromotionRequirementDto
{
    /// <summary>The policy reference, e.g. "NPPPPR §21(1)(a)".</summary>
    public string Clause { get; set; }

    /// <summary>What the clause requires, in the policy's own terms.</summary>
    public string Description { get; set; }

    public bool IsMet { get; set; }

    /// <summary>
    /// What the learner actually achieved against it — the subjects and marks
    /// that satisfied it, or what is short.
    /// </summary>
    public string Detail { get; set; }
}
