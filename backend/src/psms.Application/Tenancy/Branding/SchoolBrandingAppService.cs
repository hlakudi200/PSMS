using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Shared.Enums;
using psms.Domain.Shared.Storage;
using psms.Domain.Tenancy;
using psms.Domain.Tenancy.Entities;
using psms.MultiTenancy;
using psms.Tenancy.Branding.Dto;
using psms.Tenancy.Shared;

namespace psms.Tenancy.Branding;

/// <summary>
/// Manages each tenant's visual identity (issue #56).
///
/// No class-level <c>[AbpAuthorize]</c> on purpose: <see cref="GetPublicAsync"/>
/// must be reachable before login. Every other method carries its own
/// attribute — add one to any method introduced here.
/// </summary>
public class SchoolBrandingAppService : ApplicationService, ISchoolBrandingAppService
{
    private readonly IRepository<SchoolBranding, Guid> _brandingRepository;
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IFileStorageService _fileStorage;

    /// <summary>Supabase bucket for branding images (public-read).</summary>
    private const string BrandingBucket = "branding";

    // AC 2 caps the logo at 2MB. A favicon has no business being anywhere near
    // that, so it gets a tighter ceiling of its own.
    private const long MaxLogoBytes = 2 * 1024 * 1024;
    private const long MaxFaviconBytes = 512 * 1024;

    private static readonly HashSet<string> LogoExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg" };

