using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Financial.StudentFees.Dto;

/// <summary>
/// Input DTO for bulk-assigning a fee structure to multiple students.
/// </summary>
public class BulkCreateStudentFeesDto
{
    [Required]
    public Guid FeeStructureId { get; set; }

    [Required]
    [MinLength(1)]
    public List<Guid> StudentIds { get; set; }

    public DateTime? DueDateOverride { get; set; }
}
