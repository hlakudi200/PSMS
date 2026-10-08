using psms.Domain.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Subjects.Dto;

/// <summary>
/// Input DTO for creating a new subject.
/// </summary>
public class CreateSubjectDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string SubjectName { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 2)]
    public string SubjectCode { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    /// <summary>Whether this is a core (required) subject</summary>
    public bool IsCore { get; set; }

    /// <summary>
    /// RC-20. The level a language is offered at — National Protocol §17(6).
    /// Null for a subject that is not a language. Where it is null on a language
    /// subject, the level is read from the subject's name instead.
    /// </summary>
    public LanguageLevel? LanguageLevel { get; set; }
}
