namespace psms.Tenancy.Branding.Dto;

/// <summary>
/// A school a prospective parent can choose to apply to.
/// <para>
/// Enough to recognise it and nothing more: the name as the school writes it,
/// its logo, and the tenancy name the rest of the sign-in flow needs. No
/// contact details, no counts, nothing about who is already there.
/// </para>
/// </summary>
public class OpenSchoolDto
{
    /// <summary>What the rest of the sign-in flow identifies the school by.</summary>
    public string TenancyName { get; set; }

    /// <summary>The name as the school writes it, for a person to read.</summary>
    public string SchoolName { get; set; }

    public string LogoUrl { get; set; }
}
