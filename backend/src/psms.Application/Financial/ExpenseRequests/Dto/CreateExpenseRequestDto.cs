using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Financial.ExpenseRequests.Dto;

/// <summary>
/// DTO for creating an expense request.
/// </summary>
public class CreateExpenseRequestDto
{
    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public int Category { get; set; }

    [Required]
    [StringLength(1000)]
    public string Description { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public int Priority { get; set; }

    [StringLength(200)]
    public string Department { get; set; }

    [StringLength(200)]
    public string Vendor { get; set; }

    [StringLength(2048)]
    public string QuotationUrl { get; set; }

    public DateTime? RequiredByDate { get; set; }
}
