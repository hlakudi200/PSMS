using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Communication.Analytics.Dto;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Communication.Analytics;

/// <summary>
/// COMM-12: delivery analytics + audit. Aggregates the NotificationDeliveryLog by
/// channel × status and serves the raw audit list. Config-grade (same permission as
/// notification/template management). Tenant-scoped via the ambient filter.
/// </summary>
[AbpAuthorize(PermissionNames.Communication_Notifications_Configure)]
public class NotificationAnalyticsAppService : ApplicationService, INotificationAnalyticsAppService
{
    private readonly IRepository<NotificationDeliveryLog, Guid> _deliveryLogRepository;

    public NotificationAnalyticsAppService(IRepository<NotificationDeliveryLog, Guid> deliveryLogRepository)
    {
        _deliveryLogRepository = deliveryLogRepository;
    }

    public async Task<DeliveryAnalyticsSummaryDto> GetSummaryAsync(GetAnalyticsInput input)
    {
        var query = _deliveryLogRepository.GetAll()
            .WhereIf(input?.From != null, l => l.CreationTime >= input.From.Value)
            .WhereIf(input?.To != null, l => l.CreationTime <= input.To.Value);

        // One grouped query: counts per (channel, status), pivoted in memory.
        var grouped = await query
            .GroupBy(l => new { l.Channel, l.Status })
            .Select(g => new { g.Key.Channel, g.Key.Status, Count = g.Count() })
            .ToListAsync();

        var byChannel = grouped
            .GroupBy(x => x.Channel)
            .Select(ch =>
            {
                int Count(NotificationDeliveryStatus s) => ch.Where(x => x.Status == s).Sum(x => x.Count);
                return new ChannelDeliveryStatsDto
                {
                    Channel = ch.Key,
                    Pending = Count(NotificationDeliveryStatus.Pending),
                    Sent = Count(NotificationDeliveryStatus.Sent),
                    Delivered = Count(NotificationDeliveryStatus.Delivered),
                    Read = Count(NotificationDeliveryStatus.Read),
                    Failed = Count(NotificationDeliveryStatus.Failed),
                    Suppressed = Count(NotificationDeliveryStatus.Suppressed),
                    Total = ch.Sum(x => x.Count)
                };
            })
            .OrderBy(c => c.Channel)
            .ToList();

        return new DeliveryAnalyticsSummaryDto
        {
            ByChannel = byChannel,
            Total = byChannel.Sum(c => c.Total),
            From = input?.From,
            To = input?.To
        };
    }

    public async Task<PagedResultDto<DeliveryLogListDto>> GetDeliveriesAsync(GetDeliveryLogInput input)
    {
        var query = _deliveryLogRepository.GetAll()
            .WhereIf(input.Channel.HasValue, l => l.Channel == input.Channel.Value)
            .WhereIf(input.Status.HasValue, l => l.Status == input.Status.Value)
            .WhereIf(input.RecipientUserId.HasValue, l => l.RecipientUserId == input.RecipientUserId.Value)
            .WhereIf(input.From != null, l => l.CreationTime >= input.From.Value)
            .WhereIf(input.To != null, l => l.CreationTime <= input.To.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<DeliveryLogListDto>(
            totalCount,
            items.Select(ToListDto).ToList());
    }

    private static DeliveryLogListDto ToListDto(NotificationDeliveryLog l) => new DeliveryLogListDto
    {
        Id = l.Id,
        Channel = l.Channel,
        RecipientUserId = l.RecipientUserId,
        Status = l.Status,
        ReferenceId = l.ReferenceId,
        Error = l.Error,
        AttemptCount = l.AttemptCount,
        CreationTime = l.CreationTime,
        SentDate = l.SentDate,
        DeliveredDate = l.DeliveredDate,
        ReadDate = l.ReadDate,
        FailedDate = l.FailedDate
    };
}
