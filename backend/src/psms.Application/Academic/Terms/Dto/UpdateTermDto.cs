using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.Terms.Dto;

/// <summary>
/// Input DTO for updating a term. All fields nullable (partial update).
/// </summary>
public class UpdateTermDto
{
    [StringLength(50, MinimumLength = 2)]
    public string TermName { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
