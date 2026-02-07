using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.Subjects.Dto;

/// <summary>
/// Lightweight DTO for subject lists and dropdowns.
/// </summary>
public class SubjectListDto : EntityDto<Guid>
{
    public string SubjectName { get; set; }
    public string SubjectCode { get; set; }
    public bool IsCore { get; set; }
    public bool IsActive { get; set; }
    public int GradeCount { get; set; }
}
