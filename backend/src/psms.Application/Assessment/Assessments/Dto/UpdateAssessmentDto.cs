using psms.Domain.Shared.Enums;
using System;

namespace psms.Assessment.Assessments.Dto;

/// <summary>
/// Input DTO for updating an assessment. All fields nullable for partial updates.
/// Cannot change ClassSubjectId, TermId, or AssessmentType.
/// </summary>
public class UpdateAssessmentDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public CapsAssessmentCategory? CapsCategory { get; set; }
    public decimal? MaxMarks { get; set; }
    public decimal? Weight { get; set; }
    public decimal? PassPercentage { get; set; }
    public DateTime? ScheduledDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int? DurationMinutes { get; set; }
    public string Instructions { get; set; }
}
