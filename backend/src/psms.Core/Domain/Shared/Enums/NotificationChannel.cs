namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// COMM-01: the delivery channels a notification can be dispatched on. Each has a
    /// channel provider; the dispatcher's routing policy (COMM-11) decides which are
    /// used per request. In-app is the inbox of record (always, consent-exempt).
    /// </summary>
    public enum NotificationChannel
    {
        InApp = 1,
        Email = 2,
        Sms = 3,
        Push = 4,
        WhatsApp = 5
    }
}
