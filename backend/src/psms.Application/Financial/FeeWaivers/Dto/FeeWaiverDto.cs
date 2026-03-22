using Abp.Application.Services.Dto;
using System;

namespace psms.Financial.FeeWaivers.Dto;

/// <summary>
/// Full DTO for a fee waiver.
/// </summary>
public class FeeWaiverDto : FullAuditedEntityDto<Guid>
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public Guid? StudentFeeId { get; set; }
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }
    public int WaiverType { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string Reason { get; set; }
    public int Status { get; set; }
    public string SupportingDocumentUrl { get; set; }
    public long? ReviewedByUserId { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string ReviewNotes { get; set; }
}
