using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Academic.Grades.Dto;

/// <summary>
/// Lightweight DTO for grade lists and dropdowns.
/// </summary>
public class GradeListDto : EntityDto<Guid>
{
    public SouthAfricanGradeLevel GradeLevel { get; set; }
    public string GradeName { get; set; }
    public SouthAfricanSchoolPhase SchoolPhase { get; set; }
    public bool IsActive { get; set; }
    public int ClassCount { get; set; }
    public int StudentCount { get; set; }
}
