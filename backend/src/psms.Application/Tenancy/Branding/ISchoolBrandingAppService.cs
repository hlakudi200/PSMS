using System.Threading.Tasks;
using Abp.Application.Services;
using psms.Domain.Shared.Storage;
using psms.Tenancy.Branding.Dto;

namespace psms.Tenancy.Branding;

/// <summary>
/// Per-tenant branding — colours, logo, favicon and display name (issue #56).
/// </summary>
public interface ISchoolBrandingAppService : IApplicationService
{
    /// <summary>
    /// The current tenant's branding, or the PSMS defaults if it has none.
    /// Never returns null.
    /// </summary>
    Task<SchoolBrandingDto> GetAsync();

    /// <summary>
    /// Branding for a tenant identified by tenancy name, for unauthenticated
    /// callers (the login page). Returns the PSMS defaults for an unknown or
    /// inactive tenancy name.
    /// </summary>
    Task<PublicSchoolBrandingDto> GetPublicAsync(string tenancyName);

    /// <summary>Sets colours and display name, creating the row if needed.</summary>
    Task<SchoolBrandingDto> UpdateAsync(UpdateSchoolBrandingDto input);

    /// <summary>
    /// Step 1 of an image upload: mints a one-time signed URL the client PUTs
    /// the bytes to, so they never pass through this server.
    /// </summary>
    Task<FileUploadTicket> RequestAssetUploadUrlAsync(RequestBrandingUploadUrlDto input);

    /// <summary>
    /// Step 2: records an already-uploaded image after verifying it server-side
    /// against storage. Replaces and cleans up any previous image.
    /// </summary>
    Task<SchoolBrandingDto> SetAssetAsync(SetBrandingAssetDto input);

    /// <summary>Clears an image, reverting to the text/stock fallback.</summary>
    Task<SchoolBrandingDto> RemoveAssetAsync(RemoveBrandingAssetDto input);
}
