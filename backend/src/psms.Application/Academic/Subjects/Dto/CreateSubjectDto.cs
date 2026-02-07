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
}
