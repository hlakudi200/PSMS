namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// COMM-01: the delivery channels a notification can be dispatched on. In-app
    /// is the only one wired today; Email/Sms/Push/WhatsApp are added as channel
    /// providers in later Communication-epic tickets without changing call sites.
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
