using Castle.Core.Logging;
using psms.Domain.Shared.Enums;
using System.Threading.Tasks;

namespace psms.Communication.Channels.Push;

/// <summary>
/// COMM-10 scaffold default — no real push provider wired. Logs and returns
/// NotConfigured. A real gateway (FCM/APNS) replaces this registration.
/// Plain class (no marker) — registered explicitly in psmsApplicationModule.
/// </summary>
public class NotConfiguredPushGateway : IPushGateway
{
    public ILogger Logger { get; set; } = NullLogger.Instance;

    public Task<GatewaySendResult> SendAsync(string deviceToken, DevicePlatform platform, string title, string body, string actionUrl)
    {
        Logger.Info($"Push send skipped — no gateway configured ({platform}).");
        return Task.FromResult(GatewaySendResult.NotConfiguredResult("Push"));
    }
}
