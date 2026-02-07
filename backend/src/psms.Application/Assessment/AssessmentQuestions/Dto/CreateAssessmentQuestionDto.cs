using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.AssessmentQuestions.Dto;

/// <summary>
/// Input DTO for creating an assessment question.
/// </summary>
public class CreateAssessmentQuestionDto
{
    [Required]
    public Guid AssessmentId { get; set; }

    [Required]
    [Range(1, 999)]
    public int QuestionNumber { get; set; }

    [Required]
    public QuestionType QuestionType { get; set; }

    [Required]
    [StringLength(4000)]
    public string QuestionText { get; set; }

    [Required]
    [Range(0.01, 99999)]
    public decimal Marks { get; set; }

    [StringLength(4000)]
    public string Options { get; set; }

    [StringLength(2000)]
    public string CorrectAnswer { get; set; }

    [StringLength(2000)]
    public string Explanation { get; set; }

    public CognitiveLevel? CognitiveLevel { get; set; }
}
