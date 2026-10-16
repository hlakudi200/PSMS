using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Admissions.Offers;

/// <summary>What one sweep found and did, so it can be reported rather than guessed at.</summary>
public class ExpiredOfferReport
{
    public int? TenantId { get; set; }
    public List<string> Expired { get; set; } = new List<string>();
    public int Count => Expired.Count;
}

/// <summary>
/// ADM-030: releases places the school offered and nobody took.
/// <para>
/// <c>Application.Expire()</c> had no callers at all. Expiry was noticed only
/// when somebody tried to enrol, which meant that until then an ignored offer
/// kept its application at <c>Approved</c>, kept counting against the grade's
/// places, and kept that place out of reach of the waiting list — for ever. A
/// family that never answered quietly held a seat.
/// </para>
/// <para>
/// An accepted offer that has not been enrolled yet is deliberately left alone.
/// That is the school's own backlog, not a lapsed offer, and taking the place
/// back from a family who said yes is the opposite of what this is for.
/// </para>
/// </summary>
public class AdmissionOfferExpirer : ITransientDependency
{
    private readonly IRepository<Application, Guid> _applications;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public AdmissionOfferExpirer(
        IRepository<Application, Guid> applications,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _applications = applications;
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// Expires every offer of one school that ran out without being accepted.
    /// Pass null to sweep every school, which is what the worker does.
    /// </summary>
    public async Task<List<ExpiredOfferReport>> ExpireLapsedOffersAsync(int? tenantId)
    {
        var now = DateTime.UtcNow;

        /* The worker runs without a session, so the tenant filter would see no
           tenant and return nothing. Disabled here and the scoping done by
           hand, which is also what lets one school be swept on its own. */
        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
        {
            var lapsed = await _applications.GetAll()
                .Where(a => a.Status == ApplicationStatus.Approved
                            && a.ExpiryDate.HasValue
                            && a.ExpiryDate.Value < now
                            // Said yes: the school owes them a place, not a lapse.
                            && a.OfferAcceptedDate == null
                            && a.OfferDeclinedDate == null)
                .WhereIf(tenantId.HasValue, a => a.TenantId == tenantId.Value)
                .ToListAsync();

            var byTenant = new Dictionary<int?, ExpiredOfferReport>();

            foreach (var application in lapsed)
            {
                if (!byTenant.TryGetValue(application.TenantId, out var report))
                    byTenant[application.TenantId] = report = new ExpiredOfferReport { TenantId = application.TenantId };

                try
                {
                    application.Expire();
                    await _applications.UpdateAsync(application);
                    report.Expired.Add(application.ApplicationNumber);
                }
                catch (InvalidOperationException ex)
                {
                    /* The entity is the authority on what may expire. One it
                       refuses is a row the query should not have matched, and
                       it must not take the rest of the sweep down with it. */
                    Logger.Warn($"Could not expire {application.ApplicationNumber}: {ex.Message}");
                }
            }

            if (lapsed.Count > 0)
                await _unitOfWorkManager.Current.SaveChangesAsync();

            foreach (var report in byTenant.Values)
                Logger.Info($"Expired {report.Count} lapsed admission offer(s) for tenant {report.TenantId}.");

            return byTenant.Values.OrderBy(r => r.TenantId).ToList();
        }
    }
}
