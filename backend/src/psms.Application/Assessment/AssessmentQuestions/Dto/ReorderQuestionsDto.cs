using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.AssessmentQuestions.Dto;

/// <summary>
/// Input DTO for reordering questions within an assessment.
/// </summary>
public class ReorderQuestionsDto
{
    [Required]
    public Guid AssessmentId { get; set; }

    [Required]
    public List<Guid> QuestionIdsInOrder { get; set; }
}
