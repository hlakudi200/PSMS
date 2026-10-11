using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Tenancy;
using psms.Domain.Tenancy.Entities;
using psms.MultiTenancy;

namespace psms.Shared.Pdf;

/// <summary>
/// The school's identity as it is printed at the top of a document.
/// </summary>
public class SchoolLetterhead
{
    public string SchoolName { get; set; }
    public string PrimaryColor { get; set; } = BrandingDefaults.PrimaryColor;
    public string SecondaryColor { get; set; } = BrandingDefaults.SecondaryColor;

    /// <summary>The logo's bytes, or null when the school has none we can draw.</summary>
    public byte[] LogoBytes { get; set; }

    /// <summary>The school stamp's bytes, or null.</summary>
    public byte[] StampBytes { get; set; }
}

/// <summary>
/// Reads the school's branding — name, colours, logo, stamp — for a printed
/// document, so it looks like it came from the school rather than from PSMS
/// (issue #56). Shared by the report card and the fee statement.
/// <para>
/// The caller decides the tenant filter: report cards are built in background
/// jobs where the session has no tenant, so the tenant is passed in explicitly.
/// </para>
/// </summary>
public class SchoolLetterheadLoader : ITransientDependency
{
    /// <summary>
    /// How long to wait for the school logo before printing without it. A
    /// document must not fail, or hang a background job, over a decoration.
    /// </summary>
    private static readonly TimeSpan LogoFetchTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Beyond this a "logo" is not a logo, and we skip it.</summary>
    private const int MaxLogoBytes = 2 * 1024 * 1024;

    private static readonly HttpClient LogoClient = new HttpClient
    {
        Timeout = LogoFetchTimeout
    };

    private readonly IRepository<SchoolBranding, Guid> _brandingRepository;
    private readonly TenantManager _tenantManager;
    public Castle.Core.Logging.ILogger Logger { get; set; } = Castle.Core.Logging.NullLogger.Instance;

    public SchoolLetterheadLoader(
        IRepository<SchoolBranding, Guid> brandingRepository,
        TenantManager tenantManager)
    {
        _brandingRepository = brandingRepository;
        _tenantManager = tenantManager;
    }

    public async Task<SchoolLetterhead> LoadAsync(int? tenantId, bool includeStamp = true)
    {
        // The branding row carries the display name; the tenancy name is only a
        // fallback for a tenant that has never been branded.
        string schoolName = null;
        var letterhead = new SchoolLetterhead();
        string logoUrl = null;
        string stampUrl = null;

        if (tenantId.HasValue)
        {
            var branding = await _brandingRepository
                .GetAll()
                .FirstOrDefaultAsync(b => b.TenantId == tenantId.Value);

            if (branding != null)
            {
                if (!string.IsNullOrWhiteSpace(branding.SchoolName))
                    schoolName = branding.SchoolName;
                if (SchoolBranding.IsValidHexColor(branding.PrimaryColor))
                    letterhead.PrimaryColor = branding.PrimaryColor;
                if (SchoolBranding.IsValidHexColor(branding.SecondaryColor))
                    letterhead.SecondaryColor = branding.SecondaryColor;
                logoUrl = branding.LogoUrl;
                stampUrl = branding.StampUrl;
            }

            if (string.IsNullOrWhiteSpace(schoolName))
            {
                var tenant = await _tenantManager.FindByIdAsync(tenantId.Value);
                if (tenant != null)
                    schoolName = tenant.TenancyName;
            }
        }

        letterhead.SchoolName = string.IsNullOrWhiteSpace(schoolName) ? "School" : schoolName;
        letterhead.LogoBytes = await TryFetchImageAsync(logoUrl, "logo");
        if (includeStamp)
            letterhead.StampBytes = await TryFetchImageAsync(stampUrl, "stamp");

        return letterhead;
    }

    /// <summary>
    /// Reads the image bytes so QuestPDF can embed them. Returns null on anything
    /// going wrong — a slow host, a 404, a file that is not an image — because a
    /// document has to print with or without the badge on it.
    /// </summary>
    private async Task<byte[]> TryFetchImageAsync(string url, string what)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            Logger.Warn($"School {what} URL is not a usable http(s) address, skipping it: {url}");
            return null;
        }

        try
        {
            using var cts = new CancellationTokenSource(LogoFetchTimeout);
            using var response = await LogoClient.GetAsync(uri, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                Logger.Warn($"School {what} fetch returned {(int)response.StatusCode}, printing without it.");
                return null;
            }

            if (response.Content.Headers.ContentLength > MaxLogoBytes)
            {
                Logger.Warn($"School {what} is larger than the embed limit, printing without it.");
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return bytes.Length == 0 || bytes.Length > MaxLogoBytes ? null : bytes;
        }
        catch (Exception ex)
        {
            Logger.Warn($"Could not read the school {what} for a printed document: {ex.Message}");
            return null;
        }
    }
}
