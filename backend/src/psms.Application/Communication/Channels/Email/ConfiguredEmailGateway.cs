using Castle.Core.Logging;
using System.Threading.Tasks;

namespace psms.Communication.Channels.Email;

/// <summary>
/// Sends through whichever email provider this deployment has been given.
/// <para>
/// Two are wired, and <b>the HTTP API is preferred</b>. SMTP used to win on the
/// grounds that a school filling it in had said what it wanted — which was
/// wrong here in the way that matters: this host blocks outbound SMTP, as most
/// managed platforms do to deter spam, so the preference meant choosing a
/// provider that could not work while a working one sat beside it. Every
/// admissions email for days failed with "the mail server did not answer
/// within 20 seconds".
/// </para>
/// <para>
/// The API reaches out over 443 and works from anywhere, so a deployment with
/// both configured almost certainly wants it. SMTP remains the fallback for a
/// school that has only a mail account, and nothing about the choice is
/// hidden: the diagnostic reports which provider a send would use.
/// </para>
/// <para>
/// Neither configured behaves exactly as the COMM-09 scaffold did, reporting
/// NotConfigured rather than throwing, and says what to set — the delivery log
/// is where somebody debugging a silent channel actually looks.
/// </para>
/// </summary>
/// <summary>One provider's readiness, as the diagnostic reports it.</summary>
public class EmailProviderReadiness
{
    /// <summary><c>Resend</c> or <c>Smtp</c>.</summary>
    public string Name { get; set; }

    /// <summary>True when everything it needs has been set.</summary>
    public bool IsConfigured { get; set; }

    /// <summary>
    /// The settings it is still waiting for, by name — never a value.
    /// </summary>
    public string[] MissingSettings { get; set; }

    /// <summary>True for the one a send would actually go through.</summary>
    public bool WouldBeUsed { get; set; }
}

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
        _resend.IsConfigured ? "Resend" : _smtp.IsConfigured ? "Smtp" : "none";

    /// <summary>
    /// Both providers and what each is still waiting for, so the chosen one
    /// can be told apart from the only one available.
    /// <para>
    /// <c>SelectedProvider</c> alone cannot answer the question an operator
    /// actually has. Seeing <c>Smtp</c> after setting a Resend key means
    /// either the key did not take or the build predates the preference
    /// change, and from outside those look identical — which cost a morning.
    /// Reporting both, with the names of the blank settings, separates them.
    /// </para>
    /// <para>
    /// Setting names only. No value from either provider's configuration is
    /// returned: this travels over the wire to whoever can open the admin
    /// screen, and an API key is not theirs to read.
    /// </para>
    /// </summary>
    public EmailProviderReadiness[] Providers => new[]
    {
        new EmailProviderReadiness
        {
            Name = "Resend",
            IsConfigured = _resend.IsConfigured,
            MissingSettings = _resend.MissingSettings,
            WouldBeUsed = _resend.IsConfigured,
        },
        new EmailProviderReadiness
        {
            Name = "Smtp",
            IsConfigured = _smtp.IsConfigured,
            MissingSettings = _smtp.MissingSettings,
            WouldBeUsed = !_resend.IsConfigured && _smtp.IsConfigured,
        },
    };

    public Task<GatewaySendResult> SendAsync(string toEmail, string subject, string htmlBody)
    {
        if (_resend.IsConfigured)
            return _resend.SendAsync(toEmail, subject, htmlBody);

        if (_smtp.IsConfigured)
            return _smtp.SendAsync(toEmail, subject, htmlBody);

        /* Both halves named. Telling somebody only about the provider they did
           not choose is how "Email gateway is not configured" sent an operator
           looking at the key they had just set. */
        var message = "Email is not configured. Set either Resend__ApiKey and Resend__From, or "
                    + "Smtp__Host, Smtp__UserName and Smtp__Password.";

        Logger.Info($"Email send skipped — {message}");

        return Task.FromResult(new GatewaySendResult
        {
            NotConfigured = true,
            Success = false,
            Error = message,
        });
    }
}
