using psms.Domain.Shared.Enums;
using System;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// RC-16. A grade the learner could be moved into, for the screen that records
/// the decision.
/// </summary>
public class PromotionGradeOptionDto
{
    public Guid Id { get; set; }

    public string GradeName { get; set; }

    public SouthAfricanGradeLevel GradeLevel { get; set; }

    /// <summary>
    /// Whether this is the grade immediately after the learner's current one —
    /// the only destination a promotion can have.
    /// </summary>
    public bool IsNextGrade { get; set; }

    /// <summary>Whether this is the grade the learner is in now.</summary>
    public bool IsCurrentGrade { get; set; }
}
