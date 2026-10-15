using Abp.Authorization.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using psms.Assessment.Marks;
using psms.Assessment.Marks.Dto;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.Domain.Academic.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// #405: a learner and their family see a mark when the teacher releases it,
/// not when it is captured. Every mark read a student or parent can make —
/// Get, GetAll, GetByAssessment, GetByStudent — keeps an unreleased mark's
/// status and withholds the score, level, feedback, comment and moderation.
/// Staff reads are unchanged.
/// </summary>
public class UnreleasedMarks_Tests : psmsTestBase
{
    private const string FeedbackText = "Clear working shown throughout; revise fractions before the test.";

    private readonly IMarkAppService _marks;

    private Guid _studentId;
    private Guid _releasedAssessmentId;
    private Guid _unreleasedAssessmentId;
    private Guid _releasedMarkId;
    private Guid _unreleasedMarkId;
    private long _studentUserId;
    private long _parentUserId;

    public UnreleasedMarks_Tests()
    {
        _marks = Resolve<IMarkAppService>();
        Seed();
    }

    private void Seed()
    {
        var studentUser = CreateUser("unreleased.student");
        var parentUser = CreateUser("unreleased.parent");
        var teacherUser = CreateUser("unreleased.teacher");
        _studentUserId = studentUser.Id;
        _parentUserId = parentUser.Id;
        AssignRole(studentUser.Id, StaticRoleNames.Tenants.Student);
        AssignRole(parentUser.Id, StaticRoleNames.Tenants.Parent);

        _studentId = Guid.NewGuid();
        _releasedAssessmentId = Guid.NewGuid();
        _unreleasedAssessmentId = Guid.NewGuid();
        _releasedMarkId = Guid.NewGuid();
        _unreleasedMarkId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var classSubjectId = Guid.NewGuid();
        var termId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.Students.Add(new Student(_studentId, 1, "Ayanda", "Nkosi", new DateTime(2012, 1, 1),
                Gender.Female, "U0001", DateTime.Today, Guid.NewGuid(), Guid.NewGuid()) { UserId = studentUser.Id });
            context.Parents.Add(new Parent(parentId, 1, parentUser.Id, "Parent", "Nkosi", "unreleased.parent@test.local", "0710000000"));
            context.StudentParents.Add(new StudentParent(Guid.NewGuid(), _studentId, parentId, RelationshipType.Guardian));

            // GetAll includes each assessment's ClassSubject, a required navigation, so it must exist.
            var subjectId = Guid.NewGuid();
            context.Subjects.Add(new Subject(subjectId, 1, "Mathematics", "MATH", true));
            context.ClassSubjects.Add(new ClassSubject(classSubjectId, 1, Guid.NewGuid(), subjectId));

            var released = new psms.Domain.Assessment.Entities.Assessment(_releasedAssessmentId, 1, classSubjectId,
                termId, "Term 3 test", AcademicAssessmentType.Test, 50m, 1)
            {
                MarksReleased = true,
                MarksReleasedDate = DateTime.UtcNow.AddHours(-1),
            };
            var unreleased = new psms.Domain.Assessment.Entities.Assessment(_unreleasedAssessmentId, 1, classSubjectId,
                termId, "Term 3 assignment", AcademicAssessmentType.Assignment, 50m, 1);
            context.Assessments.Add(released);
            context.Assessments.Add(unreleased);

            context.Marks.Add(CapturedMark(_releasedMarkId, _releasedAssessmentId, teacherUser.Id));
            var pending = CapturedMark(_unreleasedMarkId, _unreleasedAssessmentId, teacherUser.Id);
            // Moderated before release: exactly what the family must not see yet.
            pending.ApplyModeration(2m, 50m);
            context.Marks.Add(pending);
            context.SaveChanges();
        });
    }

    private Mark CapturedMark(Guid markId, Guid assessmentId, long teacherUserId)
    {
        var mark = new Mark(markId, 1, assessmentId, _studentId);
        mark.RecordMark(40m, 50m, teacherUserId, "Rushed the last section.");
        mark.Feedback = FeedbackText;
        return mark;
    }

    private User CreateUser(string userName)
    {
        return UsingDbContext(context =>
        {
            var user = new User
            {
                TenantId = 1,
                UserName = userName,
                Name = userName,
                Surname = "Test",
                EmailAddress = $"{userName}@test.local",
                IsActive = true,
                IsEmailConfirmed = true,
            };
            user.SetNormalizedNames();
            user.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions()))
                .HashPassword(user, "Test123!");
            context.Users.Add(user);
            return user;
        });
    }

    private void AssignRole(long userId, string roleName)
    {
        UsingDbContext(context =>
        {
            var role = context.Roles.IgnoreQueryFilters().First(r => r.TenantId == 1 && r.Name == roleName);
            context.UserRoles.Add(new UserRole(1, userId, role.Id));
        });
    }

    private void LoginAs(long userId)
    {
        AbpSession.TenantId = 1;
        AbpSession.UserId = userId;
    }

    private static void ShouldBeWithheld(MarkListDto mark)
    {
        mark.Status.ShouldBe(MarkStatus.Completed, "the status stays, so the portal can say pending");
        mark.RawMark.ShouldBeNull();
        mark.Percentage.ShouldBeNull();
        mark.AchievementLevel.ShouldBeNull();
        mark.Feedback.ShouldBeNull();
        mark.TeacherComment.ShouldBeNull();
        mark.IsModerated.ShouldBeFalse();
        mark.ModerationAdjustment.ShouldBeNull();
    }

    private static void ShouldBeInFull(MarkListDto mark)
    {
        mark.RawMark.ShouldBe(40m);
        mark.Percentage.ShouldNotBeNull();
        mark.AchievementLevel.ShouldNotBeNull();
        mark.Feedback.ShouldBe(FeedbackText);
    }

    // ---- GetByStudent: what the mobile Marks and Progress screens call ----

    [Fact]
    public async Task A_student_sees_a_released_mark_and_not_an_unreleased_one()
    {
        LoginAs(_studentUserId);

        var marks = (await _marks.GetByStudentAsync(_studentId, null)).Items;

        ShouldBeInFull(marks.Single(m => m.AssessmentId == _releasedAssessmentId));
        ShouldBeWithheld(marks.Single(m => m.AssessmentId == _unreleasedAssessmentId));
    }

    [Fact]
    public async Task A_parent_sees_their_childs_released_mark_and_not_an_unreleased_one()
    {
        LoginAs(_parentUserId);

        var marks = (await _marks.GetByStudentAsync(_studentId, null)).Items;

        ShouldBeInFull(marks.Single(m => m.AssessmentId == _releasedAssessmentId));
        ShouldBeWithheld(marks.Single(m => m.AssessmentId == _unreleasedAssessmentId));
    }

    // ---- GetAll ----

    [Fact]
    public async Task GetAll_withholds_unreleased_marks_from_a_parent()
    {
        LoginAs(_parentUserId);

        var marks = (await _marks.GetAllAsync(new GetMarksInput { MaxResultCount = 100 })).Items;

        ShouldBeInFull(marks.Single(m => m.AssessmentId == _releasedAssessmentId));
        ShouldBeWithheld(marks.Single(m => m.AssessmentId == _unreleasedAssessmentId));
    }

    // ---- GetByAssessment ----

    [Fact]
    public async Task GetByAssessment_withholds_an_unreleased_mark_from_a_student()
    {
        LoginAs(_studentUserId);

        var marks = (await _marks.GetByAssessmentAsync(_unreleasedAssessmentId)).Items;

        ShouldBeWithheld(marks.Single());
    }

    [Fact]
    public async Task GetByAssessment_returns_a_released_mark_to_a_parent_in_full()
    {
        LoginAs(_parentUserId);

        var marks = (await _marks.GetByAssessmentAsync(_releasedAssessmentId)).Items;

        ShouldBeInFull(marks.Single());
    }

    // ---- Get ----

    [Fact]
    public async Task Get_withholds_an_unreleased_mark_from_a_parent()
    {
        LoginAs(_parentUserId);

        var mark = await _marks.GetAsync(_unreleasedMarkId);

        mark.Status.ShouldBe(MarkStatus.Completed);
        mark.MarksReleased.ShouldBeFalse();
        mark.RawMark.ShouldBeNull();
        mark.Percentage.ShouldBeNull();
        mark.AchievementLevel.ShouldBeNull();
        mark.Feedback.ShouldBeNull();
        mark.FeedbackHistory.ShouldBeNull();
        mark.TeacherComment.ShouldBeNull();
        mark.IsModerated.ShouldBeFalse();
        mark.ModerationAdjustment.ShouldBeNull();
    }

    [Fact]
    public async Task Get_returns_a_released_mark_to_a_student_in_full()
    {
        LoginAs(_studentUserId);

        var mark = await _marks.GetAsync(_releasedMarkId);

        mark.RawMark.ShouldBe(40m);
        mark.Percentage.ShouldNotBeNull();
        mark.Feedback.ShouldBe(FeedbackText);
    }

    // ---- Staff are unaffected ----

    [Fact]
    public async Task Staff_still_see_an_unreleased_mark_in_full()
    {
        LoginAsDefaultTenantAdmin();

        var listed = (await _marks.GetByAssessmentAsync(_unreleasedAssessmentId)).Items.Single();
        var single = await _marks.GetAsync(_unreleasedMarkId);

        ShouldBeInFull(listed);
        listed.TeacherComment.ShouldBe("Rushed the last section.");
        listed.IsModerated.ShouldBeTrue();
        listed.ModerationAdjustment.ShouldBe(2m);
        single.RawMark.ShouldBe(40m);
        single.Feedback.ShouldBe(FeedbackText);
    }

    // ---- Releasing the marks is what shows them ----

    [Fact]
    public async Task Once_released_the_same_mark_reaches_the_family()
    {
        UsingDbContext(1, context =>
        {
            var assessment = context.Assessments.IgnoreQueryFilters().Single(a => a.Id == _unreleasedAssessmentId);
            assessment.MarksReleased = true;
            assessment.MarksReleasedDate = DateTime.UtcNow;
            context.SaveChanges();
        });
        LoginAs(_parentUserId);

        var mark = (await _marks.GetByStudentAsync(_studentId, null)).Items
            .Single(m => m.AssessmentId == _unreleasedAssessmentId);

        ShouldBeInFull(mark);
        mark.IsModerated.ShouldBeTrue();
    }
}
