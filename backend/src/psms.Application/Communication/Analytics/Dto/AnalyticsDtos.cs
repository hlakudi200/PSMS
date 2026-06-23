using Abp.Application.Services.Dto;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;

namespace psms.Communication.Analytics.Dto;

/// <summary>COMM-12: per-channel delivery counts by status.</summary>
public class ChannelDeliveryStatsDto
{
    public NotificationChannel Channel { get; set; }
    public int Pending { get; set; }
    public int Sent { get; set; }
    public int Delivered { get; set; }
    public int Read { get; set; }
    public int Failed { get; set; }
    public int Suppressed { get; set; }
    public int Total { get; set; }
}

/// <summary>COMM-12: the analytics summary across all channels for a period.</summary>
public class DeliveryAnalyticsSummaryDto
{
    public List<ChannelDeliveryStatsDto> ByChannel { get; set; } = new List<ChannelDeliveryStatsDto>();
    public int Total { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

/// <summary>COMM-12: a delivery-log row for the audit list.</summary>
public class DeliveryLogListDto : EntityDto<Guid>
{
    public NotificationChannel Channel { get; set; }
    public long RecipientUserId { get; set; }
    public NotificationDeliveryStatus Status { get; set; }
    public string ReferenceId { get; set; }
    public string Error { get; set; }
    public int AttemptCount { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? SentDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
    public DateTime? ReadDate { get; set; }
    public DateTime? FailedDate { get; set; }
}

public class GetAnalyticsInput
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public class GetDeliveryLogInput : PagedAndSortedResultRequestDto
{
    public NotificationChannel? Channel { get; set; }
    public NotificationDeliveryStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public long? RecipientUserId { get; set; }
}
