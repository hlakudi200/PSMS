using Abp.Dependency;
using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace psms.Admissions.ApplicationFees.Yoco;

/// <summary>What Yoco said when asked to start a checkout.</summary>
public class YocoCheckout
{
    /// <summary>Yoco's own id for the checkout, kept so a webhook can be matched to it.</summary>
    public string CheckoutId { get; set; }

    /// <summary>Where to send the payer. Null when it could not be created.</summary>
    public string RedirectUrl { get; set; }

    public bool Success => !string.IsNullOrWhiteSpace(RedirectUrl);

    public bool NotConfigured { get; set; }

    public string Error { get; set; }
}

/// <summary>
/// The application fee, over Yoco's hosted checkout.
/// <para>
/// One HTTPS call creates a checkout and hands back a URL to send the payer to;
/// Yoco takes the card details on their own page, so none ever reach this
/// application. Settlement arrives later as a signed webhook, because a payer
/// closing the tab after paying is ordinary and the redirect cannot be trusted
/// to happen.
/// </para>
/// <para>
/// Configuration (environment variables use the double underscore):
/// <c>Yoco:SecretKey</c> — <c>sk_test_…</c> or <c>sk_live_…</c>, and
/// <c>Yoco:WebhookSecret</c> — <c>whsec_…</c>, handed over once when the
/// webhook is registered. Without the first there is no checkout; without the
/// second no webhook is believed, which is deliberate.
/// </para>
/// </summary>
public class YocoPaymentGateway : ITransientDependency
{
    private const string CheckoutsUrl = "https://payments.yoco.com/api/checkouts";
    private const int TimeoutSeconds = 20;

