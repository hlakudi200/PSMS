using Abp.Dependency;
using Abp.Runtime.Caching;
using System;
using System.Threading.Tasks;

namespace psms.Authorization.Accounts;

/// <summary>
/// How often one caller may create an account.
/// <para>
/// Sign-up is the one door in this system a stranger may walk through, so it
/// is the one that needs a limit on it. Without one, a script can fill the
/// tenant with users — and because every applicant account is a real person's
/// name and email address, that is a POPIA problem as much as a housekeeping
/// one.
/// </para>
/// <para>
/// Counted per caller and per tenant over a rolling window. Deliberately not a
/// per-email limit: the abuse case uses a different address every time, and a
/// genuine parent who mistypes their password twice should not be locked out of
/// their own address.
/// </para>
/// <para>
/// <b>A stated limit.</b> This is held in the application cache, so it is
/// per-instance and resets on restart. That is enough to stop a script and not
/// enough to stop a determined, distributed attempt; a real answer to that is
/// a captcha or a WAF rule, neither of which this system has yet.
/// </para>
/// </summary>
public class ApplicantSignUpThrottle : ITransientDependency
{
    /// <summary>Sign-ups one caller may make before being asked to wait.</summary>
    public const int AllowedPerWindow = 5;

    /// <summary>How long that window is.</summary>
    public static readonly TimeSpan Window = TimeSpan.FromHours(1);

    private const string CacheName = "psms.ApplicantSignUps";

    private readonly ICacheManager _cacheManager;

    public ApplicantSignUpThrottle(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    /// <summary>
    /// Records an attempt from <paramref name="caller"/> and says whether it is
    /// within the limit. An unknown caller is counted under one shared key
    /// rather than waved through.
    /// </summary>
    public async Task<bool> IsWithinLimitAsync(int? tenantId, string caller)
    {
        var key = $"{tenantId?.ToString() ?? "host"}|{(string.IsNullOrWhiteSpace(caller) ? "unknown" : caller)}";

        var cache = _cacheManager.GetCache(CacheName);

        var soFar = await cache.GetOrDefaultAsync(key) as int? ?? 0;

        if (soFar >= AllowedPerWindow)
            return false;

        // Absolute, not sliding: the window is "five in an hour", not "five
        // without a quiet hour", so a steady trickle still hits the limit.
        await cache.SetAsync(key, soFar + 1, slidingExpireTime: null,
            absoluteExpireTime: DateTimeOffset.UtcNow.Add(Window));
        return true;
    }
}
