namespace psms.Communication.Dispatch.Routing;

/// <summary>
/// COMM-11: decides which channels a request fans out to, and in what order. The
/// default policy is driven by the request's opted-in channels (so a plain in-app
/// create never spams external channels); admin-configurable per-category rules are
/// a later refinement.
/// </summary>
public interface INotificationRoutingPolicy
{
    RoutingPlan Resolve(NotificationRequest request);
}
