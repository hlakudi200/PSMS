using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Financial.FeeStructures.Dto;

/// <summary>
/// Lightweight DTO for fee structure lists.
/// </summary>
public class FeeStructureListDto : EntityDto<Guid>
{
    public Guid GradeId { get; set; }
    public Guid AcademicYearId { get; set; }
    public SouthAfricanFeeType FeeType { get; set; }
    public string FeeName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string BillingFrequency { get; set; }
    public int DueDay { get; set; }
    public bool IsActive { get; set; }

    // Flattened
    public string GradeName { get; set; }
    public string AcademicYearName { get; set; }

    // Computed
    public string FormattedAmount { get; set; }
    public int StudentFeeCount { get; set; }
}
