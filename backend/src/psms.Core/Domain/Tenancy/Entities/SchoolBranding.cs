using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace psms.Domain.Tenancy.Entities
{
    /// <summary>
    /// Per-tenant visual identity — colours, logo and display name (issue #56).
    /// Exactly one row per tenant; absence of a row means "use
    /// <see cref="BrandingDefaults"/>".
    ///
    /// Deliberately <see cref="IMustHaveTenant"/> rather than the
    /// <c>IMayHaveTenant</c> used by most PSMS entities: ABP disables the
    /// MustHaveTenant filter when the ambient tenant is null, so the host and
    /// the anonymous login-page lookup can read across tenants without any
    /// manual <c>DisableFilter</c> call. Branding is never host-owned, so the
    /// nullable variant would buy nothing and cost a filter bypass on the one
    /// endpoint that is reachable without authentication.
    /// </summary>
    [Table("SchoolBrandings")]
    public class SchoolBranding : FullAuditedEntity<Guid>, IMustHaveTenant, ISoftDelete
    {
        public const int MaxColorLength = 7;      // "#RRGGBB"
        public const int MaxUrlLength = 500;
        public const int MaxSchoolNameLength = 200;

        /// <summary>Matches a 6-digit hex colour. Shorthand (#ABC) is rejected
        /// so the value can be handed to CSS and Ant Design unmodified.</summary>
        private static readonly Regex HexColor =
            new Regex("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>The tenant this branding belongs to.</summary>
        public int TenantId { get; set; }

        /// <summary>Action colour — maps to Ant Design's <c>colorPrimary</c>.</summary>
        [Required]
        [StringLength(MaxColorLength)]
        public string PrimaryColor { get; set; }

        /// <summary>Chrome colour — app header background and sidebar accent.</summary>
        [Required]
        [StringLength(MaxColorLength)]
        public string SecondaryColor { get; set; }

        /// <summary>
        /// Public URL of the school logo in object storage. Null until a logo
        /// is uploaded, in which case the UI falls back to the school name.
        /// </summary>
        [StringLength(MaxUrlLength)]
        public string LogoUrl { get; set; }

        /// <summary>
        /// Storage object key backing <see cref="LogoUrl"/>. Kept so a replaced
        /// logo can be deleted from storage instead of being orphaned.
        /// </summary>
        [StringLength(MaxUrlLength)]
        public string LogoObjectKey { get; set; }

        /// <summary>Public URL of the browser-tab favicon. Optional.</summary>
        [StringLength(MaxUrlLength)]
        public string FaviconUrl { get; set; }

        /// <summary>Storage object key backing <see cref="FaviconUrl"/>.</summary>
        [StringLength(MaxUrlLength)]
        public string FaviconObjectKey { get; set; }

        /// <summary>
        /// Display name shown in the sidebar, header and login page. Falls back
        /// to <see cref="BrandingDefaults.SchoolName"/> when blank.
        /// </summary>
        [StringLength(MaxSchoolNameLength)]
        public string SchoolName { get; set; }

        public bool IsDeleted { get; set; }

        /// <summary>Protected constructor for EF Core.</summary>
        protected SchoolBranding()
        {
        }

        /// <summary>
        /// Creates branding for a tenant, seeded with the PSMS defaults.
        /// </summary>
        public SchoolBranding(Guid id, int tenantId) : this()
        {
            Id = id;
            TenantId = tenantId;
            PrimaryColor = BrandingDefaults.PrimaryColor;
            SecondaryColor = BrandingDefaults.SecondaryColor;
            IsDeleted = false;
        }

        /// <summary>True when <paramref name="value"/> is a "#RRGGBB" colour.</summary>
        public static bool IsValidHexColor(string value)
            => !string.IsNullOrWhiteSpace(value) && HexColor.IsMatch(value);

        /// <summary>
        /// Applies the editable branding fields. Colours are normalised to
        /// upper-case so equality checks and CSS output stay stable; an invalid
        /// colour is rejected here as well as at the DTO boundary, so the
        /// invariant holds no matter which caller writes the entity.
        /// </summary>
        public void SetAppearance(string primaryColor, string secondaryColor, string schoolName)
        {
            if (!IsValidHexColor(primaryColor))
                throw new ArgumentException("Primary colour must be a #RRGGBB hex value.", nameof(primaryColor));
            if (!IsValidHexColor(secondaryColor))
                throw new ArgumentException("Secondary colour must be a #RRGGBB hex value.", nameof(secondaryColor));

            PrimaryColor = primaryColor.ToUpperInvariant();
            SecondaryColor = secondaryColor.ToUpperInvariant();
            SchoolName = string.IsNullOrWhiteSpace(schoolName) ? null : schoolName.Trim();
        }

        /// <summary>Points the logo at a newly uploaded object.</summary>
        public void SetLogo(string logoUrl, string objectKey)
        {
            LogoUrl = logoUrl;
            LogoObjectKey = objectKey;
        }

        /// <summary>Clears the logo, reverting the UI to the school-name text.</summary>
        public void ClearLogo()
        {
            LogoUrl = null;
            LogoObjectKey = null;
        }

        /// <summary>Points the favicon at a newly uploaded object.</summary>
        public void SetFavicon(string faviconUrl, string objectKey)
        {
            FaviconUrl = faviconUrl;
            FaviconObjectKey = objectKey;
        }

        /// <summary>Clears the favicon.</summary>
        public void ClearFavicon()
        {
            FaviconUrl = null;
            FaviconObjectKey = null;
        }
    }
}
