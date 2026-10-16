using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace psms.Communication.Channels.Email;

/// <summary>
/// COMM-09: email over plain SMTP — Gmail, Workspace, or whatever the school's
/// host provides.
/// <para>
/// Wired alongside Resend because it needs nothing but an account the school
/// already has. Resend will only deliver to arbitrary recipients from a domain
/// verified in its dashboard, and verifying a domain means DNS access and a
/// wait; a Gmail app password works in five minutes. The trade is that the
/// address parents see is the Gmail one, not <c>admissions@theschool.co.za</c>.
/// </para>
/// <para>
/// Configuration (environment variables on Render use the double underscore):
/// <c>Smtp:Host</c>, <c>Smtp:UserName</c>, <c>Smtp:Password</c>, and optionally
/// <c>Smtp:Port</c> (587), <c>Smtp:From</c> (defaults to the user name) and
/// <c>Smtp:FromName</c>.
/// </para>
/// <para>
/// For Gmail the password must be an <b>app password</b> — Google stopped
/// accepting account passwords from applications in 2022, and generating one
/// requires 2-Step Verification to be on. Gmail also rewrites or rejects a
/// From address that is not the authenticated account, so there is no point
/// setting <c>Smtp:From</c> to anything else.
/// </para>
/// </summary>
public class SmtpEmailGateway : IEmailGateway
{
    private readonly IConfiguration _configuration;

    /// <summary>Long enough for a slow relay, short enough not to look hung.</summary>
    private const int SendTimeoutSeconds = 20;

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public SmtpEmailGateway(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private (string Host, int Port, string UserName, string Password, string From, string FromName, bool Ssl) GetConfig()
    {
        var section = _configuration.GetSection("Smtp");
        var userName = section["UserName"];

        return (
            Host: section["Host"],
            Port: int.TryParse(section["Port"], out var port) ? port : 587,
            UserName: userName,
            Password: section["Password"],
            // Gmail will not let the From differ from the account anyway.
            From: string.IsNullOrWhiteSpace(section["From"]) ? userName : section["From"],
            FromName: section["FromName"],
            // STARTTLS on 587. Off only if a school's own server cannot do it.
            Ssl: !string.Equals(section["EnableSsl"], "false", StringComparison.OrdinalIgnoreCase));
    }

    public bool IsConfigured
    {
        get
        {
            var (host, _, userName, password, _, _, _) = GetConfig();
            return !string.IsNullOrWhiteSpace(host)
                && !string.IsNullOrWhiteSpace(userName)
                && !string.IsNullOrWhiteSpace(password);
        }
    }

    /// <summary>
    /// The settings this provider still needs, by the name they are set under.
    /// Empty when it is ready. Names only, never a value — see the same
    /// property on <see cref="ResendEmailGateway"/>.
    /// </summary>
    public string[] MissingSettings
    {
        get
        {
            var (host, _, userName, password, _, _, _) = GetConfig();
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(host)) missing.Add("Smtp__Host");
            if (string.IsNullOrWhiteSpace(userName)) missing.Add("Smtp__UserName");
            if (string.IsNullOrWhiteSpace(password)) missing.Add("Smtp__Password");
            return missing.ToArray();
        }
    }

    public async Task<GatewaySendResult> SendAsync(string toEmail, string subject, string htmlBody)
    {
        var (host, port, userName, password, from, fromName, ssl) = GetConfig();

        if (!IsConfigured)
            return new GatewaySendResult
            {
                NotConfigured = true,
                Success = false,
                Error = "SMTP is not configured: set Smtp__Host, Smtp__UserName and Smtp__Password.",
            };

        if (string.IsNullOrWhiteSpace(toEmail))
            return GatewaySendResult.Failed("No email address to send to.");

        if (string.IsNullOrWhiteSpace(htmlBody))
            return GatewaySendResult.Failed("No email content to send.");

        try
        {
            /* A client per send. SmtpClient is not thread-safe and the
               dispatcher sends sequentially, so there is nothing to gain from
               holding one open and a race to lose. */
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = ssl,
                Credentials = new NetworkCredential(userName, password),
                // Applies to the synchronous Send only. Kept because it costs
                // nothing; the cancellation below is what actually bounds this.
                Timeout = SendTimeoutSeconds * 1000,
            };

            using var message = new MailMessage
            {
                From = string.IsNullOrWhiteSpace(fromName)
                    ? new MailAddress(from)
                    : new MailAddress(from, fromName),
                Subject = string.IsNullOrWhiteSpace(subject) ? "(no subject)" : subject.Trim(),
                Body = htmlBody,
                IsBodyHtml = true,
            };
            message.To.Add(toEmail.Trim());

            /* SmtpClient.Timeout is NOT honoured by SendMailAsync — it bounds
               the synchronous Send and nothing else. A host that silently drops
               outbound SMTP (many block it to deter spam) leaves the connect
               hanging with no timeout at all, and this is awaited inside the
               workflow transaction: a principal pressed Approve and the request
               never came back, so the decision rolled back and the application
               stayed under review. Cancellation is the only thing that bounds
               it. */
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(SendTimeoutSeconds));
            await client.SendMailAsync(message, cancellation.Token);

            Logger.Info($"Email handed to {host} for {ResendEmailGateway.Mask(toEmail)}.");

            // SMTP gives back no id to match a later delivery event against.
            return GatewaySendResult.Ok(null);
        }
        catch (OperationCanceledException)
        {
            Logger.Warn($"{host} did not answer within {SendTimeoutSeconds}s.");
            return GatewaySendResult.Failed(
                $"The mail server did not answer within {SendTimeoutSeconds} seconds. "
                + "Some hosts block outbound SMTP — check that port "
                + $"{port} is reachable from the server.");
        }
        catch (SmtpException ex)
        {
            var error = Describe(ex);
            Logger.Warn($"SMTP refused the email to {ResendEmailGateway.Mask(toEmail)}: {error}");
            return GatewaySendResult.Failed(error);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Could not reach {host} for {ResendEmailGateway.Mask(toEmail)}: {ex.Message}");
            return GatewaySendResult.Failed($"Could not reach the mail server: {ex.Message}");
        }
    }

    /// <summary>
    /// The two refusals that actually happen, said in words that point at the
    /// fix rather than at an SMTP status code.
    /// </summary>
    private static string Describe(SmtpException ex)
    {
        if (ex.StatusCode == SmtpStatusCode.MustIssueStartTlsFirst
            || ex.StatusCode == SmtpStatusCode.ClientNotPermitted
            || ex.Message.Contains("5.7.", StringComparison.Ordinal))
        {
            return "The mail server rejected the sign-in. For Gmail the password must be an "
                 + "app password, which needs 2-Step Verification switched on — an account "
                 + "password will not work.";
        }

        if (ex.StatusCode == SmtpStatusCode.MailboxBusy
            || ex.StatusCode == SmtpStatusCode.InsufficientStorage
            || ex.Message.Contains("4.7.0", StringComparison.Ordinal))
        {
            return "The mail server is throttling us. Gmail allows about 500 messages a day "
                 + "on a free account and 2000 on Workspace.";
        }

        return $"The mail server refused it: {ex.Message}";
    }
}
