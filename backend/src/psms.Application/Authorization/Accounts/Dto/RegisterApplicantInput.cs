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

    /// <summary>
    /// Whether the school may email this person about their application.
    /// <para>
    /// Every external channel is consent-gated (COMM-05) and nothing ever asked,
    /// so no applicant had a consent row and every admissions email was
    /// suppressed before it reached the provider — "No consent on file for this
    /// channel" in the delivery log, which nobody reads. The in-app notice
    /// landed and the family, who were not signed in, learned nothing.
    /// </para>
    /// <para>
    /// Defaults to true when the form does not say: this is the address they
    /// have just given in order to apply, on a page that tells them it is where
    /// the school will contact them. They can turn it off here or later.
    /// </para>
    /// </summary>
    public bool? MayEmailMe { get; set; }
}
