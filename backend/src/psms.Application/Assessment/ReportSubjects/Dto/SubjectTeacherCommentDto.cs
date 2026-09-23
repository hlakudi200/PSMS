using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.ReportSubjects.Dto;

/// <summary>
/// RC-10 / RC-08. Input for the subject teacher's comment on one subject of a
/// report card.
/// <para>
/// A DTO rather than a bare string parameter: ABP binds a primitive from the
/// query string, and the frontend has always posted <c>{ comment }</c> in the
/// body — so the comment arrived null and the endpoint blanked the field it was
/// supposed to set.
/// </para>
/// </summary>
public class SubjectTeacherCommentDto
{
    [StringLength(psms.Domain.Assessment.Entities.ReportSubject.MaxCommentLength)]
    public string Comment { get; set; }
}
