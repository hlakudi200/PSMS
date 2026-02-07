using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Financial.StudentFees.Dto;

/// <summary>
/// Input DTO for updating a student fee. All fields nullable (null-skip).
/// </summary>
public class UpdateStudentFeeDto
{
    [Range(0.01, 9999999.99)]
    public decimal? AmountDue { get; set; }

    public DateTime? DueDate { get; set; }

    [StringLength(2000)]
    public string Notes { get; set; }
}
