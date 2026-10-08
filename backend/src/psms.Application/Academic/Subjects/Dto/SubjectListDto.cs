using psms.Domain.Shared.Enums;
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

    /// <summary>
    /// RC-20. The level a language is offered at — National Protocol §17(6).
    /// Null for a subject that is not a language. Where it is null on a language
    /// subject, the level is read from the subject's name instead.
    /// </summary>
    public LanguageLevel? LanguageLevel { get; set; }
    public bool IsActive { get; set; }
    public int GradeCount { get; set; }
}
