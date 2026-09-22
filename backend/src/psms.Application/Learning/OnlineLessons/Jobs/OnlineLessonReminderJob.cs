using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using System.Threading.Tasks;

namespace psms.Learning.OnlineLessons.Jobs;

/// <summary>
/// US-TCH-004: sends one 24 h or 15 min lesson reminder to the enrolled
/// students. Queued with a delay by <see cref="OnlineLessonNotifier"/> when a
/// lesson is scheduled or rescheduled; delivery goes through the COMM-01
/// dispatcher, so there is no separate reminder scheduler. Runs without a
/// session, like NotificationDispatchJob.
/// </summary>
public class OnlineLessonReminderJob : AsyncBackgroundJob<OnlineLessonReminderJobArgs>, ITransientDependency
{
    private readonly OnlineLessonNotifier _notifier;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public OnlineLessonReminderJob(OnlineLessonNotifier notifier, IUnitOfWorkManager unitOfWorkManager)
    {
        _notifier = notifier;
        _unitOfWorkManager = unitOfWorkManager;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(OnlineLessonReminderJobArgs args)
    {
        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
        {
            await _notifier.SendReminderAsync(args);
            await _unitOfWorkManager.Current.SaveChangesAsync();
        }
    }
}
