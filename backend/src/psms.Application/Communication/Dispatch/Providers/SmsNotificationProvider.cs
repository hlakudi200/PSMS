using psms.Communication.Channels.Sms;
using psms.Communication.Contacts;
using psms.Communication.Templates;
using psms.Domain.Shared.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Communication.Dispatch.Providers;

/// <summary>
/// COMM-08: the SMS channel. Resolves the recipient's mobile number (COMM-04),
/// renders the SMS template (COMM-06) or uses the literal message, and sends via the
/// gateway. Consent is gated upstream by the dispatcher (COMM-05). The gateway is the
/// not-configured default today; the channel isn't routed until COMM-11.
/// </summary>
public class SmsNotificationProvider : INotificationChannelProvider
{
    private readonly IContactResolver _contactResolver;
    private readonly INotificationTemplateRenderer _templateRenderer;
    private readonly ISmsGateway _gateway;

    public SmsNotificationProvider(
        IContactResolver contactResolver,
        INotificationTemplateRenderer templateRenderer,
        ISmsGateway gateway)
    {
        _contactResolver = contactResolver;
        _templateRenderer = templateRenderer;
        _gateway = gateway;
    }

    public NotificationChannel Channel => NotificationChannel.Sms;

    public async Task<ChannelSendResult> SendAsync(NotificationRequest request, long recipientUserId)
    {
        var contacts = await _contactResolver.ResolveAsync(recipientUserId);
        if (!contacts.HasMobile)
            return ChannelSendResult.Failed(Channel, recipientUserId, "Recipient has no mobile number.");

        var body = request.Message;
        if (!string.IsNullOrWhiteSpace(request.TemplateKey))
        {
            var vars = request.Variables != null ? new Dictionary<string, string>(request.Variables) : null;
            var rendered = await _templateRenderer.RenderAsync(request.TemplateKey, Channel, request.Language, vars);
            if (rendered.Found) body = rendered.Body;
        }

        if (string.IsNullOrWhiteSpace(body))
            return ChannelSendResult.Failed(Channel, recipientUserId, "No SMS content to send.");

        var result = await _gateway.SendAsync(contacts.MobileNumber, body.Trim());
        return result.Success
            ? ChannelSendResult.Ok(Channel, recipientUserId, result.ProviderMessageId)
            : ChannelSendResult.Failed(Channel, recipientUserId, result.Error);
    }
}
