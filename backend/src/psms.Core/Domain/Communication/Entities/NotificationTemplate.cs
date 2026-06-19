using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Communication.Entities
{
    /// <summary>
    /// COMM-06: one channel- and language-specific rendering of a logical message.
    /// Keyed by (TemplateKey × Channel × Language): e.g. key "report.published" has
    /// an SMS/en row, a WhatsApp/en row (with a Meta-approved template name), an
    /// email/en row, etc. The renderer substitutes {{variables}} and falls back to
    /// the default language when a translation is missing.
    /// </summary>
    [Table("NotificationTemplates")]
    public class NotificationTemplate : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public const string DefaultLanguage = "en";

        public const int MaxKeyLength = 128;
        public const int MaxLanguageLength = 10;
        public const int MaxTitleLength = 200;
        public const int MaxBodyLength = 4000;
        public const int MaxProviderTemplateNameLength = 200;

        public int? TenantId { get; set; }

        /// <summary>Logical message key, e.g. "report.published".</summary>
        [Required]
        [StringLength(MaxKeyLength)]
        public string TemplateKey { get; set; }

        public NotificationChannel Channel { get; set; }

        /// <summary>ISO language code, e.g. "en", "af", "zu".</summary>
        [Required]
        [StringLength(MaxLanguageLength)]
        public string Language { get; set; }

        /// <summary>Optional — email subject / in-app + push title.</summary>
        [StringLength(MaxTitleLength)]
        public string Title { get; set; }

        [Required]
        [StringLength(MaxBodyLength)]
        public string Body { get; set; }

        /// <summary>WhatsApp only — the Meta-registered template name.</summary>
        [StringLength(MaxProviderTemplateNameLength)]
        public string ProviderTemplateName { get; set; }

        public TemplateApprovalStatus ApprovalStatus { get; set; }

        public bool IsActive { get; set; }

        protected NotificationTemplate()
        {
        }

        public NotificationTemplate(
            Guid id, int? tenantId, string templateKey, NotificationChannel channel, string language,
            string title, string body, string providerTemplateName = null)
        {
            Id = id;
            TenantId = tenantId;
            TemplateKey = templateKey;
            Channel = channel;
            // Normalize language so "en"/"EN" can't become two rows or miss lookups.
            Language = language?.Trim().ToLowerInvariant();
            Title = title;
            Body = body;
            ProviderTemplateName = providerTemplateName;
            // WhatsApp templates start as Draft (need Meta approval); others are usable as-is.
            ApprovalStatus = channel == NotificationChannel.WhatsApp
                ? TemplateApprovalStatus.Draft
                : TemplateApprovalStatus.NotApplicable;
            IsActive = true;
        }

        public void Update(string title, string body, string providerTemplateName, bool isActive)
        {
            Title = title;
            Body = body;
            ProviderTemplateName = providerTemplateName;
            IsActive = isActive;
            // A WhatsApp content edit needs Meta re-approval — drop back to Draft so
            // the COMM-07 provider never sends a stale "Approved" template.
            if (Channel == NotificationChannel.WhatsApp)
                ApprovalStatus = TemplateApprovalStatus.Draft;
        }

        public void SetApprovalStatus(TemplateApprovalStatus status)
        {
            ApprovalStatus = status;
        }
    }
}
