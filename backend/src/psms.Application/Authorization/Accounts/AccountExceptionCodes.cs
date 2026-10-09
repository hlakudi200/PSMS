namespace psms.Authorization.Accounts;

/// <summary>
/// Codes for the refusals this module raises. See the module's other
/// *ExceptionCodes for the convention.
/// </summary>
public static class AccountExceptionCodes
{
    /// <summary>Sign-up is rate limited; this caller has had their share for now.</summary>
    public const string TooManySignUps = "ACC_TOO_MANY_SIGN_UPS";

    /// <summary>An account already exists for this email address.</summary>
    public const string EmailAlreadyRegistered = "ACC_EMAIL_ALREADY_REGISTERED";
}
