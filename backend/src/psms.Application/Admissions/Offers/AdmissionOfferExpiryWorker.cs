using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using System;

namespace psms.Admissions.Offers;

/// <summary>
/// Sweeps every school hourly for offers that ran out and were never answered.
/// <para>
/// The first periodic worker in the system — everything background until now
/// has been a job queued in response to something happening. An offer running
/// out is the opposite: it is time passing while nobody does anything, so
/// there is no event to hang it on.
/// </para>
/// <para>
/// Hourly rather than nightly because the family is told the offer has lapsed
/// (and the place goes back to the grade) the moment it does, and a deadline
/// that is enforced up to twenty-four hours late is not a deadline. Cheap
/// either way: the query is indexed on status and touches nothing when there
/// is nothing to expire.
/// </para>
/// </summary>
public class AdmissionOfferExpiryWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
{
    private readonly IIocResolver _iocResolver;

    public AdmissionOfferExpiryWorker(AbpTimer timer, IIocResolver iocResolver)
        : base(timer)
    {
        _iocResolver = iocResolver;
        Timer.Period = (int)TimeSpan.FromHours(1).TotalMilliseconds;

        /* Run on start as well as on the hour. A deployment that restarts often
           would otherwise keep postponing the sweep, and on a host that sleeps
           between requests the wake-up may be the only chance it gets. */
        Timer.RunOnStart = true;
    }

    protected override void DoWork()
    {
        using (var expirer = _iocResolver.ResolveAsDisposable<AdmissionOfferExpirer>())
        using (var unitOfWorkManager = _iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
        using (var unitOfWork = unitOfWorkManager.Object.Begin())
        {
            // null: every school. The worker has no session and belongs to none.
            AsyncHelper.RunSync(() => expirer.Object.ExpireLapsedOffersAsync(null));
            unitOfWork.Complete();
        }
    }
}
