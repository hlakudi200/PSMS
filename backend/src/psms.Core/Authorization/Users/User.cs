using Abp.Authorization.Users;
using Abp.Extensions;
using System;
using System.Collections.Generic;

namespace psms.Authorization.Users;

public class User : AbpUser<User>
{
    public const string DefaultPassword = "123qwe";

    /// <summary>
    /// COMM-04: WhatsApp number (E.164) for the WhatsApp channel. Distinct from
    /// AbpUser.PhoneNumber (the SMS/voice MSISDN) because WhatsApp needs its own
    /// opt-in and may differ. Null when the user has no WhatsApp contact.
    /// </summary>
    public virtual string WhatsAppNumber { get; set; }

    public static string CreateRandomPassword()
    {
        return Guid.NewGuid().ToString("N").Truncate(16);
    }

    public static User CreateTenantAdminUser(int tenantId, string emailAddress)
    {
        var user = new User
        {
            TenantId = tenantId,
            UserName = AdminUserName,
            Name = AdminUserName,
            Surname = AdminUserName,
            EmailAddress = emailAddress,
            Roles = new List<UserRole>()
        };

        user.SetNormalizedNames();

        return user;
    }
}
