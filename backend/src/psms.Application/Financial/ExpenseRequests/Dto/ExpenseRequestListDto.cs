using Abp.Application.Services.Dto;
using System;

namespace psms.Financial.ExpenseRequests.Dto;

/// <summary>
/// Lightweight list DTO for expense requests.
/// </summary>
public class ExpenseRequestListDto : EntityDto<Guid>
{
    public string RequestNumber { get; set; }
    public string RequestedByName { get; set; }
    public string Department { get; set; }
    public int Category { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public int Status { get; set; }
    public int Priority { get; set; }
    public DateTime? RequiredByDate { get; set; }
    public DateTime CreationTime { get; set; }
}
