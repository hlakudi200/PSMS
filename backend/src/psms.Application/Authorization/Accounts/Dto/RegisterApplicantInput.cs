using Abp.Auditing;
using Abp.Authorization.Users;
using System.ComponentModel.DataAnnotations;

namespace psms.Authorization.Accounts.Dto;

/// <summary>
/// ADM-APPLY. A prospective parent or guardian signing themselves up so they
/// can apply to the school.
/// <para>
/// Deliberately narrower than ABP's <see cref="RegisterInput"/>: no username to
/// choose — the email address is the username, because a parent applying once
/// should not have to invent and remember a second identifier — and no role to
/// pick, because this door only ever produces an applicant.
/// </para>
/// </summary>
public class RegisterApplicantInput
{
    [Required]
    [StringLength(AbpUserBase.MaxNameLength, MinimumLength = 2)]
    public string Name { get; set; }

    [Required]
    [StringLength(AbpUserBase.MaxSurnameLength, MinimumLength = 2)]
    public string Surname { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(AbpUserBase.MaxEmailAddressLength)]
    public string EmailAddress { get; set; }

    [Required]
    [StringLength(AbpUserBase.MaxPlainPasswordLength, MinimumLength = 8)]
    [DisableAuditing]
    public string Password { get; set; }
}
