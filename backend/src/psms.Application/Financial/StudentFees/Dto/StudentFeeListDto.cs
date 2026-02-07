using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Financial.StudentFees.Dto;

/// <summary>
/// Lightweight DTO for student fee lists.
/// </summary>
public class StudentFeeListDto : EntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid FeeStructureId { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime DueDate { get; set; }
    public FeeStatus Status { get; set; }

    // Computed
    public decimal OutstandingBalance { get; set; }

    // Flattened
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }
    public string FeeStructureName { get; set; }
    public SouthAfricanFeeType FeeType { get; set; }
}
