using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Learning.LearningMaterials.Dto;

/// <summary>
/// Lightweight DTO for learning material lists.
/// </summary>
public class LearningMaterialListDto : EntityDto<Guid>
{
    public Guid ClassSubjectId { get; set; }
    public string Title { get; set; }
    public LearningMaterialType MaterialType { get; set; }
    public bool IsPublished { get; set; }
    public int DisplayOrder { get; set; }
    public int ViewCount { get; set; }
    public string FileName { get; set; }
}
