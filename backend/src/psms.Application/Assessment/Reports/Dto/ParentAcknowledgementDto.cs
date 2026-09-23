using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// RC-10. Input for a parent acknowledging a report card. The comment is
/// optional — acknowledging is the point, and a parent need not write anything.
/// </summary>
public class ParentAcknowledgementDto
{
    [StringLength(psms.Domain.Assessment.Entities.Report.MaxParentCommentLength)]
    public string Comment { get; set; }
}