    /// <summary>Shared: a client per call exhausts sockets.</summary>
    private static readonly HttpClient Http = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(TimeoutSeconds),
    };

    private readonly IConfiguration _configuration;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public YocoPaymentGateway(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string SecretKey => _configuration.GetSection("Yoco")["SecretKey"];
    private string WebhookSecret => _configuration.GetSection("Yoco")["WebhookSecret"];

    public bool IsConfigured => !string.IsNullOrWhiteSpace(SecretKey);

    /// <summary>Whether a webhook can be believed at all.</summary>
    public bool CanVerifyWebhooks => !string.IsNullOrWhiteSpace(WebhookSecret);

    /// <summary>
    /// Whether this is a test key. Worth knowing out loud: a school running on
    /// a test key takes no money and would not find out from the screens.
    /// </summary>
    public bool IsTestKey => SecretKey?.StartsWith("sk_test_", StringComparison.Ordinal) ?? false;

    /// <summary>
    /// Starts a checkout for one application fee.
    /// </summary>
    /// <param name="amount">In rands. Converted to cents, which is what Yoco takes.</param>
    public async Task<YocoCheckout> CreateCheckoutAsync(
        decimal amount,
        string reference,
        Guid applicationId,
        string successUrl,
        string cancelUrl,
        string failureUrl)
    {
        if (!IsConfigured)
            return new YocoCheckout
            {
                NotConfigured = true,
                Error = "Online payment is not configured: set Yoco__SecretKey.",
            };

        if (amount <= 0)
            return new YocoCheckout { Error = "There is nothing to pay." };

        /* Cents, and rounded rather than truncated: 199.995 is 200.00 to a
           parent and R199.99 would be a cent the school never asked for.
           MidpointRounding.AwayFromZero because money rounds up at the half. */
        var cents = (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);

        var payload = new
        {
            amount = cents,
            currency = "ZAR",
            successUrl,
            cancelUrl,
            failureUrl,
            /* Carried back on the webhook. The application id is what lets a
               payment be matched to the thing it paid for without trusting
               anything the payer could have edited in the URL. */
            metadata = new
            {
                applicationId = applicationId.ToString(),
                reference,
            },
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, CheckoutsUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", SecretKey);

            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
            using var response = await Http.SendAsync(request, cancellation.Token);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var error = Describe(response.StatusCode, body);
                Logger.Warn($"Yoco refused a checkout for {reference}: {error}");
                return new YocoCheckout { Error = error };
            }

            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            var redirect = root.TryGetProperty("redirectUrl", out var r) ? r.GetString() : null;
            var id = root.TryGetProperty("id", out var i) ? i.GetString() : null;

            if (string.IsNullOrWhiteSpace(redirect))
            {
                /* Answered 2xx without somewhere to send the payer. Treated as a
                   failure rather than guessed at: sending somebody nowhere is
                   worse than telling them it did not work. */
                Logger.Warn($"Yoco accepted the checkout for {reference} and returned no redirect URL.");
                return new YocoCheckout { Error = "The payment service did not say where to send you." };
            }

            Logger.Info($"Yoco checkout {id} created for {reference}.");
            return new YocoCheckout { CheckoutId = id, RedirectUrl = redirect };
        }
        catch (OperationCanceledException)
        {
            Logger.Warn($"Yoco did not answer within {TimeoutSeconds}s for {reference}.");
            return new YocoCheckout { Error = $"The payment service did not answer within {TimeoutSeconds} seconds." };
        }
        catch (HttpRequestException ex)
        {
            Logger.Warn($"Could not reach Yoco for {reference}: {ex.Message}");
            return new YocoCheckout { Error = $"Could not reach the payment service: {ex.Message}" };
        }
        catch (JsonException ex)
        {
            Logger.Warn($"Could not read Yoco's answer for {reference}: {ex.Message}");
            return new YocoCheckout { Error = "The payment service answered with something unreadable." };
        }
    }

    /// <summary>
    /// Whether a webhook really came from Yoco.
    /// <para>
    /// The single most security-critical thing here. An endpoint that marks an
    /// application paid without checking this is a way to buy a place at the
    /// school for nothing, from anywhere on the internet, by posting a JSON
    /// body at it.
    /// </para>
    /// <para>
    /// Standard Webhooks: the signed content is <c>{id}.{timestamp}.{body}</c>,
    /// HMAC-SHA256 with the secret (base64, after its <c>whsec_</c> prefix),
    /// compared in constant time. The header may carry several space-separated
    /// versioned signatures and any one matching is enough, because that is how
    /// a secret gets rotated without dropping deliveries.
    /// </para>
    /// </summary>
    public bool IsSignedByYoco(string webhookId, string timestamp, string signatureHeader, string body)
    {
        if (!CanVerifyWebhooks)
        {
            Logger.Warn("A Yoco webhook arrived and cannot be verified — set Yoco__WebhookSecret.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(webhookId)
            || string.IsNullOrWhiteSpace(timestamp)
            || string.IsNullOrWhiteSpace(signatureHeader))
            return false;

        if (!WithinTolerance(timestamp))
        {
            // A replayed delivery from last week is not a payment made today.
            Logger.Warn($"A Yoco webhook arrived with a timestamp outside tolerance: {timestamp}.");
            return false;
        }

        byte[] key;
        try
        {
            var secret = WebhookSecret;
            if (secret.StartsWith("whsec_", StringComparison.Ordinal)) secret = secret.Substring("whsec_".Length);
            key = Convert.FromBase64String(secret);
        }
        catch (FormatException)
        {
            Logger.Error("Yoco__WebhookSecret is not valid base64, so no webhook can be verified.");
            return false;
        }

        using var hmac = new HMACSHA256(key);
        var expected = Convert.ToBase64String(
            hmac.ComputeHash(Encoding.UTF8.GetBytes($"{webhookId}.{timestamp}.{body}")));

        foreach (var candidate in signatureHeader.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            // "v1,<base64>" — the version prefix is not part of the signature.
            var comma = candidate.IndexOf(',');
            var value = comma >= 0 ? candidate.Substring(comma + 1) : candidate;

            if (FixedTimeEquals(value, expected)) return true;
        }

        return false;
    }

    /// <summary>
    /// Five minutes either way. Without this a signature stays valid for ever
    /// and one captured delivery can be replayed at will.
    /// </summary>
    private static bool WithinTolerance(string timestamp)
    {
        if (!long.TryParse(timestamp, out var seconds)) return false;

        var sent = DateTimeOffset.FromUnixTimeSeconds(seconds);
        return (DateTimeOffset.UtcNow - sent).Duration() <= TimeSpan.FromMinutes(5);
    }

    /// <summary>Compared in constant time: a fast "no" tells an attacker how close they were.</summary>
    private static bool FixedTimeEquals(string a, string b)
    {
        var left = Encoding.UTF8.GetBytes(a ?? string.Empty);
        var right = Encoding.UTF8.GetBytes(b ?? string.Empty);

        return left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
    }

    private static string Describe(System.Net.HttpStatusCode status, string body)
    {
        var message = ReadMessage(body) ?? body?.Trim();

        return status switch
        {
            System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden =>
                "The payment service rejected the key. Check Yoco__SecretKey.",
            System.Net.HttpStatusCode.TooManyRequests =>
                "The payment service is rate limiting us. Try again in a moment.",
            _ => string.IsNullOrWhiteSpace(message)
                ? $"The payment service answered {(int)status}."
                : $"{message} ({(int)status})",
        };
    }

    private static string ReadMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        try
        {
            using var document = JsonDocument.Parse(body);
            foreach (var name in new[] { "message", "displayMessage", "description", "error" })
                if (document.RootElement.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                    return value.GetString();
        }
        catch (JsonException) { }

        return null;
    }
}
