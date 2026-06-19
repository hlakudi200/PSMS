using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Communication.Entities
{
    /// <summary>
    /// COMM-05: a user's per-channel, per-category preference override. Absence of a
    /// row means the default (enabled), so this table only holds explicit opt-outs
    /// (or re-enables). The dispatcher skips a channel for a category the user has
    /// disabled. Distinct from <see cref="NotificationConsent"/> (the legal opt-in):
    /// a category toggle is a UX choice, not a consent withdrawal.
    /// </summary>
    [Table("NotificationPreferences")]
    public class NotificationPreference : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public long UserId { get; set; }

        public NotificationChannel Channel { get; set; }

        /// <summary>The notification category this preference applies to.</summary>
        public NotificationType Category { get; set; }

        public bool IsEnabled { get; set; }

        protected NotificationPreference()
        {
        }

        public NotificationPreference(Guid id, int? tenantId, long userId, NotificationChannel channel, NotificationType category, bool isEnabled)
        {
            Id = id;
            TenantId = tenantId;
            UserId = userId;
            Channel = channel;
            Category = category;
            IsEnabled = isEnabled;
        }

        public void SetEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }
    }
}
