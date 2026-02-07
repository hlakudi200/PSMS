using psms.Domain.Shared.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Financial.FeeStructures.Dto;

/// <summary>
/// Input DTO for creating a fee structure.
/// </summary>
public class CreateFeeStructureDto
{
    [Required]
    public Guid GradeId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public SouthAfricanFeeType FeeType { get; set; }

    [Required]
    [StringLength(200)]
    public string FeeName { get; set; }

    [Required]
    [Range(0.01, 9999999.99)]
    public decimal Amount { get; set; }

    [StringLength(10)]
    public string Currency { get; set; }

    [StringLength(50)]
    public string BillingFrequency { get; set; }

    [Range(1, 31)]
    public int DueDay { get; set; } = 1;
}
