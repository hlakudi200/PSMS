using psms.Domain.Shared.Enums;

namespace psms.Communication.Templates;

/// <summary>COMM-06: the result of rendering a template for a channel + language.</summary>
public class RenderedTemplate
{
    /// <summary>False when no active template matched the key/channel (caller falls back).</summary>
    public bool Found { get; set; }

    public string Title { get; set; }

    public string Body { get; set; }

    /// <summary>WhatsApp only — the Meta template name to invoke.</summary>
    public string ProviderTemplateName { get; set; }

    /// <summary>The language actually used (requested, or the default-language fallback).</summary>
    public string Language { get; set; }

    public TemplateApprovalStatus ApprovalStatus { get; set; }

    public static RenderedTemplate NotFound() => new RenderedTemplate { Found = false };
}
