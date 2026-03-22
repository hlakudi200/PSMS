using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.StudentTransfers.Dto;

/// <summary>
/// Lightweight list DTO for student transfer requests.
/// </summary>
public class StudentTransferListDto : EntityDto<Guid>
{
    public string TransferNumber { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public int TransferType { get; set; }
    public int Status { get; set; }
    public DateTime RequestedDate { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string FromSchoolName { get; set; }
    public string ToSchoolName { get; set; }
    public DateTime CreationTime { get; set; }
}
