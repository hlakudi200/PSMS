using Castle.Core.Logging;
using System.Threading.Tasks;

namespace psms.Communication.Channels.Email;

/// <summary>
/// Sends through whichever email provider this deployment has been given.
/// <para>
/// Two are wired. SMTP wins when it is configured, because a school that has
/// filled in an SMTP account has said what it wants; Resend is the better one
/// to grow into — real delivery events, no daily cap, a sending address on the
/// school's own domain — but it will only deliver to arbitrary recipients from
/// a domain verified in its dashboard, which needs DNS access and a wait.
/// </para>
/// <para>
/// Neither configured behaves exactly as the COMM-09 scaffold did, reporting
/// NotConfigured rather than throwing, and says what to set — the delivery log
/// is where somebody debugging a silent channel actually looks.
/// </para>
/// </summary>
public class ConfiguredEmailGateway : IEmailGateway
{
    private readonly SmtpEmailGateway _smtp;
    private readonly ResendEmailGateway _resend;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public ConfiguredEmailGateway(SmtpEmailGateway smtp, ResendEmailGateway resend)
    {
        _smtp = smtp;
        _resend = resend;
    }

    /// <summary>
    /// Which provider a send would go through right now: <c>Smtp</c>,
    /// <c>Resend</c>, or <c>none</c>.
    /// <para>
    /// Worth asking, because precedence is invisible from the outside. A
    /// deployment whose SMTP settings cannot work — this host blocks outbound
    /// SMTP — keeps choosing SMTP until those settings are cleared, and
    /// "it still is not sending" after adding a Resend key is a confusing
    /// place to be without being told which one is being tried.
    /// </para>
    /// </summary>
    public string SelectedProvider =>
        _smtp.IsConfigured ? "Smtp" : _resend.IsConfigured ? "Resend" : "none";

    public Task<GatewaySendResult> SendAsync(string toEmail, string subject, string htmlBody)
    {
        if (_smtp.IsConfigured)
            return _smtp.SendAsync(toEmail, subject, htmlBody);

        if (_resend.IsConfigured)
            return _resend.SendAsync(toEmail, subject, htmlBody);

        /* Both halves named. Telling somebody only about the provider they did
           not choose is how "Email gateway is not configured" sent an operator
           looking at the key they had just set. */
        var message = "Email is not configured. Set either Smtp__Host, Smtp__UserName and "
                    + "Smtp__Password, or Resend__ApiKey and Resend__From.";

        Logger.Info($"Email send skipped — {message}");

        return Task.FromResult(new GatewaySendResult
        {
            NotConfigured = true,
            Success = false,
            Error = message,
        });
    }
}
