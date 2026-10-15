using Abp.Authorization;
using psms.Authorization;
using psms.Authorization.Roles;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace psms.Tests.Authorization;

/// <summary>
/// A school created last year still gets this year's permissions.
/// <para>
/// A permission added to a role in code only reaches tenants created after it.
/// #368 gave the admissions Interview step to the Teacher role and granted
/// teachers the interview permissions to go with it; both seeders carry those
/// grants. An existing school's Teacher role held none of them, so the teacher
/// sitting in the interview could not see it, record it, or open their own
/// admissions page — which answered 403 — while the workflow went on handing
/// them the step.
/// </para>
/// <para>
/// The two backfills that existed each covered one feature group and both sat
/// behind <c>Pages.Tenants</c>, which is host-only, so no school administrator
/// could run either. These pin the replacement: every role, reachable by the
/// school itself, additive.
/// </para>
/// </summary>
public class RolesCatchUpWithTheCode_Tests
{
    private static List<string> PermissionsFor(string getter) =>
        (List<string>)typeof(PsmsRolePermissionSeeder)
            .GetMethod(getter, BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, null);

    [Fact]
    public void A_teacher_can_see_and_record_the_interview_the_workflow_gives_them()
    {
        var teacher = PermissionsFor("GetTeacherPermissions");

        // The 403 that met a real teacher on /teacher/admissions was GetMine,
        // which wants the parent permission and View.
        teacher.ShouldContain(PermissionNames.Admissions_Interviews);
        teacher.ShouldContain(PermissionNames.Admissions_Interviews_View);
        teacher.ShouldContain(PermissionNames.Admissions_Interviews_RecordOutcome);
        teacher.ShouldContain(PermissionNames.Admissions_Assessments_View);
        teacher.ShouldContain(PermissionNames.Admissions_Assessments_RecordResults);
    }

    [Fact]
    public void Catching_up_covers_every_role_the_full_seed_covers()
    {
        /* The whole failure was a role being left behind, so a catch-up that
           quietly skips one is the same bug wearing a different hat. Both
           methods are read out of the source rather than trusted. */
        var source = System.IO.File.ReadAllText(SeederSourcePath());

        var full = RoleNamesIn(source, "public async Task SeedRolePermissionsAsync");
        var catchUp = RoleNamesIn(source, "public async Task CatchUpEveryRoleAsync");

        catchUp.ShouldNotBeEmpty();
        full.ShouldNotBeEmpty();
        catchUp.OrderBy(x => x).ShouldBe(full.OrderBy(x => x));
    }

    [Fact]
    public void Catching_up_never_takes_a_permission_away()
    {
        /* SetGrantedPermissions replaces a role's whole set and would wipe a
           school's customisations — which is exactly why the full seeder cannot
           be pointed at a live tenant. The catch-up must only ever grant. */
        var source = System.IO.File.ReadAllText(SeederSourcePath());
        var body = MethodBody(source, "public async Task CatchUpEveryRoleAsync");

        body.ShouldNotContain("GrantPermissionsToRoleAsync");
        body.ShouldContain("AddPermissionsToRoleAsync");
    }

    [Fact]
    public void The_school_can_reach_it_without_the_host()
    {
        /* Pages.Tenants is host-only, and a host admin is signed in to no
           tenant — which is why the two older backfills were never run. */
        var guard = typeof(psms.Authorization.RolePermissionCatchUpService)
            .GetCustomAttribute<AbpAuthorizeAttribute>();

        guard.ShouldNotBeNull();
        guard.Permissions.ShouldContain(PermissionNames.Pages_Roles);
        guard.Permissions.ShouldNotContain(PermissionNames.Pages_Tenants);
    }

    private static string SeederSourcePath()
    {
        var directory = new System.IO.DirectoryInfo(System.AppContext.BaseDirectory);
        while (directory != null && !System.IO.Directory.Exists(System.IO.Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;

        return System.IO.Path.Combine(directory.FullName,
            "src", "psms.Core", "Authorization", "Roles", "PsmsRolePermissionSeeder.cs");
    }

    private static string MethodBody(string source, string signature)
    {
        var start = source.IndexOf(signature, System.StringComparison.Ordinal);
        start.ShouldBeGreaterThan(-1, $"{signature} not found");
        var open = source.IndexOf('{', start);
        var close = source.IndexOf("\n    }", open, System.StringComparison.Ordinal);
        return source.Substring(open, close - open);
    }

    private static List<string> RoleNamesIn(string source, string signature) =>
        System.Text.RegularExpressions.Regex
            .Matches(MethodBody(source, signature), @"StaticRoleNames\.Tenants\.(\w+)")
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .ToList();
}
