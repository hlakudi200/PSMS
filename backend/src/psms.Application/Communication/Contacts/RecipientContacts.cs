using psms.Domain.Shared.Enums;
using System.Collections.Generic;

namespace psms.Communication.Contacts;

/// <summary>
/// COMM-04: the reachable addresses for a recipient — what the dispatcher/channel
/// providers (COMM-07+) need to know "where can I reach this person?".
/// </summary>
public class RecipientContacts
{
    public long UserId { get; set; }
    public string Email { get; set; }
    public string MobileNumber { get; set; }
    public string WhatsAppNumber { get; set; }
    public List<RecipientDeviceToken> DeviceTokens { get; set; } = new List<RecipientDeviceToken>();

    public bool HasEmail => !string.IsNullOrWhiteSpace(Email);
    public bool HasMobile => !string.IsNullOrWhiteSpace(MobileNumber);
    public bool HasWhatsApp => !string.IsNullOrWhiteSpace(WhatsAppNumber);
    public bool HasDevices => DeviceTokens != null && DeviceTokens.Count > 0;
}

public class RecipientDeviceToken
{
    public string Token { get; set; }
    public DevicePlatform Platform { get; set; }
}
