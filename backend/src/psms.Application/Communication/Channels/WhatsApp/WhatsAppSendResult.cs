namespace psms.Communication.Channels.WhatsApp;

/// <summary>COMM-07: outcome of a WhatsApp gateway send.</summary>
public class WhatsAppSendResult
{
    public bool Success { get; set; }

    /// <summary>The provider/Meta message id (stored on the delivery log for webhook matching).</summary>
    public string ProviderMessageId { get; set; }

    public string Error { get; set; }

    /// <summary>True when no real gateway is configured (scaffold default) — distinct from a send failure.</summary>
    public bool NotConfigured { get; set; }

    public static WhatsAppSendResult Ok(string providerMessageId)
        => new WhatsAppSendResult { Success = true, ProviderMessageId = providerMessageId };

    public static WhatsAppSendResult Failed(string error)
        => new WhatsAppSendResult { Success = false, Error = error };

    public static WhatsAppSendResult NotConfiguredResult()
        => new WhatsAppSendResult { Success = false, NotConfigured = true, Error = "WhatsApp gateway is not configured." };
}
