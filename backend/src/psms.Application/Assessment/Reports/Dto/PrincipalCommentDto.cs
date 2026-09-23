using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// RC-10. Input for the principal's comment on a report card, at the length the
/// column actually holds.
/// </summary>
public class PrincipalCommentDto
{
    [Required]
    [StringLength(psms.Domain.Assessment.Entities.Report.MaxPrincipalCommentLength)]
    public string Comment { get; set; }
}
