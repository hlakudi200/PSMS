using Abp.Application.Services;
using Abp.Authorization;
using psms.Authorization;
using System.Threading.Tasks;

namespace psms.Admissions.ApplicationFees.Yoco;

/// <summary>What the payment channel is, and whether it answers.</summary>
public class PaymentChannelStatus
{
    /// <summary>Simulated, Yoco, or OfflineOnly.</summary>
    public string Mode { get; set; }

    /// <summary>A secret key is present.</summary>
    public bool KeyConfigured { get; set; }

    /// <summary>
    /// It is a test key. Said out loud because a school running live on one
    /// takes no money and would never find out from the screens.
    /// </summary>
    public bool TestKey { get; set; }

    /// <summary>
    /// A webhook secret is present. Without it every webhook is refused, so
    /// payments would be taken and never settled.
    /// </summary>
    public bool WebhookSecretConfigured { get; set; }

    /// <summary>Where Yoco should be told to post.</summary>
    public string WebhookUrl { get; set; }

    /// <summary>Whether a real checkout could be created just now.</summary>
    public bool Reachable { get; set; }

    public string Error { get; set; }

    /// <summary>The one thing most worth doing next, or null when there is nothing.</summary>
    public string NextStep { get; set; }
}

/// <summary>
/// Answers "can this school take a payment" in one call.
/// <para>
/// The alternative, and what the email channel cost before it got one of
/// these, is registering an applicant, filling in an application, submitting
/// it and then reading a log — ten minutes a guess, on live data, leaving a
/// test family behind each time.
/// </para>
/// <para>
/// It creates a real checkout and does not pay it. Nothing is charged by
/// asking and an abandoned checkout costs nothing — but it proves the key, the
/// network and the answer shape all the way through, which is what reading
/// configuration back cannot do.
/// </para>
/// </summary>
[AbpAuthorize(PermissionNames.Admissions_Settings_Manage)]
public class PaymentChannelDiagnosticsService : ApplicationService
{
    /// <summary>
    /// Yoco's own floor is R2.00, and a probe below it comes back as a refusal
    /// that reads like a broken integration. Found exactly that way.
    /// </summary>
    private const decimal ProbeAmount = 2m;

    private readonly YocoPaymentGateway _yoco;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

    public PaymentChannelDiagnosticsService(
        YocoPaymentGateway yoco,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _yoco = yoco;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Where this deployment actually answers, taken from the request that is
    /// asking rather than from configuration.
    /// <para>
    /// App:ServerRootAddress is a development value on the deployed server, so
    /// reading it printed <c>https://localhost:44311/api/yoco/webhook</c> — an
    /// address nobody could register, handed over as the thing to register.
    /// </para>
    /// <para>
    /// The scheme comes from <c>X-Forwarded-Proto</c> where there is one. TLS
    /// is terminated at the host's proxy and the request arrives as plain HTTP,
    /// so <c>Request.Scheme</c> alone said <c>http://</c> — a URL Yoco would
    /// refuse, offered as the one to register.
    /// </para>
    /// </summary>
    private string ServerRoot()
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        if (request != null && request.Host.HasValue)
        {
            var forwarded = request.Headers["X-Forwarded-Proto"].ToString();
            var scheme = string.IsNullOrWhiteSpace(forwarded)
                ? request.Scheme
                // The header may list several; the first is the client's.
                : forwarded.Split(',')[0].Trim();

            return $"{scheme}://{request.Host.Value}";
        }

        return _configuration["App:ServerRootAddress"]?.TrimEnd('/') ?? string.Empty;
    }

    public async Task<PaymentChannelStatus> CheckAsync()
    {
        var root = ServerRoot();

        var status = new PaymentChannelStatus
        {
            Mode = _yoco.IsConfigured ? nameof(PaymentGatewayMode.Yoco) : nameof(PaymentGatewayMode.Simulated),
            KeyConfigured = _yoco.IsConfigured,
            TestKey = _yoco.IsTestKey,
            WebhookSecretConfigured = _yoco.CanVerifyWebhooks,
            WebhookUrl = $"{root}/api/yoco/webhook",
        };

        if (!_yoco.IsConfigured)
        {
            status.NextStep = "Set Yoco__SecretKey. Until then the fee step is simulated and takes no money.";
            return status;
        }

        var checkout = await _yoco.CreateCheckoutAsync(
            amount: ProbeAmount,
            reference: "PSMS-CHANNEL-CHECK",
            applicationId: System.Guid.Empty,
            successUrl: $"{root}/api/yoco/check",
            cancelUrl: $"{root}/api/yoco/check",
            failureUrl: $"{root}/api/yoco/check");

        status.Reachable = checkout.Success;
        status.Error = checkout.Error;

        if (!checkout.Success)
            status.NextStep = "The payment service refused or could not be reached — see the error.";
        else if (!_yoco.CanVerifyWebhooks)
            status.NextStep =
                "Set Yoco__WebhookSecret, and register the webhook URL above with Yoco. Without it every "
                + "webhook is refused, so a parent would be charged and the fee never marked paid.";
        else if (_yoco.IsTestKey)
            status.NextStep =
                "Working, on a TEST key — no money will move. Swap to the live key when the school is ready.";

        return status;
    }

    /// <summary>
    /// What the gateway says about one checkout, in its own words.
    /// <para>
    /// The webhook is not the only way a payment can be confirmed, and the
    /// retrieve call had never been made against a real checkout — the status
    /// vocabulary came from documentation. This reports the raw word, so the
    /// first real use proves it rather than assuming it.
    /// </para>
    /// <para>
    /// Reads only. It settles nothing; ApplicationFee/ConfirmPayment is what
    /// acts on the answer.
    /// </para>
    /// </summary>
    public async Task<CheckoutProbe> ProbeCheckoutAsync(string checkoutId)
    {
        var state = await _yoco.GetCheckoutAsync(checkoutId);

        return new CheckoutProbe
        {
            CheckoutId = checkoutId,
            Found = state.Found,
            Status = state.Status,
            HasPaymentId = !string.IsNullOrWhiteSpace(state.PaymentId),
            ReadAsPaid = state.IsPaid,
            Error = state.Error,
        };
    }
}

/// <summary>What the gateway reports about one checkout, for diagnosis only.</summary>
public class CheckoutProbe
{
    public string CheckoutId { get; set; }

    /// <summary>Whether the gateway answered about it at all.</summary>
    public bool Found { get; set; }

    /// <summary>The gateway's own word, carried verbatim.</summary>
    public string Status { get; set; }

    /// <summary>
    /// Whether a payment id came back — never the id itself, which identifies
    /// a real transaction and this result travels over the wire.
    /// </summary>
    public bool HasPaymentId { get; set; }

    /// <summary>What ConfirmPayment would make of it.</summary>
    public bool ReadAsPaid { get; set; }

    public string Error { get; set; }
}
