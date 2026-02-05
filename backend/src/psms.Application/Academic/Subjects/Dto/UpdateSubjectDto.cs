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

    public bool? IsActive { get; set; }
}
