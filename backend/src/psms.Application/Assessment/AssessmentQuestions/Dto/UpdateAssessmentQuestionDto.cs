using psms.Domain.Shared.Enums;

namespace psms.Assessment.AssessmentQuestions.Dto;

/// <summary>
/// Input DTO for updating an assessment question. All fields nullable for partial updates.
/// </summary>
public class UpdateAssessmentQuestionDto
{
    public int? QuestionNumber { get; set; }
    public QuestionType? QuestionType { get; set; }
    public string QuestionText { get; set; }
    public decimal? Marks { get; set; }
    public string Options { get; set; }
    public string CorrectAnswer { get; set; }
    public string Explanation { get; set; }
    public CognitiveLevel? CognitiveLevel { get; set; }
    public bool? IsActive { get; set; }
}