    private static readonly HashSet<string> FaviconExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".ico" };

    public SchoolBrandingAppService(
        IRepository<SchoolBranding, Guid> brandingRepository,
        IRepository<Tenant> tenantRepository,
        IFileStorageService fileStorage)
    {
        _brandingRepository = brandingRepository;
        _tenantRepository = tenantRepository;
        _fileStorage = fileStorage;
    }

    /* ==================== Reads ==================== */

    /// <summary>
    /// Any authenticated user may read their tenant's branding — every portal
    /// needs it to render. Editing is what requires a permission.
    /// </summary>
    [AbpAuthorize]
    public async Task<SchoolBrandingDto> GetAsync()
    {
        var tenantId = AbpSession.TenantId;
        if (tenantId == null)
        {
            // Host users have no branding of their own; give them the defaults
            // so the host console still renders.
            return BuildDefaultDto(null);
        }

        var branding = await _brandingRepository
            .GetAll()
            .FirstOrDefaultAsync(b => b.TenantId == tenantId.Value);

        return branding == null ? BuildDefaultDto(tenantId) : MapToDto(branding);
    }

    /// <summary>
    /// Anonymous, pre-login lookup by tenancy name (AC 6).
    ///
    /// Runs explicitly host-side via <c>SetTenantId(null)</c> so the result
    /// cannot be steered by an <c>Abp-TenantId</c> header the caller supplies,
    /// and so the MustHaveTenant filter is off for the branding read.
    ///
    /// An unknown or inactive tenancy name returns the PSMS defaults rather
    /// than a 404: the login page must still render, and a differing response
    /// would turn this into a tenant-enumeration oracle.
    /// </summary>
    [AbpAllowAnonymous]
    public async Task<PublicSchoolBrandingDto> GetPublicAsync(string tenancyName)
    {
        if (string.IsNullOrWhiteSpace(tenancyName))
            return BuildDefaultPublicDto();

        var normalised = tenancyName.Trim();

        // AbpTenants is host-owned, so the tenant itself is always resolved
        // host-side.
        int tenantId;
        using (CurrentUnitOfWork.SetTenantId(null))
        {
            var tenant = await _tenantRepository
                .GetAll()
                .FirstOrDefaultAsync(t => t.TenancyName == normalised && t.IsActive);

            if (tenant == null)
                return BuildDefaultPublicDto();

            tenantId = tenant.Id;
        }

        // Read the branding AS that tenant, not host-side. Tenants share the
        // host database today, so this is equivalent — but a tenant with its
        // own ConnectionString (still supported by CreateTenantDto) keeps its
        // branding in its own database, where a host-side read would never
        // find it and the login page would silently fall back to defaults.
        //
        // Still not steerable by an Abp-TenantId header: the id comes from the
        // tenancy name resolved above, never from the request.
        using (CurrentUnitOfWork.SetTenantId(tenantId))
        {
            var branding = await _brandingRepository
                .GetAll()
                .FirstOrDefaultAsync(b => b.TenantId == tenantId);

            if (branding == null)
                return BuildDefaultPublicDto();

            return new PublicSchoolBrandingDto
            {
                PrimaryColor = branding.PrimaryColor,
                SecondaryColor = branding.SecondaryColor,
                LogoUrl = branding.LogoUrl,
                FaviconUrl = branding.FaviconUrl,
                SchoolName = ResolveSchoolName(branding.SchoolName),
            };
        }
    }

    /* ==================== Writes ==================== */

    [AbpAuthorize(PermissionNames.Administration_Settings_Edit)]
    public async Task<SchoolBrandingDto> UpdateAsync(UpdateSchoolBrandingDto input)
    {
        var branding = await GetOrCreateForCurrentTenantAsync();

        try
        {
            branding.SetAppearance(input.PrimaryColor, input.SecondaryColor, input.SchoolName);
        }
        catch (ArgumentException ex)
        {
            // The DTO regex already covers this; the entity guard is the
            // backstop for any other caller, so surface it as a user error
            // rather than a 500.
            throw new UserFriendlyException(TenancyExceptionCodes.InvalidBrandingColor, ex.Message);
        }

        await _brandingRepository.UpdateAsync(branding);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToDto(branding);
    }

    [AbpAuthorize(PermissionNames.Administration_Settings_Edit)]
    public async Task<FileUploadTicket> RequestAssetUploadUrlAsync(RequestBrandingUploadUrlDto input)
    {
        var tenantId = GetTenantIdOrThrow();

        ValidateExtension(input.AssetType, input.FileName);

        var ext = Path.GetExtension(input.FileName).ToLowerInvariant();
        var key = $"{tenantId}/{input.AssetType.ToString().ToLowerInvariant()}/{Guid.NewGuid()}{ext}";

        return await _fileStorage.CreateUploadTicketAsync(BrandingBucket, key);
    }

    [AbpAuthorize(PermissionNames.Administration_Settings_Edit)]
    public async Task<SchoolBrandingDto> SetAssetAsync(SetBrandingAssetDto input)
    {
        var tenantId = GetTenantIdOrThrow();

        // The key must sit under this tenant's prefix — otherwise one tenant
        // could point its logo at another tenant's uploaded object.
        var expectedPrefix = $"{tenantId}/{input.AssetType.ToString().ToLowerInvariant()}/";
        if (string.IsNullOrWhiteSpace(input.ObjectKey)
            || !input.ObjectKey.StartsWith(expectedPrefix, StringComparison.Ordinal)
            || input.ObjectKey.Contains(".."))
        {
            throw new UserFriendlyException(TenancyExceptionCodes.InvalidBrandingUpload,
                "Invalid upload reference. Request an upload URL and upload the file first.");
        }

        ValidateExtension(input.AssetType, input.ObjectKey);

        var info = await _fileStorage.GetObjectInfoAsync(BrandingBucket, input.ObjectKey);
        if (info == null)
        {
            throw new UserFriendlyException(TenancyExceptionCodes.InvalidBrandingUpload,
                "The uploaded image was not found in storage. Please re-upload.");
        }

        // Validate the REAL stored size and type, never the client's claim.
        //
        // The bytes are already in the bucket by this point, so a rejection has
        // to delete them — otherwise every rejected attempt leaves a permanent,
        // publicly-readable orphan and the caller can repeat it without bound.
        var maxBytes = input.AssetType == BrandingAssetType.Logo ? MaxLogoBytes : MaxFaviconBytes;
        if (info.SizeBytes > maxBytes)
        {
            await TryDeleteStoredObjectAsync(input.ObjectKey, null);
            throw new UserFriendlyException(TenancyExceptionCodes.BrandingImageTooLarge,
                $"The image is {info.SizeBytes / 1024}KB. The maximum is {maxBytes / 1024}KB.");
        }

        if (!string.IsNullOrWhiteSpace(info.ContentType)
            && !info.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            await TryDeleteStoredObjectAsync(input.ObjectKey, null);
            throw new UserFriendlyException(TenancyExceptionCodes.BrandingImageTypeNotAllowed,
                "Only image files may be used for branding.");
        }

        var branding = await GetOrCreateForCurrentTenantAsync();
        var publicUrl = _fileStorage.GetPublicUrl(BrandingBucket, input.ObjectKey);

        var previousKey = input.AssetType == BrandingAssetType.Logo
            ? branding.LogoObjectKey
            : branding.FaviconObjectKey;

        if (input.AssetType == BrandingAssetType.Logo)
            branding.SetLogo(publicUrl, input.ObjectKey);
        else
            branding.SetFavicon(publicUrl, input.ObjectKey);

        await _brandingRepository.UpdateAsync(branding);
        await CurrentUnitOfWork.SaveChangesAsync();

        await TryDeleteStoredObjectAsync(previousKey, input.ObjectKey);

        return MapToDto(branding);
    }

    [AbpAuthorize(PermissionNames.Administration_Settings_Edit)]
    public async Task<SchoolBrandingDto> ClearAssetAsync(RemoveBrandingAssetDto input)
    {
        GetTenantIdOrThrow();

        var branding = await GetOrCreateForCurrentTenantAsync();

        var previousKey = input.AssetType == BrandingAssetType.Logo
            ? branding.LogoObjectKey
            : branding.FaviconObjectKey;

        if (input.AssetType == BrandingAssetType.Logo)
            branding.ClearLogo();
        else
            branding.ClearFavicon();

        await _brandingRepository.UpdateAsync(branding);
        await CurrentUnitOfWork.SaveChangesAsync();

        await TryDeleteStoredObjectAsync(previousKey, null);

        return MapToDto(branding);
    }

    /* ==================== Helpers ==================== */

    private int GetTenantIdOrThrow()
    {
        var tenantId = AbpSession.TenantId;
        if (tenantId == null)
        {
            throw new UserFriendlyException(TenancyExceptionCodes.BrandingTenantRequired,
                "Branding can only be configured from within a school. Switch to a tenant first.");
        }

        return tenantId.Value;
    }

    /// <summary>
    /// Returns the tenant's branding row, creating one seeded with the PSMS
    /// defaults on first write. Upserting here means existing tenants — which
    /// predate this feature and have no seeded row — work identically to newly
    /// provisioned ones, with no data backfill.
    /// </summary>
    private async Task<SchoolBranding> GetOrCreateForCurrentTenantAsync()
    {
        var tenantId = GetTenantIdOrThrow();

        var branding = await _brandingRepository
            .GetAll()
            .FirstOrDefaultAsync(b => b.TenantId == tenantId);

        if (branding != null)
            return branding;

        try
        {
            branding = new SchoolBranding(Guid.NewGuid(), tenantId);
            await _brandingRepository.InsertAsync(branding);
            await CurrentUnitOfWork.SaveChangesAsync();
            return branding;
        }
        catch (DbUpdateException)
        {
            // Two concurrent first-writes for a tenant with no row (Save
            // Branding while a logo's SetAsset is in flight, or two admin tabs)
            // both see null and both insert; the loser hits
            // IX_SchoolBrandings_TenantId. That is a benign race, not a 500 —
            // the row the winner created is exactly what we wanted, so re-read
            // it. Untracked because the failed insert is still in the change
            // tracker.
            var existing = await _brandingRepository
                .GetAll()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.TenantId == tenantId);

            if (existing == null) throw;

            return existing;
        }
    }

    private void ValidateExtension(BrandingAssetType assetType, string fileNameOrKey)
    {
        var allowed = assetType == BrandingAssetType.Logo ? LogoExtensions : FaviconExtensions;
        var ext = Path.GetExtension(fileNameOrKey);

        if (string.IsNullOrWhiteSpace(ext) || !allowed.Contains(ext))
        {
            throw new UserFriendlyException(TenancyExceptionCodes.BrandingImageTypeNotAllowed,
                $"Allowed file types: {string.Join(", ", allowed.OrderBy(e => e))}.");
        }
    }

    /// <summary>
    /// Best-effort cleanup of a replaced image. A storage failure here must not
    /// fail the request — the branding row is already saved and correct; the
    /// worst case is an orphaned object.
    /// </summary>
    private async Task TryDeleteStoredObjectAsync(string objectKey, string replacementKey)
    {
        if (string.IsNullOrWhiteSpace(objectKey) || objectKey == replacementKey)
            return;

        try
        {
            await _fileStorage.DeleteAsync(BrandingBucket, objectKey);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to delete replaced branding object '{objectKey}'.", ex);
        }
    }

    private SchoolBrandingDto MapToDto(SchoolBranding branding)
    {
        var dto = ObjectMapper.Map<SchoolBrandingDto>(branding);
        dto.SchoolName = ResolveSchoolName(branding.SchoolName);
        dto.IsConfigured = HasCustomBranding(branding);
        return dto;
    }

    /// <summary>
    /// Whether the tenant has actually chosen anything, as opposed to still
    /// carrying the seeded defaults. Deliberately not "a row exists" — tenant
    /// provisioning seeds a defaults-only row for every new school, so that
    /// test would be true for everyone and the UI's "using default branding"
    /// hint would never show.
    /// </summary>
    private static bool HasCustomBranding(SchoolBranding branding)
        => !string.Equals(branding.PrimaryColor, BrandingDefaults.PrimaryColor,
               StringComparison.OrdinalIgnoreCase)
           || !string.Equals(branding.SecondaryColor, BrandingDefaults.SecondaryColor,
               StringComparison.OrdinalIgnoreCase)
           || !string.IsNullOrWhiteSpace(branding.SchoolName)
           || !string.IsNullOrWhiteSpace(branding.LogoUrl)
           || !string.IsNullOrWhiteSpace(branding.FaviconUrl);

    private static SchoolBrandingDto BuildDefaultDto(int? tenantId) => new()
    {
        Id = null,
        TenantId = tenantId,
        PrimaryColor = BrandingDefaults.PrimaryColor,
        SecondaryColor = BrandingDefaults.SecondaryColor,
        LogoUrl = null,
        FaviconUrl = null,
        SchoolName = BrandingDefaults.SchoolName,
        ConfiguredSchoolName = null,
        IsConfigured = false,
    };

    private static PublicSchoolBrandingDto BuildDefaultPublicDto() => new()
    {
        PrimaryColor = BrandingDefaults.PrimaryColor,
        SecondaryColor = BrandingDefaults.SecondaryColor,
        LogoUrl = null,
        FaviconUrl = null,
        SchoolName = BrandingDefaults.SchoolName,
    };

    private static string ResolveSchoolName(string stored)
        => string.IsNullOrWhiteSpace(stored) ? BrandingDefaults.SchoolName : stored;
}
