using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Communication.Channels.WhatsApp;

/// <summary>
/// COMM-07: provider-agnostic WhatsApp send gateway. The concrete implementation
/// (Meta Cloud API / CM.com / Clickatell — HTTP + credentials) is a follow-up;
/// the scaffold ships a graceful "not configured" default so the channel resolves
/// and no-ops cleanly until a real gateway is wired. Sends a pre-approved template
/// with ordered body parameters (Meta uses its own stored template text).
/// </summary>
public interface IWhatsAppGateway
{
    // NOTE: bodyParameters models the common case — positional BODY TEXT params.
    // Typed params (currency/date/media) and header/button components will need a
    // richer shape; that's an additive widening for the real-gateway follow-up, not
    // a break to this signature. The sender (Meta phone_number_id) is config-injected
    // by the concrete gateway, not passed per call.
    Task<WhatsAppSendResult> SendTemplateAsync(
        string toNumber,
        string providerTemplateName,
        string language,
        IReadOnlyList<string> bodyParameters);
}
