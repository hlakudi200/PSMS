using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Assessment.Marks.Dto;

/// <summary>
/// Input DTO for querying marks with optional filters.
/// </summary>
public class GetMarksInput : PagedAndSortedResultRequestDto
{
    public Guid? AssessmentId { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SubjectId { get; set; }
    public Guid? TermId { get; set; }
    public MarkStatus? Status { get; set; }
    public string StudentName { get; set; }
}
