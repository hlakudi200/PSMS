using psms.Domain.Shared.Enums;
using System.Threading.Tasks;

namespace psms.Communication.Channels.Push;

/// <summary>
/// COMM-10: provider-agnostic push gateway. The concrete impl (FCM for Android/web,
/// APNS for iOS — credentials/keys) is wired later; the scaffold ships a graceful
/// "not configured" default. Sends to a single device token; the provider fans out
/// over a recipient's tokens and prunes ones the gateway reports invalid.
/// </summary>
public interface IPushGateway
{
    Task<GatewaySendResult> SendAsync(string deviceToken, DevicePlatform platform, string title, string body, string actionUrl);
}
