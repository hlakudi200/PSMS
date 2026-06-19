using psms.Communication.Channels.WhatsApp;
using psms.Communication.Contacts;
using psms.Communication.Templates;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Communication.Dispatch.Providers;

/// <summary>
/// COMM-07: the WhatsApp channel. Resolves the recipient's WhatsApp number (COMM-04),
/// renders the WhatsApp template (COMM-06), requires it to be Approved with a Meta
/// template name, builds the ordered body parameters, and sends via the gateway.
/// The dispatcher has already gated consent (COMM-05) before reaching here. Today
/// the gateway is the not-configured default, so this returns a Failed result until
/// a real gateway is wired — and the channel isn't routed until COMM-11.
/// </summary>
public class WhatsAppNotificationProvider : INotificationChannelProvider
{
    private readonly IContactResolver _contactResolver;
    private readonly INotificationTemplateRenderer _templateRenderer;
    private readonly IWhatsAppGateway _gateway;

    public WhatsAppNotificationProvider(
        IContactResolver contactResolver,
        INotificationTemplateRenderer templateRenderer,
        IWhatsAppGateway gateway)
    {
        _contactResolver = contactResolver;
        _templateRenderer = templateRenderer;
        _gateway = gateway;
    }

    public NotificationChannel Channel => NotificationChannel.WhatsApp;

    public async Task<ChannelSendResult> SendAsync(NotificationRequest request, long recipientUserId)
    {
        var contacts = await _contactResolver.ResolveAsync(recipientUserId);
        if (!contacts.HasWhatsApp)
            return ChannelSendResult.Failed(Channel, recipientUserId, "Recipient has no WhatsApp number.");

        if (string.IsNullOrWhiteSpace(request.TemplateKey))
            return ChannelSendResult.Failed(Channel, recipientUserId, "WhatsApp requires a template.");

        var vars = request.Variables != null ? new Dictionary<string, string>(request.Variables) : null;
        var rendered = await _templateRenderer.RenderAsync(request.TemplateKey, Channel, request.Language, vars);

        if (!rendered.Found)
            return ChannelSendResult.Failed(Channel, recipientUserId, "No WhatsApp template for this key.");
        if (rendered.ApprovalStatus != TemplateApprovalStatus.Approved)
            return ChannelSendResult.Failed(Channel, recipientUserId, "WhatsApp template is not approved.");
        if (string.IsNullOrWhiteSpace(rendered.ProviderTemplateName))
            return ChannelSendResult.Failed(Channel, recipientUserId, "WhatsApp template has no Meta template name.");

        var bodyParameters = BuildBodyParameters(rendered.ProviderParameterKeys, request.Variables);

        var result = await _gateway.SendTemplateAsync(
            contacts.WhatsAppNumber, rendered.ProviderTemplateName, rendered.Language, bodyParameters);

        if (result.Success)
            return ChannelSendResult.Ok(Channel, recipientUserId, result.ProviderMessageId);

        // NotConfigured (scaffold default) collapses into Failed here — the delivery
        // log gets the gateway's reason text. If ops ever want a distinct
        // "channel unavailable" status, plumb a third state through ChannelSendResult.
        return ChannelSendResult.Failed(Channel, recipientUserId, result.Error);
    }

    /// <summary>
    /// Maps the template's ordered ProviderParameterKeys ("a,b,c") to Meta's
    /// positional body params, pulling each value from the request variables
    /// (missing → empty, so positions stay aligned).
    /// </summary>
    private static IReadOnlyList<string> BuildBodyParameters(string providerParameterKeys, IDictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(providerParameterKeys))
            return Array.Empty<string>();

        return providerParameterKeys
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(key => variables != null && variables.TryGetValue(key, out var value) ? value ?? string.Empty : string.Empty)
            .ToList();
    }
}
