using Abp.Application.Services.Dto;
using System;

namespace psms.Academic.StudentTransfers.Dto;

/// <summary>
/// Full DTO for a student transfer request.
/// </summary>
public class StudentTransferDto : FullAuditedEntityDto<Guid>
{
    public string TransferNumber { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public Guid AcademicYearId { get; set; }
    public string AcademicYearName { get; set; }
    public int TransferType { get; set; }
    public int Status { get; set; }
    public string Reason { get; set; }
    public string FromSchoolName { get; set; }
    public string ToSchoolName { get; set; }
    public DateTime RequestedDate { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public Guid? TransferGradeId { get; set; }
    public string TransferGradeName { get; set; }
    public string PreviousReportUrl { get; set; }
    public string TransferCertificateUrl { get; set; }
    public long? ApprovedByUserId { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string Notes { get; set; }
}
