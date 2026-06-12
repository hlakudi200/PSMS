using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Assessments.Dto;

/// <summary>
/// Creates an assessment together with all of its questions in a single
/// atomic operation (QA-001). Reuses the assessment-level fields from
/// <see cref="CreateAssessmentDto"/>; because the whole insert runs inside
/// one ABP unit of work, a failure on any question rolls back the
/// assessment too — the caller never ends up with an orphaned assessment.
/// </summary>
public class CreateAssessmentWithQuestionsDto : CreateAssessmentDto
{
    [Required]
    public List<CreateAssessmentQuestionInlineDto> Questions { get; set; } = new();
}
