using System.ComponentModel.DataAnnotations;

namespace psms.Financial.FeeWaivers.Dto;

/// <summary>
/// Input DTO for updating a fee waiver (null-skip pattern).
/// </summary>
public class UpdateFeeWaiverDto
{
    [StringLength(1000)]
    public string Reason { get; set; }

    public decimal? RequestedAmount { get; set; }

    [StringLength(2048)]
    public string SupportingDocumentUrl { get; set; }
}
