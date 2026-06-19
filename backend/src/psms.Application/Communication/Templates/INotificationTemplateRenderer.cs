using psms.Domain.Shared.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace psms.Communication.Templates;

/// <summary>
/// COMM-06: renders a logical message into channel- and language-specific content.
/// Resolves the best template for (key, channel, language) — the requested language
/// or the default-language fallback — and substitutes {{variables}}.
/// </summary>
public interface INotificationTemplateRenderer
{
    Task<RenderedTemplate> RenderAsync(
        string templateKey,
        NotificationChannel channel,
        string language,
        IReadOnlyDictionary<string, string> variables);
}
