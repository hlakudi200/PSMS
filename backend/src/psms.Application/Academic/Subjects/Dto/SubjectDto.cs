using psms.Domain.Shared.Enums;
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

    /// <summary>
    /// RC-20. The level a language is offered at — National Protocol §17(6).
    /// Null for a subject that is not a language. Where it is null on a language
    /// subject, the level is read from the subject's name instead.
    /// </summary>
    public LanguageLevel? LanguageLevel { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Number of grades this subject is assigned to</summary>
    public int GradeCount { get; set; }

    /// <summary>Number of teachers teaching this subject</summary>
    public int TeacherCount { get; set; }
}
