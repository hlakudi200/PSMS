using Abp.Application.Services;
using Abp.Authorization;
using psms.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Admissions.Offers;

/// <summary>
/// Runs the offer sweep for the signed-in school, now, and says what it did.
/// <para>
/// The worker does this hourly. This exists because an hourly sweep is
/// unverifiable and unforceable: nobody can confirm it works without waiting,
/// and a school that has just corrected an expiry date should not have to.
/// The deployment host also sleeps between requests, so the timer is not a
/// promise.
/// </para>
/// </summary>
[AbpAuthorize(PermissionNames.Admissions_Decision)]
public class AdmissionOfferMaintenanceService : ApplicationService
{
    private readonly AdmissionOfferExpirer _expirer;

    public AdmissionOfferMaintenanceService(AdmissionOfferExpirer expirer)
    {
        _expirer = expirer;
    }

    /// <summary>
    /// Expires this school's offers that ran out without being accepted, and
    /// returns which. Safe to run repeatedly — a second run finds nothing.
    /// </summary>
    public async Task<ExpiredOfferReport> ExpireLapsedOffersAsync()
    {
        var reports = await _expirer.ExpireLapsedOffersAsync(AbpSession.TenantId);
        await CurrentUnitOfWork.SaveChangesAsync();

        // One school asked, so one report — empty rather than null when there
        // was nothing to expire, which is the common and uninteresting case.
        return reports.Count > 0
            ? reports[0]
            : new ExpiredOfferReport { TenantId = AbpSession.TenantId, Expired = new List<string>() };
    }
}
