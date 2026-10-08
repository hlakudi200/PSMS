namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// The uploadable image slots on a tenant's branding (issue #56).
    /// </summary>
    public enum BrandingAssetType
    {
        /// <summary>School logo shown in the sidebar and on the login page.</summary>
        Logo = 1,

        /// <summary>Browser-tab icon.</summary>
        Favicon = 2,

        /// <summary>
        /// The school's stamp, printed on a report card beside the signatures.
        /// National Protocol §25(8)(b) names it among the essential components
        /// of a report card. It is distinct from the logo: the logo is the
        /// school's mark on a screen, the stamp is what authenticates a paper
        /// document.
        /// </summary>
        Stamp = 3
    }
}
