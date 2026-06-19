using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.Communication.Entities
{
    /// <summary>
    /// COMM-04: a push device token registered to a user. A user can have several
    /// (phone, tablet, browser). The push channel provider (COMM-10) sends to the
    /// active tokens and deactivates ones the gateway rejects.
    /// </summary>
    [Table("UserDeviceTokens")]
    public class UserDeviceToken : CreationAuditedEntity<Guid>, IMayHaveTenant
    {
        public const int MaxTokenLength = 512;
        public const int MaxDeviceNameLength = 200;

        public int? TenantId { get; set; }

        public long UserId { get; set; }

        [Required]
        [StringLength(MaxTokenLength)]
        public string Token { get; set; }

        public DevicePlatform Platform { get; set; }

        [StringLength(MaxDeviceNameLength)]
        public string DeviceName { get; set; }

        public bool IsActive { get; set; }

        public DateTime LastSeenDate { get; set; }

        protected UserDeviceToken()
        {
        }

        public UserDeviceToken(Guid id, int? tenantId, long userId, string token, DevicePlatform platform, string deviceName = null)
        {
            Id = id;
            TenantId = tenantId;
            UserId = userId;
            Token = token;
            Platform = platform;
            DeviceName = deviceName;
            IsActive = true;
            LastSeenDate = DateTime.UtcNow;
        }

        /// <summary>Re-activate / refresh an existing token (e.g. the client re-registered it).</summary>
        public void Touch(DevicePlatform platform, string deviceName = null)
        {
            Platform = platform;
            if (deviceName != null) DeviceName = deviceName;
            IsActive = true;
            LastSeenDate = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
