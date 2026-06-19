using Castle.Core.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Communication.Channels.WhatsApp;

/// <summary>
/// COMM-07 scaffold default: no real WhatsApp provider is wired yet, so this logs
/// and returns NotConfigured (a recorded non-failure) instead of attempting a send.
/// A real gateway (Meta Cloud API / CM.com / Clickatell) replaces this registration
/// in a follow-up once credentials + a webhook URL exist. Plain class (no marker
/// interface) — registered explicitly against IWhatsAppGateway in psmsApplicationModule.
/// </summary>
public class NotConfiguredWhatsAppGateway : IWhatsAppGateway
{
    public ILogger Logger { get; set; } = NullLogger.Instance;

    public Task<WhatsAppSendResult> SendTemplateAsync(
        string toNumber,
        string providerTemplateName,
        string language,
        IReadOnlyList<string> bodyParameters)
    {
        Logger.Info($"WhatsApp send skipped — no gateway configured (template '{providerTemplateName}', lang '{language}').");
        return Task.FromResult(WhatsAppSendResult.NotConfiguredResult());
    }
}
