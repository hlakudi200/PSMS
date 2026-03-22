using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Financial.ExpenseRequests.Dto;

/// <summary>
/// DTO for updating an expense request.
/// All fields optional (null-skip pattern).
/// </summary>
public class UpdateExpenseRequestDto
{
    public int? Category { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    public decimal? Amount { get; set; }

    public int? Priority { get; set; }

    [StringLength(200)]
    public string Department { get; set; }

    [StringLength(200)]
    public string Vendor { get; set; }

    [StringLength(2048)]
    public string QuotationUrl { get; set; }

    public DateTime? RequiredByDate { get; set; }
}
