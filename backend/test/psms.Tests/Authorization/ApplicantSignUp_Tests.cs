using Abp.UI;
using psms.Authorization.Accounts;
using psms.Authorization.Accounts.Dto;
using psms.Authorization.Roles;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Authorization;

/// <summary>
/// A prospective parent signs themselves up so they can apply to the school.
/// <para>
/// Before this there was no door at all: the Applicant role existed and
/// carried exactly the right permissions, the apply page was already gated on
/// it, and nothing could put anyone in it. What <i>was</i> open was ABP's
/// template <c>Register</c> — anyone who knew a tenant id could create accounts
/// on it, anonymously, with no limit. Those accounts landed with no role, which
/// is the only reason it had not caused harm; it was still a stranger writing a
/// real person's name and email address into a school's database.
/// </para>
/// </summary>
public class ApplicantSignUp_Tests : psmsTestBase
{
    private readonly IAccountAppService _accounts;

    public ApplicantSignUp_Tests()
    {
        _accounts = Resolve<IAccountAppService>();
        // A prospective parent has no session. This is the point.
        AbpSession.TenantId = 1;
        AbpSession.UserId = null;
    }

    private static RegisterApplicantInput Parent(string email) => new RegisterApplicantInput
    {
        Name = "Thandiwe",
        Surname = "Nkosi",
        EmailAddress = email,
        Password = "Applying@2026",
    };

    [Fact]
    public async Task A_parent_can_sign_themselves_up_without_an_account()
    {
        var result = await _accounts.RegisterApplicant(Parent("thandiwe@example.test"));

        result.CanLogin.ShouldBeTrue();
    }

    [Fact]
    public async Task They_land_in_the_Applicant_role_and_nothing_else()
    {
        // Assigned here rather than left to whichever roles happen to be marked
        // default, so a change to the role seeder cannot quietly turn sign-ups
        // into staff.
        await _accounts.RegisterApplicant(Parent("roles@example.test"));

        var roles = await UsingDbContextAsync(1, async context =>
        {
            var user = context.Users.Single(u => u.EmailAddress == "roles@example.test");
            return context.UserRoles.Where(r => r.UserId == user.Id)
                .Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToList();
        });

        roles.ShouldBe(new[] { StaticRoleNames.Tenants.Applicant });
    }

    [Fact]
    public async Task Their_email_address_is_their_username()
    {
        // A parent applying for one child should not have to invent a second
        // identifier and then remember which one the school wanted.
        await _accounts.RegisterApplicant(Parent("username@example.test"));

        var user = await UsingDbContextAsync(1, async context =>
            context.Users.Single(u => u.EmailAddress == "username@example.test"));

        user.UserName.ShouldBe("username@example.test");
    }

    [Fact]
    public async Task Signing_up_twice_with_one_address_is_refused_kindly()
    {
        await _accounts.RegisterApplicant(Parent("twice@example.test"));

        var again = await Should.ThrowAsync<UserFriendlyException>(
            () => _accounts.RegisterApplicant(Parent("twice@example.test")));

        again.Message.ShouldBe(AccountExceptionCodes.EmailAlreadyRegistered);
        again.Details.ShouldContain("Sign in instead");
    }

    [Fact]
    public async Task The_name_and_address_are_trimmed()
    {
        await _accounts.RegisterApplicant(new RegisterApplicantInput
        {
            Name = "  Thandiwe  ",
            Surname = "  Nkosi  ",
            EmailAddress = "  spaced@example.test  ",
            Password = "Applying@2026",
        });

        var user = await UsingDbContextAsync(1, async context =>
            context.Users.Single(u => u.EmailAddress == "spaced@example.test"));

        user.Name.ShouldBe("Thandiwe");
        user.Surname.ShouldBe("Nkosi");
    }
}
