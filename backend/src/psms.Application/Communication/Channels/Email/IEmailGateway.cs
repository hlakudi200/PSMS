using System.Threading.Tasks;

namespace psms.Communication.Channels.Email;

/// <summary>
/// COMM-09: provider-agnostic email gateway. The concrete impl (SendGrid / Amazon SES
/// / Mailgun — HTTP/SMTP + credentials, from address, SPF/DKIM/DMARC) is wired later;
/// the scaffold ships a graceful "not configured" default. The body is HTML (values
/// are HTML-encoded at render time for the Email channel).
/// </summary>
public interface IEmailGateway
{
    Task<GatewaySendResult> SendAsync(string toEmail, string subject, string htmlBody);
}
