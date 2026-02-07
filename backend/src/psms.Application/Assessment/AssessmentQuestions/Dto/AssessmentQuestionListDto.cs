using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Assessment.AssessmentQuestions.Dto;

/// <summary>
/// Lightweight DTO for assessment question lists.
/// </summary>
public class AssessmentQuestionListDto : EntityDto<Guid>
{
    public Guid AssessmentId { get; set; }
    public int QuestionNumber { get; set; }
    public QuestionType QuestionType { get; set; }
    public string QuestionText { get; set; }
    public decimal Marks { get; set; }
    public CognitiveLevel? CognitiveLevel { get; set; }
    public bool IsActive { get; set; }
}
