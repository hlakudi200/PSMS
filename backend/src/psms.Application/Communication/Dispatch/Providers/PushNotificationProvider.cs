using psms.Communication.Channels.Push;
using psms.Communication.Contacts;
using psms.Communication.Templates;
using psms.Domain.Shared.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Communication.Dispatch.Providers;

/// <summary>
/// COMM-10: the push channel. Resolves the recipient's active device tokens (COMM-04),
/// renders the push template (COMM-06) or uses the literal title/message, and sends to
/// each token via the gateway. Success if any token accepts. Consent is gated upstream
/// by the dispatcher (COMM-05). The gateway is the not-configured default today; the
/// channel isn't routed until COMM-11.
/// </summary>
public class PushNotificationProvider : INotificationChannelProvider
{
    private readonly IContactResolver _contactResolver;
    private readonly INotificationTemplateRenderer _templateRenderer;
    private readonly IPushGateway _gateway;

    public PushNotificationProvider(
        IContactResolver contactResolver,
        INotificationTemplateRenderer templateRenderer,
        IPushGateway gateway)
    {
        _contactResolver = contactResolver;
        _templateRenderer = templateRenderer;
        _gateway = gateway;
    }

    public NotificationChannel Channel => NotificationChannel.Push;

    public async Task<ChannelSendResult> SendAsync(NotificationRequest request, long recipientUserId)
    {
        var contacts = await _contactResolver.ResolveAsync(recipientUserId);
        if (!contacts.HasDevices)
            return ChannelSendResult.Failed(Channel, recipientUserId, "Recipient has no registered devices.");

        var title = request.Title;
        var body = request.Message;
        if (!string.IsNullOrWhiteSpace(request.TemplateKey))
        {
            var vars = request.Variables != null ? new Dictionary<string, string>(request.Variables) : null;
            var rendered = await _templateRenderer.RenderAsync(request.TemplateKey, Channel, request.Language, vars);
            if (rendered.Found)
            {
                title = rendered.Title;
                body = rendered.Body;
            }
        }

        var anySuccess = false;
        var allNotConfigured = true;
        string firstMessageId = null;
        string lastError = null;

        foreach (var device in contacts.DeviceTokens)
        {
            var result = await _gateway.SendAsync(device.Token, device.Platform, title, body, request.ActionUrl);
            if (!result.NotConfigured) allNotConfigured = false;
            if (result.Success)
            {
                anySuccess = true;
                firstMessageId ??= result.ProviderMessageId;
            }
            else
            {
                lastError = result.Error;
            }
        }

        if (anySuccess)
            return ChannelSendResult.Ok(Channel, recipientUserId, firstMessageId);

        return ChannelSendResult.Failed(Channel, recipientUserId,
            allNotConfigured ? "Push gateway is not configured." : (lastError ?? "Push send failed."));
    }
}
