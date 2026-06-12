using psms.Domain.Shared.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Assessments.Dto;

/// <summary>
/// A single multiple-choice question supplied inline when creating an
/// assessment together with its questions (QA-001). The full structure is
/// re-validated server-side in <see cref="AssessmentAppService"/> —
/// 2-6 options, exactly one correct option, bounded text — so a crafted
/// request cannot bypass the rules the client enforces.
/// </summary>
public class CreateAssessmentQuestionInlineDto
{
    [Required]
    [StringLength(1000, MinimumLength = 10)]
    public string QuestionText { get; set; }

    /// <summary>Marks allocated to this question. Defaults to 1.</summary>
    [Range(0.01, 9999)]
    public decimal Marks { get; set; } = 1;

    /// <summary>2-6 answer options.</summary>
    [Required]
    public List<string> Options { get; set; } = new();

    /// <summary>Zero-based index of the single correct option.</summary>
    [Range(0, 5)]
    public int CorrectOptionIndex { get; set; }

    [StringLength(2000)]
    public string Explanation { get; set; }

    public CognitiveLevel? CognitiveLevel { get; set; }
}
