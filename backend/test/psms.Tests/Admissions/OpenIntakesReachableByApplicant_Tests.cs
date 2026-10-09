using psms.Admissions.AdmissionSettings;
using psms.Admissions.Applications;
using psms.Authorization;
using Shouldly;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// "What can I apply for" has to be answerable by the person applying.
/// <para>
/// ABP checks the class-level <c>AbpAuthorize</c> as well as the method's: a
/// method-level attribute <i>adds to</i> the class's, it does not replace it.
/// So this call sat on AdmissionSettingsAppService — guarded by
/// <c>Admissions.Settings</c>, which no applicant holds — and was refused
/// before its own guard was ever consulted. The apply form's first field had
/// nothing to show, and the 403 was invisible because the screen swallowed it.
/// </para>
/// <para>
/// These pin the permission arrangement rather than the data, because the data
/// was never the problem.
/// </para>
/// </summary>
public class OpenIntakesReachableByApplicant_Tests
{
    private static string ClassPermissionOf(Type service) =>
        service.GetCustomAttribute<Abp.Authorization.AbpAuthorizeAttribute>()?.Permissions?.FirstOrDefault();

    private static string MethodPermissionOf(Type service, string method) =>
        service.GetMethod(method)?.GetCustomAttribute<Abp.Authorization.AbpAuthorizeAttribute>()
            ?.Permissions?.FirstOrDefault();

    [Fact]
    public void It_lives_on_a_service_an_applicant_can_reach()
    {
        ClassPermissionOf(typeof(ApplicationAppService))
            .ShouldBe(PermissionNames.Admissions_Applications);
    }

    [Fact]
    public void And_asks_only_for_the_permission_to_apply()
    {
        MethodPermissionOf(typeof(ApplicationAppService), nameof(ApplicationAppService.GetOpenIntakesAsync))
            .ShouldBe(PermissionNames.Admissions_Applications_Create);
    }

    [Fact]
    public void Both_of_those_are_permissions_an_applicant_holds()
    {
        // The Applicant role's own grant list, which is what makes the above
        // reachable rather than merely sensible.
        var applicant = psms.Authorization.PsmsRolePermissionSeeder.ApplicantPermissions();

        applicant.ShouldContain(PermissionNames.Admissions_Applications);
        applicant.ShouldContain(PermissionNames.Admissions_Applications_Create);
    }

    [Fact]
    public void The_settings_service_still_guards_the_rest_of_itself()
    {
        // Moving one call out must not have loosened the service it came from.
        ClassPermissionOf(typeof(AdmissionSettingsAppService))
            .ShouldBe(PermissionNames.Admissions_Settings);
    }

    [Fact]
    public void An_applicant_does_not_hold_that_one()
    {
        var applicant = psms.Authorization.PsmsRolePermissionSeeder.ApplicantPermissions();

        applicant.ShouldNotContain(PermissionNames.Admissions_Settings);
    }
}
