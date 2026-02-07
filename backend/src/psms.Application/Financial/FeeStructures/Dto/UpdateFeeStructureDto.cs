using psms.Domain.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace psms.Financial.FeeStructures.Dto;

/// <summary>
/// Input DTO for updating a fee structure. All fields nullable (null-skip).
/// IsActive is NOT included — use Activate/Deactivate endpoints (requires Approve permission).
/// </summary>
public class UpdateFeeStructureDto
{
    [StringLength(200)]
    public string FeeName { get; set; }

    [Range(0.01, 9999999.99)]
    public decimal? Amount { get; set; }

    public SouthAfricanFeeType? FeeType { get; set; }

    [StringLength(10)]
    public string Currency { get; set; }

    [StringLength(50)]
    public string BillingFrequency { get; set; }

    [Range(1, 31)]
    public int? DueDay { get; set; }
}
