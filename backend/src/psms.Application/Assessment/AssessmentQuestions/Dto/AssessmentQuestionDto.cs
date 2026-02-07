using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Assessment.AssessmentQuestions.Dto;

/// <summary>
/// Full DTO for an assessment question.
/// </summary>
public class AssessmentQuestionDto : EntityDto<Guid>
{
    public Guid AssessmentId { get; set; }
    public int QuestionNumber { get; set; }
    public QuestionType QuestionType { get; set; }
    public string QuestionText { get; set; }
    public decimal Marks { get; set; }
    public string Options { get; set; }
    public string CorrectAnswer { get; set; }
    public string Explanation { get; set; }
    public CognitiveLevel? CognitiveLevel { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreationTime { get; set; }
    public long? CreatorUserId { get; set; }
}
