using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Assessment.Assessments.Dto;

/// <summary>
/// Full DTO for an assessment.
/// </summary>
public class AssessmentDto : FullAuditedEntityDto<Guid>
{
    public Guid ClassSubjectId { get; set; }
    public Guid TermId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public AcademicAssessmentType AssessmentType { get; set; }
    public CapsAssessmentCategory? CapsCategory { get; set; }
    public decimal MaxMarks { get; set; }
    public decimal Weight { get; set; }
    public decimal PassPercentage { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int? DurationMinutes { get; set; }
    public string Instructions { get; set; }
    public bool IsPublished { get; set; }
    public bool MarksReleased { get; set; }
    public long CreatedByTeacherUserId { get; set; }

    // Flattened from ClassSubject
    public string ClassName { get; set; }
    public string SubjectName { get; set; }

    // Flattened from Term
    public string TermName { get; set; }

    // Computed
    public int MarkCount { get; set; }
    public int QuestionCount { get; set; }
}
