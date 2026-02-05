using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.Subjects.Dto;

/// <summary>
/// Full DTO for Subject entity including computed properties.
/// </summary>
public class SubjectDto : FullAuditedEntityDto<Guid>
{
    public string SubjectName { get; set; }
    public string SubjectCode { get; set; }
    public string Description { get; set; }
    public bool IsCore { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Number of grades this subject is assigned to</summary>
    public int GradeCount { get; set; }

    /// <summary>Number of teachers teaching this subject</summary>
    public int TeacherCount { get; set; }
}
