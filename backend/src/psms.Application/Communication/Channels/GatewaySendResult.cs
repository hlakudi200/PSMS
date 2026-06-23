namespace psms.Communication.Channels;

/// <summary>
/// COMM-08/09/10: shared outcome shape for the SMS/Email/Push gateways. NotConfigured
/// is the scaffold default (no real provider wired) — distinct from a send failure.
/// </summary>
public class GatewaySendResult
{
    public bool Success { get; set; }

    /// <summary>Provider message id (stored on the delivery log for webhook matching).</summary>
    public string ProviderMessageId { get; set; }

    public string Error { get; set; }

    public bool NotConfigured { get; set; }

    public static GatewaySendResult Ok(string providerMessageId)
        => new GatewaySendResult { Success = true, ProviderMessageId = providerMessageId };

    public static GatewaySendResult Failed(string error)
        => new GatewaySendResult { Success = false, Error = error };

    public static GatewaySendResult NotConfiguredResult(string channel)
        => new GatewaySendResult { Success = false, NotConfigured = true, Error = $"{channel} gateway is not configured." };
}
