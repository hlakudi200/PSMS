using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Learning.LearningMaterials.Dto;

/// <summary>
/// Input DTO for querying learning materials with optional filters.
/// </summary>
public class GetLearningMaterialsInput : PagedAndSortedResultRequestDto
{
    public Guid? ClassSubjectId { get; set; }
    public Guid? TermId { get; set; }
    public LearningMaterialType? MaterialType { get; set; }
    public bool? IsPublished { get; set; }
}
