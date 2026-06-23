using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Communication.Analytics.Dto;
using System.Threading.Tasks;

namespace psms.Communication.Analytics;

/// <summary>
/// COMM-12: read-side delivery analytics + audit over the notification delivery log.
/// </summary>
public interface INotificationAnalyticsAppService : IApplicationService
{
    /// <summary>Per-channel delivery counts by status for the (optional) period.</summary>
    Task<DeliveryAnalyticsSummaryDto> GetSummaryAsync(GetAnalyticsInput input);

    /// <summary>Paged delivery-log audit (who/what/when/which channel/outcome).</summary>
    Task<PagedResultDto<DeliveryLogListDto>> GetDeliveriesAsync(GetDeliveryLogInput input);
}
