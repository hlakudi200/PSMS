using System;

namespace psms.Tenancy.Branding.Dto;

/// <summary>
/// A tenant's full branding as seen by an authenticated user. When the tenant
/// has never configured branding the service returns this populated from
/// <see cref="psms.Domain.Tenancy.BrandingDefaults"/> with
/// <see cref="IsConfigured"/> false — callers never have to handle a null.
/// </summary>
public class SchoolBrandingDto
{
    /// <summary>Null when the tenant has no branding row yet.</summary>
    public Guid? Id { get; set; }

    public int? TenantId { get; set; }

    /// <summary>Action colour — Ant Design <c>colorPrimary</c>. "#RRGGBB".</summary>
    public string PrimaryColor { get; set; }

    /// <summary>Chrome colour — header background and sidebar accent. "#RRGGBB".</summary>
    public string SecondaryColor { get; set; }

    /// <summary>Public logo URL, or null to fall back to the school name.</summary>
    public string LogoUrl { get; set; }

    /// <summary>Public favicon URL, or null for the stock icon.</summary>
    public string FaviconUrl { get; set; }

    /// <summary>
    /// Display name for the sidebar, header and login page — always populated,
    /// falling back to the default when the tenant has set none.
    /// </summary>
    public string SchoolName { get; set; }

    /// <summary>
    /// The name the tenant actually stored, or null if it has set none.
    /// Distinct from <see cref="SchoolName"/> so an editor can tell "unset"
    /// apart from "deliberately named the same as the default" — binding a
    /// form to the resolved value would silently persist the fallback as an
    /// explicit choice on the next save.
    /// </summary>
    public string ConfiguredSchoolName { get; set; }

    /// <summary>
    /// False when these values are the PSMS defaults rather than a stored row.
    /// Lets the settings page show "not yet configured" without a second call.
    /// </summary>
    public bool IsConfigured { get; set; }
}
