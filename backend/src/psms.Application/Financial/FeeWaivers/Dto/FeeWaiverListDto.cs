using Abp.Application.Services.Dto;
using System;

namespace psms.Financial.FeeWaivers.Dto;

/// <summary>
/// Lightweight list DTO for fee waivers.
/// </summary>
public class FeeWaiverListDto : EntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public int WaiverType { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public int Status { get; set; }
    public DateTime CreationTime { get; set; }
}
