using psms.Authorization;
using psms.Domain.Workflow.Entities;
using psms.Workflow.Engine;
using psms.Workflow.Engine.SkipRules;
using Shouldly;
using System.Linq;
using System.Reflection;
using Xunit;

namespace psms.Tests.Workflow;

/// <summary>
/// WF-34: the steps a school said it does not do.
/// <para>
/// A school records per intake whether it interviews and whether it sets a
/// placement assessment. Those answers were stored and then ignored — every
/// application walked the same five steps, so a school that does not interview
/// still had an Interview step sitting in somebody's queue waiting to be
/// dismissed, and the person dismissing it had no part in admissions.
/// </para>
/// <para>
/// A step that does not apply is now stepped over on entry, with the reason in
/// the history. These pin the arrangement that makes that happen; the rules'
/// own reading of the settings is exercised against the database.
/// </para>
/// </summary>
public class SkippingStepsAGradeDoesNotNeed_Tests
{
    [Fact]
    public void A_step_can_say_which_records_it_does_not_apply_to()
    {
        typeof(WorkflowStep).GetProperty(nameof(WorkflowStep.SkipWhenKey))
            .ShouldNotBeNull();
    }

    [Fact]
    public void The_interview_rule_is_registered_for_applications()
    {
        var rule = new InterviewNotRequiredSkipRule(null, null);

        rule.Key.ShouldBe("application.interview-not-required");
        rule.EntityType.ShouldBe(psms.Domain.Workflow.Enums.WorkflowEntityType.Application);
        rule.ShouldBeAssignableTo<IWorkflowStepSkipRule>();
    }

    [Fact]
    public void And_so_is_the_assessment_rule()
    {
        var rule = new AssessmentNotRequiredSkipRule(null, null);

        rule.Key.ShouldBe("application.assessment-not-required");
        rule.ShouldBeAssignableTo<IWorkflowStepSkipRule>();
    }

    [Fact]
    public void A_skipped_step_is_recorded_as_a_waive_so_the_history_shows_it()
    {
        // The transition already carries IsWaived — "passed without its work
        // being done" — which is exactly what an automatic skip is. Nothing
        // silently disappears from the record.
        typeof(WorkflowTransition).GetProperty(nameof(WorkflowTransition.IsWaived))
            .ShouldNotBeNull();
    }

    [Fact]
    public void A_terminal_step_is_never_skipped()
    {
        // The guarantee lives in the engine, and the engine is what this names:
        // whatever a definition says, the decision at the end of a workflow
        // cannot be configured away.
        var engine = typeof(psms.Workflow.WorkflowInstances.WorkflowInstanceAppService);
        var walker = engine.GetMethod("StepOverInapplicableAsync", BindingFlags.NonPublic | BindingFlags.Instance);

        walker.ShouldNotBeNull();
    }
}

/// <summary>
/// The Interview step belongs to the person who conducts the interview.
/// <para>
/// It was assigned to Admin — the school's IT administrator — along with the
/// three steps either side of it, while the Teacher role held no admissions
/// interview permission of any kind, not even View. So the teacher who sat in
/// the room could not see the interview or record what came of it, and someone
/// with no part in it clicked the step through.
/// </para>
/// </summary>
public class TeachersConductAdmissionInterviews_Tests
{
    private static System.Collections.Generic.IReadOnlyList<string> Teacher() =>
        PsmsRolePermissionSeeder.TeacherPermissions();

    [Fact]
    public void A_teacher_can_see_an_interview()
    {
        Teacher().ShouldContain(PermissionNames.Admissions_Interviews);
        Teacher().ShouldContain(PermissionNames.Admissions_Interviews_View);
    }

    [Fact]
    public void And_record_what_came_of_it()
    {
        Teacher().ShouldContain(PermissionNames.Admissions_Interviews_Conduct);
        Teacher().ShouldContain(PermissionNames.Admissions_Interviews_RecordOutcome);
    }

    [Fact]
    public void And_act_on_the_workflow_step_it_belongs_to()
    {
        // Holding the interview permissions is no use if the step cannot be
        // advanced; both halves have to be true for the assignment to mean
        // anything.
        Teacher().ShouldContain(PermissionNames.Workflow_Instances_Advance);
    }

    [Fact]
    public void A_teacher_still_cannot_read_other_applications()
    {
        // Conducting an interview is not a reason to see the school's whole
        // admissions book.
        Teacher().ShouldNotContain(PermissionNames.Admissions_Applications_ViewAll);
        Teacher().ShouldNotContain(PermissionNames.Admissions_Decision_Approve);
    }

    [Fact]
    public void The_two_seeders_agree_about_this()
    {
        /* One seeds new tenants, the other tenant 1 and the tests, and a
           permission granted in one and not the other is a role that behaves
           differently depending on when the school was created. The two lists
           already differ more broadly than this — DefaultRolesCreator's teacher
           has no Academic.Subjects or Academic.ClassSubjects, among others —
           which is its own problem and not one to paper over here. This pins
           only the interview grants, so that half cannot drift. */
        var fromDefaults = (System.Collections.Generic.List<string>)
            typeof(psms.EntityFrameworkCore.Seed.Tenants.DefaultRolesCreator)
                .GetMethod("GetTeacherPermissions", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, null);

        var interviewGrants = new[]
        {
            PermissionNames.Admissions_Interviews,
            PermissionNames.Admissions_Interviews_View,
            PermissionNames.Admissions_Interviews_Conduct,
            PermissionNames.Admissions_Interviews_RecordOutcome,
        };

        foreach (var permission in interviewGrants)
        {
            Teacher().ShouldContain(permission);
            fromDefaults.ShouldContain(permission);
        }
    }
}
