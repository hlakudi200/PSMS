using System;
using System.ComponentModel.DataAnnotations;

namespace psms.Financial.FeeWaivers.Dto;

/// <summary>
/// Input DTO for creating a fee waiver.
/// </summary>
public class CreateFeeWaiverDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid AcademicYearId { get; set; }

    [Required]
    public int WaiverType { get; set; }

    [Required]
    public decimal RequestedAmount { get; set; }

    [Required]
    [StringLength(1000)]
    public string Reason { get; set; }

    public Guid? StudentFeeId { get; set; }

    [StringLength(2048)]
    public string SupportingDocumentUrl { get; set; }
}
