using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Academic.StudentTransfers.Dto;

/// <summary>
/// DTO for updating a student transfer request.
/// All fields optional (null-skip pattern).
/// </summary>
public class UpdateStudentTransferDto
{
    public int? TransferType { get; set; }

    [StringLength(1000)]
    public string Reason { get; set; }

    [StringLength(200)]
    public string FromSchoolName { get; set; }

    [StringLength(200)]
    public string ToSchoolName { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public Guid? TransferGradeId { get; set; }

    [StringLength(2048)]
    public string PreviousReportUrl { get; set; }
}
