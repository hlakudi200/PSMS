using Castle.Core.Logging;
using System.Threading.Tasks;

namespace psms.Communication.Channels.Email;

/// <summary>
/// COMM-09 scaffold default — no real email provider wired. Logs and returns
/// NotConfigured. A real gateway (SendGrid/SES/Mailgun) replaces this registration.
/// Plain class (no marker) — registered explicitly in psmsApplicationModule.
/// </summary>
public class NotConfiguredEmailGateway : IEmailGateway
{
    public ILogger Logger { get; set; } = NullLogger.Instance;

    public Task<GatewaySendResult> SendAsync(string toEmail, string subject, string htmlBody)
    {
        Logger.Info("Email send skipped — no gateway configured.");
        return Task.FromResult(GatewaySendResult.NotConfiguredResult("Email"));
    }
}
