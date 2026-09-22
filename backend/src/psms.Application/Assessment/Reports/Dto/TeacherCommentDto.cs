using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// RC-10. Input for the class teacher's comment on a report card.
/// <para>
/// The three comment endpoints used to share one DTO with [StringLength(2000)],
/// while the principal's and the parent's columns hold 1000. A 1500-character
/// principal comment passed validation and then failed at the database. Each
/// endpoint now carries the length its column actually has.
/// </para>
/// </summary>
public class TeacherCommentDto
{
    [Required]
    [StringLength(psms.Domain.Assessment.Entities.Report.MaxTeacherCommentLength)]
    public string Comment { get; set; }
}
