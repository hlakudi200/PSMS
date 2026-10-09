using Abp.Authorization;
using Abp.Configuration;
using Abp.UI;
using Abp.Zero.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using psms.Authorization.Accounts.Dto;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Authorization.Accounts;

public class AccountAppService : psmsAppServiceBase, IAccountAppService
{
    // from: http://regexlib.com/REDetails.aspx?regexp_id=1923
    public const string PasswordRegex = "(?=^.{8,}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s)[0-9a-zA-Z!@#$%^&*()]*$";

    private readonly UserRegistrationManager _userRegistrationManager;
    private readonly UserManager _userManager;
    private readonly ApplicantSignUpThrottle _throttle;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountAppService(
        UserRegistrationManager userRegistrationManager,
        UserManager userManager,
        ApplicantSignUpThrottle throttle,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRegistrationManager = userRegistrationManager;
        _userManager = userManager;
        _throttle = throttle;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input)
    {
        var tenant = await TenantManager.FindByTenancyNameAsync(input.TenancyName);
        if (tenant == null)
        {
            return new IsTenantAvailableOutput(TenantAvailabilityState.NotFound);
        }

        if (!tenant.IsActive)
        {
            return new IsTenantAvailableOutput(TenantAvailabilityState.InActive);
        }

        return new IsTenantAvailableOutput(TenantAvailabilityState.Available, tenant.Id);
    }

    /// <summary>
    /// ABP's generic sign-up, now behind the permission to create users.
    /// <para>
    /// This arrived with the project template and was left open to the
    /// internet: anyone who knew a tenant id could create accounts on it. They
    /// landed with no role, so nothing could be done with them, which is the
    /// only reason it had not caused harm. It is still a stranger writing a
    /// real person's name and email address into the school's database.
    /// </para>
    /// <para>
    /// A prospective parent signs up through
    /// <see cref="RegisterApplicant"/> instead, which is the one door that is
    /// deliberately open and knows what it is for.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Administration_Users_Create)]
    public async Task<RegisterOutput> Register(RegisterInput input)
    {
        var user = await _userRegistrationManager.RegisterAsync(
            input.Name,
            input.Surname,
            input.EmailAddress,
            input.UserName,
            input.Password,
            true // Assumed email address is always confirmed. Change this if you want to implement email confirmation.
        );

        var isEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin);

        return new RegisterOutput
        {
            CanLogin = user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin)
        };
    }

    /// <summary>
    /// ADM-APPLY. A prospective parent or guardian signs themselves up so they
    /// can apply to the school.
    /// <para>
    /// The one anonymous door in the system that creates anything, and it only
    /// ever produces an applicant: the role is assigned here rather than left
    /// to whichever roles happen to be marked default, so a change to the role
    /// seeder cannot quietly turn sign-ups into staff. The Applicant role
    /// already carries exactly what this person needs — create, edit, submit
    /// and withdraw their own application, upload its documents, accept or
    /// decline an offer — and nothing else.
    /// </para>
    /// <para>
    /// The email address is the username. A parent applying for one child
    /// should not have to invent a second identifier and remember which one
    /// the school wanted.
    /// </para>
    /// </summary>
    public async Task<RegisterOutput> RegisterApplicant(RegisterApplicantInput input)
    {
        if (!await _throttle.IsWithinLimitAsync(AbpSession.TenantId, CallerAddress()))
            throw new UserFriendlyException(AccountExceptionCodes.TooManySignUps,
                "Too many accounts have been created from here recently. Wait a little while "
                + "and try again, or phone the school and they will help you apply.");

        var email = input.EmailAddress.Trim();

        // A friendlier answer than the one the user manager gives, and the same
        // answer either way, so this cannot be used to discover who has an
        // account at the school.
        var taken = await _userManager.Users.AnyAsync(u =>
            u.TenantId == AbpSession.TenantId &&
            (u.NormalizedEmailAddress == email.ToUpperInvariant()
             || u.NormalizedUserName == email.ToUpperInvariant()));

        if (taken)
            throw new UserFriendlyException(AccountExceptionCodes.EmailAlreadyRegistered,
                "There is already an account for this email address. Sign in instead, or use "
                + "Forgot Password if you cannot remember it.");

        var user = await _userRegistrationManager.RegisterAsync(
            input.Name.Trim(),
            input.Surname.Trim(),
            email,
            email,
            input.Password,
            isEmailConfirmed: true);

        CheckErrors(await _userManager.SetRolesAsync(user, new[] { StaticRoleNames.Tenants.Applicant }));

        var isEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(
            AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin);

        return new RegisterOutput
        {
            CanLogin = user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin)
        };
    }

    /// <summary>
    /// Who is asking, for the sign-up limit. The proxy header first, because
    /// every request to this application arrives through one.
    /// </summary>
    private string CallerAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return null;

        var forwarded = context.Request.Headers["X-Forwarded-For"].ToString();
        if (!string.IsNullOrWhiteSpace(forwarded))
            return forwarded.Split(',')[0].Trim();

        return context.Connection.RemoteIpAddress?.ToString();
    }
}
