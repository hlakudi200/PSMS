using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.StudentTransfers.Dto;

/// <summary>
/// Input DTO for querying student transfer requests.
/// </summary>
public class GetStudentTransfersInput : PagedAndSortedResultRequestDto
{
    public Guid? StudentId { get; set; }
    public Guid? AcademicYearId { get; set; }
    public int? TransferType { get; set; }
    public int? Status { get; set; }
    public string Search { get; set; }
}
