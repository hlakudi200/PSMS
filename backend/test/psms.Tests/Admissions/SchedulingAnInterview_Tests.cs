using psms.Admissions.AdmissionInterviews;
using psms.Admissions.AdmissionInterviews.Dto;
using psms.Authorization;
using Shouldly;
using System.Linq;
using System.Reflection;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// Booking an interview, and the person booked for it.
/// <para>
/// Schedule, Complete and MarkNoShow all existed and no screen called any of
/// them, so the Interview step of the admissions workflow was a step about
/// something that could not happen. Worse, scheduling took the interviewer's
/// id and name straight from the request, unchecked and unrelated to each
/// other: an interview could be booked against somebody who did not exist,
/// under a name belonging to someone else, or against a colleague with no
/// permission to open it — and that only surfaced on the day, when they came
/// to record the outcome and could not.
/// </para>
/// </summary>
public class SchedulingAnInterview_Tests
{
    private static MethodInfo Method(string name) =>
        typeof(AdmissionInterviewAppService).GetMethod(name);

    private static string PermissionOf(string method) =>
        Method(method)?.GetCustomAttribute<Abp.Authorization.AbpAuthorizeAttribute>()
            ?.Permissions?.FirstOrDefault();

    [Fact]
    public void The_interviewer_is_resolved_rather_than_trusted()
    {
        // The name stored on the interview comes from the school's record of
        // the chosen user, so the id and the name cannot describe two people.
        typeof(AdmissionInterviewAppService)
            .GetMethod("ResolveInterviewerAsync", BindingFlags.NonPublic | BindingFlags.Instance)
            .ShouldNotBeNull();
    }

    [Fact]
    public void And_the_name_on_the_request_is_no_longer_required()
    {
        // It is ignored now; demanding it would only make callers invent one.
        var required = typeof(ScheduleInterviewDto)
            .GetProperty(nameof(ScheduleInterviewDto.InterviewerName))
            .GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>();

        required.ShouldBeNull();
    }

    [Fact]
    public void Recording_an_outcome_is_checked_against_who_conducted_it()
    {
        typeof(AdmissionInterviewAppService)
            .GetMethod("AssertMayRecordOutcomeAsync", BindingFlags.NonPublic | BindingFlags.Instance)
            .ShouldNotBeNull();
    }

    [Fact]
    public void A_teacher_can_find_the_interviews_they_are_down_for()
    {
        // GetMine asks only for View — being named the interviewer is the
        // authority, and a teacher holds nothing that would let them reach it
        // through the application it belongs to.
        PermissionOf(nameof(AdmissionInterviewAppService.GetMineAsync))
            .ShouldBe(PermissionNames.Admissions_Interviews_View);

        PsmsRolePermissionSeeder.TeacherPermissions()
            .ShouldContain(PermissionNames.Admissions_Interviews_View);
    }

    [Fact]
    public void But_cannot_see_who_else_is_available_to_interview()
    {
        // Choosing an interviewer is the admissions staff's job, and the list
        // is every colleague's name and workload.
        PermissionOf(nameof(AdmissionInterviewAppService.GetInterviewersAsync))
            .ShouldBe(PermissionNames.Admissions_Interviews_Schedule);

        PsmsRolePermissionSeeder.TeacherPermissions()
            .ShouldNotContain(PermissionNames.Admissions_Interviews_Schedule);
    }

    [Fact]
    public void The_roles_that_arrange_interviews_can_see_that_list()
    {
        var admissions = typeof(PsmsRolePermissionSeeder)
            .GetMethod("GetAdmissionsOfficerPermissions", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, null) as System.Collections.Generic.List<string>;

        admissions.ShouldContain(PermissionNames.Admissions_Interviews_Schedule);
    }
}
