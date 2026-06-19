using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace psms.Communication.Templates;

/// <summary>
/// COMM-06: loads the active template for (key, channel) in the requested language,
/// falling back to the default language, and substitutes {{placeholders}}.
/// </summary>
public class NotificationTemplateRenderer : INotificationTemplateRenderer, ITransientDependency
{
    // {{ name }} — tolerant of surrounding whitespace.
    private static readonly Regex PlaceholderRegex = new Regex(@"\{\{\s*(\w+)\s*\}\}", RegexOptions.Compiled);

    private readonly IRepository<NotificationTemplate, Guid> _templateRepository;

    public NotificationTemplateRenderer(IRepository<NotificationTemplate, Guid> templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<RenderedTemplate> RenderAsync(
        string templateKey,
        NotificationChannel channel,
        string language,
        IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrWhiteSpace(templateKey))
            return RenderedTemplate.NotFound();

        var requested = string.IsNullOrWhiteSpace(language)
            ? NotificationTemplate.DefaultLanguage
            : language.Trim().ToLowerInvariant();

        // Pull the requested + default-language rows for this key/channel in one go,
        // then prefer the requested language and fall back to the default.
        var candidates = await _templateRepository.GetAll()
            .Where(t => t.TemplateKey == templateKey
                && t.Channel == channel
                && t.IsActive
                && (t.Language == requested || t.Language == NotificationTemplate.DefaultLanguage))
            .ToListAsync();

        if (candidates.Count == 0)
            return RenderedTemplate.NotFound();

        var template = candidates.FirstOrDefault(t => t.Language == requested)
            ?? candidates.FirstOrDefault(t => t.Language == NotificationTemplate.DefaultLanguage);

        if (template == null)
            return RenderedTemplate.NotFound();

        return new RenderedTemplate
        {
            Found = true,
            Title = Substitute(template.Title, variables),
            Body = Substitute(template.Body, variables),
            ProviderTemplateName = template.ProviderTemplateName,
            Language = template.Language,
            ApprovalStatus = template.ApprovalStatus
        };
    }

    private static string Substitute(string text, IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return PlaceholderRegex.Replace(text, match =>
        {
            var key = match.Groups[1].Value;
            if (variables != null && variables.TryGetValue(key, out var value))
                return value ?? string.Empty;
            // Unknown placeholder → blank, so a missing variable never leaks "{{x}}".
            return string.Empty;
        });
    }
}
