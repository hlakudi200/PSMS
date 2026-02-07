using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Assessment.Assessments.Dto;

/// <summary>
/// Lightweight DTO for assessment lists.
/// </summary>
public class AssessmentListDto : EntityDto<Guid>
{
    public Guid ClassSubjectId { get; set; }
    public Guid TermId { get; set; }
    public string Name { get; set; }
    public AssessmentType AssessmentType { get; set; }
    public CapsAssessmentCategory? CapsCategory { get; set; }
    public decimal MaxMarks { get; set; }
    public decimal Weight { get; set; }
    public decimal PassPercentage { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsPublished { get; set; }
    public bool MarksReleased { get; set; }

    // Flattened
    public string ClassName { get; set; }
    public string SubjectName { get; set; }
    public string TermName { get; set; }

    // Computed
    public int MarkCount { get; set; }
    public int QuestionCount { get; set; }
}
