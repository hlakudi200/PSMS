using System.ComponentModel.DataAnnotations;

namespace psms.Assessment.Reports.Dto;

/// <summary>
/// Input DTO for adding a comment to a report.
/// </summary>
public class ReportCommentDto
{
    [Required]
    [StringLength(2000)]
    public string Comment { get; set; }
}
