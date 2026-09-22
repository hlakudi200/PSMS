using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Assessment.Assessments.Dto;

/// <summary>
/// Input DTO for querying assessments with optional filters.
/// </summary>
public class GetAssessmentsInput : PagedAndSortedResultRequestDto
{
    public Guid? ClassSubjectId { get; set; }
    public Guid? TermId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SubjectId { get; set; }
    public AcademicAssessmentType? AssessmentType { get; set; }
    public bool? IsPublished { get; set; }
    public string Name { get; set; }
    public string Keyword { get; set; }
}
