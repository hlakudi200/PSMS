namespace psms.Tenancy.Branding.Dto;

/// <summary>
/// The subset of branding served to unauthenticated callers so the login page
/// can theme itself before a session exists (issue #56, AC 6).
///
/// Deliberately a separate, narrower DTO from <see cref="SchoolBrandingDto"/>:
/// this one is reachable anonymously, so it must never grow audit fields, ids,
/// or anything else about the tenant. Add new fields here only if they are
/// safe to hand to the open internet.
/// </summary>
public class PublicSchoolBrandingDto
{
    public string PrimaryColor { get; set; }

    public string SecondaryColor { get; set; }

    public string LogoUrl { get; set; }

    public string FaviconUrl { get; set; }

    public string SchoolName { get; set; }
}
