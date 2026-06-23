using psms.Communication.Channels.Email;
using psms.Communication.Contacts;
using psms.Communication.Templates;
using psms.Domain.Shared.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Communication.Dispatch.Providers;

/// <summary>
/// COMM-09: the email channel. Resolves the recipient's email (COMM-04), renders the
/// email template (COMM-06; variable values are HTML-encoded for the Email channel)
/// or uses the literal title/message, and sends via the gateway. Consent is gated
/// upstream by the dispatcher (COMM-05). The gateway is the not-configured default
/// today; the channel isn't routed until COMM-11.
/// </summary>
public class EmailNotificationProvider : INotificationChannelProvider
{
    private readonly IContactResolver _contactResolver;
    private readonly INotificationTemplateRenderer _templateRenderer;
    private readonly IEmailGateway _gateway;

    public EmailNotificationProvider(
        IContactResolver contactResolver,
        INotificationTemplateRenderer templateRenderer,
        IEmailGateway gateway)
    {
        _contactResolver = contactResolver;
        _templateRenderer = templateRenderer;
        _gateway = gateway;
    }

    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task<ChannelSendResult> SendAsync(NotificationRequest request, long recipientUserId)
    {
        var contacts = await _contactResolver.ResolveAsync(recipientUserId);
        if (!contacts.HasEmail)
            return ChannelSendResult.Failed(Channel, recipientUserId, "Recipient has no email address.");

        var subject = request.Title;
        string htmlBody = null;
        var fromTemplate = false;
        if (!string.IsNullOrWhiteSpace(request.TemplateKey))
        {
            var vars = request.Variables != null ? new Dictionary<string, string>(request.Variables) : null;
            var rendered = await _templateRenderer.RenderAsync(request.TemplateKey, Channel, request.Language, vars);
            if (rendered.Found)
            {
                subject = rendered.Title;
                htmlBody = rendered.Body; // already HTML (values encoded at render time)
                fromTemplate = true;
            }
        }

        if (!fromTemplate)
        {
            // Literal fallback: request.Message is plain text, so HTML-encode it and
            // preserve line breaks before sending as an HTML body (don't emit raw markup).
            htmlBody = string.IsNullOrWhiteSpace(request.Message)
                ? null
                : System.Net.WebUtility.HtmlEncode(request.Message).Replace("\n", "<br>");
        }

        if (string.IsNullOrWhiteSpace(htmlBody))
            return ChannelSendResult.Failed(Channel, recipientUserId, "No email content to send.");

        var result = await _gateway.SendAsync(contacts.Email, subject?.Trim(), htmlBody);
        return result.Success
            ? ChannelSendResult.Ok(Channel, recipientUserId, result.ProviderMessageId)
            : ChannelSendResult.Failed(Channel, recipientUserId, result.Error);
    }
}
