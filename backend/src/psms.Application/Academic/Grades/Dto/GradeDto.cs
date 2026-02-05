using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Academic.Grades.Dto;

/// <summary>
/// Full DTO for Grade entity including computed properties.
/// </summary>
public class GradeDto : FullAuditedEntityDto<Guid>
{
    public SouthAfricanGradeLevel GradeLevel { get; set; }
    public string GradeName { get; set; }
    public SouthAfricanSchoolPhase SchoolPhase { get; set; }
    public string SchoolPhaseDisplayName => SchoolPhase.ToString();
    public string Description { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Total number of classes in this grade</summary>
    public int ClassCount { get; set; }

    /// <summary>Total number of active students in this grade</summary>
    public int StudentCount { get; set; }

    /// <summary>Total number of subjects assigned to this grade</summary>
    public int SubjectCount { get; set; }
}
