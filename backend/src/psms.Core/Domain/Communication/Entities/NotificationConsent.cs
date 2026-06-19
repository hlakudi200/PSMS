using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Communication.Entities
{
    /// <summary>
    /// COMM-05: a user's consent for an EXTERNAL channel (the POPIA opt-in record).
    /// External channels (WhatsApp/SMS/Email/Push) require an explicit granted
    /// consent before the dispatcher will use them; in-app needs no consent. The
    /// grant/revoke timestamps + source give the auditable trail POPIA expects.
    /// </summary>
    [Table("NotificationConsents")]
    public class NotificationConsent : CreationAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxSourceLength = 200;

        public int? TenantId { get; set; }

        public long UserId { get; set; }

        public NotificationChannel Channel { get; set; }

        public bool IsGranted { get; set; }

        public DateTime? GrantedDate { get; set; }

        public DateTime? RevokedDate { get; set; }

        /// <summary>Where the opt-in/out came from (e.g. "preferences-screen", "whatsapp-STOP").</summary>
        [StringLength(MaxSourceLength)]
        public string Source { get; set; }

        protected NotificationConsent()
        {
        }

        public NotificationConsent(Guid id, int? tenantId, long userId, NotificationChannel channel)
        {
            Id = id;
            TenantId = tenantId;
            UserId = userId;
            Channel = channel;
            IsGranted = false;
        }

        public void Grant(string source)
        {
            IsGranted = true;
            GrantedDate = DateTime.UtcNow;
            RevokedDate = null;
            Source = source;
        }

        public void Revoke(string source)
        {
            IsGranted = false;
            RevokedDate = DateTime.UtcNow;
            Source = source;
        }
    }
}
