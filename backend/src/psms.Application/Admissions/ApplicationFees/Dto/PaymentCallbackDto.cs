using System.ComponentModel.DataAnnotations;

namespace psms.Admissions.ApplicationFees.Dto;

/// <summary>
/// DTO for payment gateway callback/webhook.
/// Used by payment gateways like PayFast, Ozow, etc.
/// </summary>
public class PaymentCallbackDto
{
    /// <summary>
    /// Payment reference from our system.
    /// </summary>
    [Required]
    public string MerchantReference { get; set; }

    /// <summary>
    /// Payment reference from the gateway.
    /// </summary>
    public string GatewayReference { get; set; }

    /// <summary>
    /// Payment status from the gateway.
    /// </summary>
    [Required]
    public string Status { get; set; }

    /// <summary>
    /// Amount paid.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment method used (from gateway).
    /// </summary>
    public string PaymentMethod { get; set; }

    /// <summary>
    /// Signature for validation.
    /// </summary>
    public string Signature { get; set; }

    /// <summary>
    /// Raw payload for logging.
    /// </summary>
    public string RawPayload { get; set; }

    /// <summary>
    /// Gateway name (PayFast, Ozow, etc.).
    /// </summary>
    public string Gateway { get; set; }

    /// <summary>
    /// Transaction date from gateway.
    /// </summary>
    public string TransactionDate { get; set; }

    /// <summary>
    /// Error message if payment failed.
    /// </summary>
    public string ErrorMessage { get; set; }
}
