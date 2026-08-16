namespace psms.Domain.Tenancy
{
    /// <summary>
    /// The stock PSMS branding, used whenever a tenant has not configured its
    /// own (issue #56: "Fallback to default PSMS branding if not configured").
    ///
    /// These values mirror the hard-coded frontend palette
    /// (<c>utils/theme-config.ts</c> colorPrimary and the LayoutShell header
    /// accent) so an unbranded tenant looks exactly as it did before this
    /// feature shipped.
    /// </summary>
    public static class BrandingDefaults
    {
        /// <summary>Action colour — buttons, links, selected states.</summary>
        public const string PrimaryColor = "#0066CC";

        /// <summary>Chrome colour — app header background and sidebar accent.</summary>
        public const string SecondaryColor = "#003D73";

        /// <summary>Shown when the tenant has set no display name of its own.</summary>
        public const string SchoolName = "Private School Management System";
    }
}
