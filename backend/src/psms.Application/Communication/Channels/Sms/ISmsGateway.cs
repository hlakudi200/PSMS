using System.Threading.Tasks;

namespace psms.Communication.Channels.Sms;

/// <summary>
/// COMM-08: provider-agnostic SMS gateway. The concrete impl (Clickatell / Twilio —
/// HTTP + credentials) is wired later; the scaffold ships a graceful "not configured"
/// default. Sender id and credentials are config-injected by the concrete gateway.
/// </summary>
public interface ISmsGateway
{
    Task<GatewaySendResult> SendAsync(string toNumber, string body);
}
