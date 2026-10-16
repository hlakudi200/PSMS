using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using psms.Authorization;
using psms.Authorization.Users;
using System.Threading.Tasks;

namespace psms.Communication.Channels.Email;

/// <summary>What the email channel did when asked to prove itself.</summary>
public class EmailChannelTestResult
{
    /// <summary>Which provider was tried: Smtp, Resend, or none.</summary>
    public string Provider { get; set; }

    /// <summary>Where it was sent, masked.</summary>
    public string SentTo { get; set; }

    public bool Success { get; set; }

    /// <summary>True when no provider is configured at all, which is not a failure.</summary>
    public bool NotConfigured { get; set; }

    /// <summary>Why not, in words that name the setting or the likely cause.</summary>
    public string Error { get; set; }

    /// <summary>The provider's own id for the message, where it gives one.</summary>
    public string ProviderMessageId { get; set; }

    /// <summary>
    /// Every provider and what each is still waiting for, so a send that went
    /// through the wrong one can be explained rather than guessed at.
    /// </summary>
    public EmailProviderReadiness[] Providers { get; set; }
}

/// <summary>
/// Sends one email to whoever asked, and reports exactly what the provider said.
/// <para>
/// Added because finding out whether email worked otherwise meant registering
/// an applicant, filling in an application, pushing it through four workflow
/// steps, making an admission decision and then reading the delivery log — a
/// ten-minute round trip per guess, on live data. Three guesses in: a missing
/// consent row, a missing setting, and a host that blocks outbound SMTP.
/// </para>
/// <para>
/// It sends to the caller's own address and nowhere else. A diagnostic that can
/// address a stranger is a spam tool.
/// </para>
/// </summary>
[AbpAuthorize(PermissionNames.Communication_Notifications_Configure)]
public class EmailChannelDiagnosticsService : ApplicationService
{
    private readonly IEmailGateway _gateway;
    private readonly IRepository<User, long> _users;

    public EmailChannelDiagnosticsService(IEmailGateway gateway, IRepository<User, long> users)
    {
        _gateway = gateway;
        _users = users;
    }

    /// <summary>
    /// Sends a test message to the signed-in user's own email address.
    /// <para>
    /// Deliberately not routed through the dispatcher: this answers "can this
    /// deployment send an email at all", and going through the dispatcher would
    /// fold in consent and preference gating, which are separate questions with
    /// their own answers in the delivery log.
    /// </para>
    /// </summary>
    public async Task<EmailChannelTestResult> SendTestEmailAsync()
    {
        var user = await _users.GetAsync(AbpSession.GetUserId());

        if (string.IsNullOrWhiteSpace(user.EmailAddress))
            return new EmailChannelTestResult
            {
                Provider = (_gateway as ConfiguredEmailGateway)?.SelectedProvider ?? "unknown",
                Success = false,
                Error = "Your account has no email address, so there is nowhere to send it.",
            };

        var result = await _gateway.SendAsync(
            user.EmailAddress,
            "PSMS email test",
            "<p>If you are reading this, this school's PSMS can send email.</p>"
            + "<p>Nobody else was sent anything — a test goes only to the address of "
            + "whoever asked for it.</p>");

        return new EmailChannelTestResult
        {
            Provider = (_gateway as ConfiguredEmailGateway)?.SelectedProvider ?? "unknown",
            SentTo = ResendEmailGateway.Mask(user.EmailAddress),
            Success = result.Success,
            NotConfigured = result.NotConfigured,
            Error = result.Error,
            ProviderMessageId = result.ProviderMessageId,
            Providers = (_gateway as ConfiguredEmailGateway)?.Providers,
        };
    }

    /// <summary>
    /// Reports which providers are configured, and which one a send would go
    /// through, without sending anything.
    /// <para>
    /// The send test cannot answer this on its own. A deployment reporting
    /// <c>Smtp</c> after a Resend key was added looks the same whether the key
    /// did not take or the running build predates the preference change, and
    /// SMTP takes twenty seconds to time out before saying so. This answers in
    /// one call and costs nothing — no message, no provider quota, no row in
    /// the delivery log.
    /// </para>
    /// <para>
    /// Setting names only, never values.
    /// </para>
    /// </summary>
    public Task<EmailChannelStatus> GetStatusAsync()
    {
        var configured = _gateway as ConfiguredEmailGateway;

        return Task.FromResult(new EmailChannelStatus
        {
            SelectedProvider = configured?.SelectedProvider ?? "unknown",
            Providers = configured?.Providers ?? new EmailProviderReadiness[0],
        });
    }
}

/// <summary>What the email channel is set up to do, without doing it.</summary>
public class EmailChannelStatus
{
    /// <summary>The provider a send would go through: Resend, Smtp, or none.</summary>
    public string SelectedProvider { get; set; }

    /// <summary>Both providers, each with the settings it still needs.</summary>
    public EmailProviderReadiness[] Providers { get; set; }
}
