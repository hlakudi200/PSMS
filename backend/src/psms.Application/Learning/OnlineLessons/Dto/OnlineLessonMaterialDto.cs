using psms.Domain.Shared.Enums;
using System;

namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// A learning material attached to an online lesson as pre-lesson reading.
/// Id is the learning material's id, so clients open it through the existing
/// LearningMaterial endpoints.
/// </summary>
public class OnlineLessonMaterialDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public LearningMaterialType MaterialType { get; set; }
    public string FileName { get; set; }
    public string ExternalLink { get; set; }
}
