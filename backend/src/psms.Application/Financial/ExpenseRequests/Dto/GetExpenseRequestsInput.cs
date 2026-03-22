using Abp.Application.Services.Dto;
using System;

namespace psms.Financial.ExpenseRequests.Dto;

/// <summary>
/// Input DTO for querying expense requests.
/// </summary>
public class GetExpenseRequestsInput : PagedAndSortedResultRequestDto
{
    public Guid? AcademicYearId { get; set; }
    public int? Category { get; set; }
    public int? Status { get; set; }
    public int? Priority { get; set; }
    public long? RequestedByUserId { get; set; }
    public string Search { get; set; }
}
