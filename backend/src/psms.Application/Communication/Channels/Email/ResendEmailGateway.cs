using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace psms.Communication.Channels.Email;

/// <summary>
/// COMM-09: email over Resend (https://resend.com), replacing the scaffold's
/// "not configured" default.
/// <para>
/// One HTTPS POST per message. Resend wants an API key and a <c>from</c> address
/// on a domain verified in the Resend dashboard — without a verified domain it
/// will only deliver to the account holder's own address, which is fine for a
/// smoke test and no use for emailing parents.
/// </para>
/// <para>
/// Configuration (environment variables on Render use the double underscore):
/// <c>Resend:ApiKey</c>, <c>Resend:From</c>, optional <c>Resend:ReplyTo</c>.
/// With no key set this behaves exactly as the scaffold did — it reports
/// NotConfigured rather than throwing — so the same build runs in an
/// environment that has no email and one that does.
/// </para>
/// </summary>
public class ResendEmailGateway : IEmailGateway
{
    private const string SendUrl = "https://api.resend.com/emails";

    /// <summary>
    /// Shared on purpose: a client per send exhausts sockets. Matches how
    /// <c>SupabaseStorageService</c> and <c>LiveKitTokenService</c> hold theirs.
    /// </summary>
    private static readonly HttpClient Http = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(20),
    };

    /// <summary>
    /// Resend allows two requests a second on the lower plans, and a fan-out to
    /// a grade's parents is a few hundred sequential sends. Pacing them here
    /// costs a quarter-second each and turns a wall of 429s into a slow send.
    /// </summary>
    private static readonly SemaphoreSlim Pace = new SemaphoreSlim(1, 1);
    private static readonly TimeSpan MinimumGap = TimeSpan.FromMilliseconds(550);
    private static DateTime _lastSentUtc = DateTime.MinValue;

    private readonly IConfiguration _configuration;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public ResendEmailGateway(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private (string ApiKey, string From, string ReplyTo) GetConfig()
    {
        var section = _configuration.GetSection("Resend");
        return (section["ApiKey"], section["From"], section["ReplyTo"]);
    }

    public bool IsConfigured
    {
        get
        {
            var (apiKey, from, _) = GetConfig();
            return !string.IsNullOrWhiteSpace(apiKey) && !string.IsNullOrWhiteSpace(from);
        }
    }

    public async Task<GatewaySendResult> SendAsync(string toEmail, string subject, string htmlBody)
    {
        var (apiKey, from, replyTo) = GetConfig();

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(from))
        {
            Logger.Info("Email send skipped — set Resend__ApiKey and Resend__From to enable it.");
            return GatewaySendResult.NotConfiguredResult("Email");
        }

        if (string.IsNullOrWhiteSpace(toEmail))
            return GatewaySendResult.Failed("No email address to send to.");

        if (string.IsNullOrWhiteSpace(htmlBody))
            return GatewaySendResult.Failed("No email content to send.");

        var payload = new
        {
            from,
            to = new[] { toEmail.Trim() },
            subject = string.IsNullOrWhiteSpace(subject) ? "(no subject)" : subject.Trim(),
            html = htmlBody,
            reply_to = string.IsNullOrWhiteSpace(replyTo) ? null : replyTo.Trim(),
        };

        try
        {
            using var response = await PostPacedAsync(apiKey, payload);
            var body = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var id = ReadMessageId(body);
                Logger.Info($"Email accepted by Resend for {Mask(toEmail)} (id {id ?? "unknown"}).");
                return GatewaySendResult.Ok(id);
            }

            var error = Describe(response.StatusCode, body);
            // The address, never the key, and never the message body.
            Logger.Warn($"Resend refused the email to {Mask(toEmail)}: {error}");
            return GatewaySendResult.Failed(error);
        }
        catch (TaskCanceledException)
        {
            // Includes the HttpClient timeout. The dispatch job may retry.
            Logger.Warn($"Resend did not answer in time for {Mask(toEmail)}.");
            return GatewaySendResult.Failed("The email service did not answer in time.");
        }
        catch (HttpRequestException ex)
        {
            Logger.Warn($"Could not reach Resend for {Mask(toEmail)}: {ex.Message}");
            return GatewaySendResult.Failed($"Could not reach the email service: {ex.Message}");
        }
    }

    private static async Task<HttpResponseMessage> PostPacedAsync(string apiKey, object payload)
    {
        await Pace.WaitAsync();
        try
        {
            var since = DateTime.UtcNow - _lastSentUtc;
            if (since < MinimumGap)
                await Task.Delay(MinimumGap - since);

            using var request = new HttpRequestMessage(HttpMethod.Post, SendUrl)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload, new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    }),
                    Encoding.UTF8,
                    "application/json"),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await Http.SendAsync(request);
            _lastSentUtc = DateTime.UtcNow;
            return response;
        }
        finally
        {
            Pace.Release();
        }
    }

    /// <summary>Resend answers a send with <c>{"id":"..."}</c>.</summary>
    private static string ReadMessageId(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.TryGetProperty("id", out var id) ? id.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Turns a refusal into something an administrator can act on. Resend's
    /// errors arrive as <c>{"statusCode":422,"name":"...","message":"..."}</c>,
    /// and the two that actually happen in practice deserve naming.
    /// </summary>
    private static string Describe(HttpStatusCode status, string body)
    {
        var message = ReadErrorMessage(body) ?? body?.Trim();

        return status switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                "The email service rejected the API key. Check Resend__ApiKey.",
            HttpStatusCode.TooManyRequests =>
                "The email service is rate limiting us. The message was not sent.",
            HttpStatusCode.UnprocessableEntity when message != null && message.Contains("domain", StringComparison.OrdinalIgnoreCase) =>
                $"{message} Verify the sending domain in Resend, or set Resend__From to an address on a domain you have verified.",
            _ => string.IsNullOrWhiteSpace(message)
                ? $"The email service answered {(int)status}."
                : $"{message} ({(int)status})",
        };
    }

    private static string ReadErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;
        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.TryGetProperty("message", out var message)
                ? message.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Addresses end up in the application log, which is read by more people
    /// than the mailbox is. Enough to identify the send, not enough to harvest.
    /// </summary>
    public static string Mask(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return "(none)";

        var at = email.IndexOf('@');
        if (at <= 0) return "***";

        var name = email.Substring(0, at);
        var domain = email.Substring(at);
        var head = name.Length <= 2 ? name.Substring(0, 1) : name.Substring(0, 2);
        return $"{head}***{domain}";
    }
}
