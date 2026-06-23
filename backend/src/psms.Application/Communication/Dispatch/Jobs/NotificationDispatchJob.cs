using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using System.Threading.Tasks;

namespace psms.Communication.Dispatch.Jobs;

/// <summary>
/// COMM-11: runs a queued notification dispatch in the background (off the request
/// thread), so bulk/external sends don't block and get ABP's built-in job retries.
/// Runs without a session — disables the tenant filter and carries TenantId in the
/// request, mirroring the existing report-PDF job.
/// </summary>
public class NotificationDispatchJob : AsyncBackgroundJob<NotificationDispatchJobArgs>, ITransientDependency
{
    private readonly INotificationDispatcher _dispatcher;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public NotificationDispatchJob(INotificationDispatcher dispatcher, IUnitOfWorkManager unitOfWorkManager)
    {
        _dispatcher = dispatcher;
        _unitOfWorkManager = unitOfWorkManager;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(NotificationDispatchJobArgs args)
    {
        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
        {
            var request = new NotificationRequest
            {
                TenantId = args.TenantId,
                RecipientUserIds = args.RecipientUserIds,
                Type = args.Type,
                Priority = args.Priority,
                Title = args.Title,
                Message = args.Message,
                ActionUrl = args.ActionUrl,
                EntityType = args.EntityType,
                EntityId = args.EntityId,
                IdempotencyKey = args.IdempotencyKey,
                TemplateKey = args.TemplateKey,
                Language = args.Language,
                Variables = args.Variables,
                RequestedChannels = args.RequestedChannels
            };

            await _dispatcher.DispatchAsync(request);
            await _unitOfWorkManager.Current.SaveChangesAsync();
        }
    }
}
