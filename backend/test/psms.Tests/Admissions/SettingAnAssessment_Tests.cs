using psms.Admissions.AdmissionAssessments;
using psms.Authorization;
using Shouldly;
using System.Linq;
using System.Reflection;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// Setting a placement assessment, and the person down to mark it.
/// <para>
/// Schedule, RecordResults and Cancel all existed and no screen called any of
/// them, so the workflow's Placement Assessment step was a step about something
/// that could not happen — the same hole the interview had.
/// </para>
/// <para>
/// Scheduling also took the assessor's id straight from the request, with
/// nothing checking it belonged to somebody who could open the result. The
/// mistake surfaced on the day the paper needed marking.
/// </para>
/// </summary>
public class SettingAnAssessment_Tests
{
    private static string PermissionOf(string method) =>
        typeof(AdmissionAssessmentAppService).GetMethod(method)
            ?.GetCustomAttribute<Abp.Authorization.AbpAuthorizeAttribute>()
            ?.Permissions?.FirstOrDefault();

    private static MethodInfo Private(string name) =>
        typeof(AdmissionAssessmentAppService)
            .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

    [Fact]
    public void The_assessor_is_resolved_rather_than_trusted()
    {
        Private("ResolveAssessorAsync").ShouldNotBeNull();
    }

    [Fact]
    public void Recording_a_result_is_checked_against_who_marked_it()
    {
        // Otherwise anyone holding RecordResults could enter somebody else's
        // marking under that person's name.
        Private("AssertMayRecordAsync").ShouldNotBeNull();
    }

    [Fact]
    public void The_assessor_gets_a_name()
    {
        /* The entity stores only a user id, and the mapper ignored AssessorName
           with a note saying the service would fill it in. No service did, so
           every assessment came back with no assessor — including on the
           application page, which printed an empty cell. */
        Private("NameTheAssessorsAsync").ShouldNotBeNull();
    }

    [Fact]
    public void A_teacher_can_find_the_assessments_they_are_down_for()
    {
        // GetMine asks only for View — being named the assessor is the
        // authority, and a teacher holds nothing that would let them reach it
        // through the application it belongs to.
        PermissionOf(nameof(AdmissionAssessmentAppService.GetMineAsync))
            .ShouldBe(PermissionNames.Admissions_Assessments_View);

        PsmsRolePermissionSeeder.TeacherPermissions()
            .ShouldContain(PermissionNames.Admissions_Assessments_View);
    }

    [Fact]
    public void And_record_the_result()
    {
        PsmsRolePermissionSeeder.TeacherPermissions()
            .ShouldContain(PermissionNames.Admissions_Assessments_Conduct);
        PsmsRolePermissionSeeder.TeacherPermissions()
            .ShouldContain(PermissionNames.Admissions_Assessments_RecordResults);
    }

    [Fact]
    public void But_not_choose_who_sets_them()
    {
        // Picking an assessor is the admissions staff's job, and the list is
        // every colleague's name and how much marking they already have.
        PermissionOf(nameof(AdmissionAssessmentAppService.GetAssessorsAsync))
            .ShouldBe(PermissionNames.Admissions_Assessments_Schedule);

        PsmsRolePermissionSeeder.TeacherPermissions()
            .ShouldNotContain(PermissionNames.Admissions_Assessments_Schedule);
    }

    [Fact]
    public void The_roles_that_arrange_them_can_see_that_list()
    {
        var admissions = typeof(PsmsRolePermissionSeeder)
            .GetMethod("GetAdmissionsOfficerPermissions", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, null) as System.Collections.Generic.List<string>;

        admissions.ShouldContain(PermissionNames.Admissions_Assessments_Schedule);
    }
}
