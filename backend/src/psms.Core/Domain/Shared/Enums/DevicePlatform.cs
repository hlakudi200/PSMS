namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// COMM-04: the platform a registered push device token belongs to. Used by the
    /// push channel provider (COMM-10) to route a token to the right gateway.
    /// </summary>
    public enum DevicePlatform
    {
        Fcm = 1,
        Apns = 2,
        WebPush = 3
    }
}
