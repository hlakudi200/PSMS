using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;

namespace psms.Financial.StudentFees.Dto;

/// <summary>
/// Full DTO for a student fee.
/// </summary>
public class StudentFeeDto : FullAuditedEntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public Guid FeeStructureId { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime DueDate { get; set; }
    public FeeStatus Status { get; set; }
    public string Notes { get; set; }
    public string ConcurrencyStamp { get; set; }

    // Computed
    public decimal OutstandingBalance { get; set; }
    public string FormattedAmountDue { get; set; }
    public int AllocationCount { get; set; }

    // Flattened from Student
    public string StudentName { get; set; }
    public string StudentAdmissionNumber { get; set; }

    // Flattened from FeeStructure
    public string FeeStructureName { get; set; }
    public SouthAfricanFeeType FeeType { get; set; }
}
