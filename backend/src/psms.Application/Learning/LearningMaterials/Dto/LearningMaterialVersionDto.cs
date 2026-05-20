using Abp.Application.Services.Dto;
using System;

namespace psms.Learning.LearningMaterials.Dto;

/// <summary>
/// One row of a learning-material's version history.
/// </summary>
public class LearningMaterialVersionDto : EntityDto<Guid>
{
    public Guid LearningMaterialId { get; set; }
    public int VersionNumber { get; set; }
    public string ChangeDescription { get; set; }
    public string FileName { get; set; }
    public string FileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string ContentType { get; set; }
    public long UploadedByUserId { get; set; }
    public DateTime CreationTime { get; set; }
}
