using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Financial.StudentFees.Dto;

/// <summary>
/// Input DTO for creating a student fee.
/// </summary>
public class CreateStudentFeeDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid FeeStructureId { get; set; }

    [Required]
    [Range(0.01, 9999999.99)]
    public decimal AmountDue { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [StringLength(2000)]
    public string Notes { get; set; }
}
