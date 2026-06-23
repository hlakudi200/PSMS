using Castle.Core.Logging;
using System.Threading.Tasks;

namespace psms.Communication.Channels.Sms;

/// <summary>
/// COMM-08 scaffold default — no real SMS provider wired. Logs and returns
/// NotConfigured. A real gateway (Clickatell/Twilio) replaces this registration.
/// Plain class (no marker) — registered explicitly in psmsApplicationModule.
/// </summary>
public class NotConfiguredSmsGateway : ISmsGateway
{
    public ILogger Logger { get; set; } = NullLogger.Instance;

    public Task<GatewaySendResult> SendAsync(string toNumber, string body)
    {
        Logger.Info("SMS send skipped — no gateway configured.");
        return Task.FromResult(GatewaySendResult.NotConfiguredResult("SMS"));
    }
}
