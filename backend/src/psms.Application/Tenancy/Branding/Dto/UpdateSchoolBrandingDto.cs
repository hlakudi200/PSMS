using System.ComponentModel.DataAnnotations;
using psms.Domain.Tenancy.Entities;

namespace psms.Tenancy.Branding.Dto;

/// <summary>
/// Colours and display name. Logo/favicon are managed separately (they need an
/// upload round-trip), so this DTO never touches them — posting it will not
/// clear an existing logo.
/// </summary>
public class UpdateSchoolBrandingDto
{
    [Required]
    [StringLength(SchoolBranding.MaxColorLength)]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Primary colour must be a #RRGGBB hex value.")]
    public string PrimaryColor { get; set; }

    [Required]
    [StringLength(SchoolBranding.MaxColorLength)]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Secondary colour must be a #RRGGBB hex value.")]
    public string SecondaryColor { get; set; }

    /// <summary>Blank falls back to the default PSMS name.</summary>
    [StringLength(SchoolBranding.MaxSchoolNameLength)]
    public string SchoolName { get; set; }
}
