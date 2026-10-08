using psms.Domain.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Subjects.Dto;

/// <summary>
/// Input DTO for updating a subject. All fields nullable (partial update).
/// </summary>
public class UpdateSubjectDto
{
    [StringLength(100, MinimumLength = 2)]
    public string SubjectName { get; set; }

    [StringLength(20, MinimumLength = 2)]
    public string SubjectCode { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    public bool? IsCore { get; set; }

    /// <summary>
    /// RC-20. The level a language is offered at — National Protocol §17(6).
    /// Null for a subject that is not a language. Where it is null on a language
    /// subject, the level is read from the subject's name instead.
    /// </summary>
    public LanguageLevel? LanguageLevel { get; set; }

    /// <summary>
    /// RC-20. Clears the language level. A patch DTO uses null to mean "not
    /// supplied", so a subject that stops being a language needs an explicit way
    /// to say so.
    /// </summary>
    public bool ClearLanguageLevel { get; set; }

    public bool? IsActive { get; set; }
}
