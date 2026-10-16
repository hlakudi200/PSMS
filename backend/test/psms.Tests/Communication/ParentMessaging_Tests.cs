using Abp.Authorization.Users;
using Abp.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.Communication.Messages;
using psms.Communication.Messages.Dto;
using psms.Communication.Shared;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Communication;

/// <summary>
/// #204: a parent messages their children's teachers.
/// <para>
/// GetMyRecipients lists the class teacher and the subject teachers of each
/// child's class, worked out on the server. A parent may start a conversation
/// only with one of them, and may reply to anyone who wrote to them. A reply
/// stays inside a conversation the sender is part of, for every caller. Reads
/// carry names, not just user ids.
/// </para>
/// </summary>
public class ParentMessaging_Tests : psmsTestBase
{
    private readonly IMessageAppService _messages;

    private long _parentUserId;
    private long _otherParentUserId;
    private long _classTeacherUserId;
    private long _mathsTeacherUserId;
    private long _otherClassTeacherUserId;

    public ParentMessaging_Tests()
    {
        _messages = Resolve<IMessageAppService>();
        Seed();
    }

    private void Seed()
    {
        var parentUser = CreateUser("msg.parent", "Thandi", "Nkosi");
        var otherParentUser = CreateUser("msg.otherparent", "Other", "Parent");
        var classTeacherUser = CreateUser("msg.classteacher", "Lerato", "Mokoena");
        var mathsTeacherUser = CreateUser("msg.maths", "Pieter", "Botha");
        var otherClassTeacherUser = CreateUser("msg.otherclass", "Sizwe", "Dlamini");
        _parentUserId = parentUser.Id;
        _otherParentUserId = otherParentUser.Id;
        _classTeacherUserId = classTeacherUser.Id;
        _mathsTeacherUserId = mathsTeacherUser.Id;
        _otherClassTeacherUserId = otherClassTeacherUser.Id;
        AssignRole(parentUser.Id, StaticRoleNames.Tenants.Parent);
        AssignRole(otherParentUser.Id, StaticRoleNames.Tenants.Parent);
        AssignRole(classTeacherUser.Id, StaticRoleNames.Tenants.Teacher);
        AssignRole(mathsTeacherUser.Id, StaticRoleNames.Tenants.Teacher);
        AssignRole(otherClassTeacherUser.Id, StaticRoleNames.Tenants.Teacher);

        var academicYearId = Guid.NewGuid();
        var gradeId = Guid.NewGuid();
        var class3AId = Guid.NewGuid();
        var class3BId = Guid.NewGuid();
        var classTeacherId = Guid.NewGuid();
        var mathsTeacherId = Guid.NewGuid();
        var otherClassTeacherId = Guid.NewGuid();
        var mathsId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.AcademicYears.Add(new AcademicYear(academicYearId, 1, 2026, new DateTime(2026, 1, 1), new DateTime(2026, 12, 1)));
            context.Grades.Add(new Grade(gradeId, 1, SouthAfricanGradeLevel.Grade3, "Grade 3", SouthAfricanSchoolPhase.Foundation));
            context.Teachers.Add(new Teacher(classTeacherId, 1, classTeacherUser.Id, "Lerato", "Mokoena", "T001", "msg.classteacher@test.local"));
            context.Teachers.Add(new Teacher(mathsTeacherId, 1, mathsTeacherUser.Id, "Pieter", "Botha", "T002", "msg.maths@test.local"));
            context.Teachers.Add(new Teacher(otherClassTeacherId, 1, otherClassTeacherUser.Id, "Sizwe", "Dlamini", "T003", "msg.otherclass@test.local"));
            context.Classes.Add(new Class(class3AId, 1, "3A", gradeId, academicYearId, 30) { ClassTeacherId = classTeacherId });
            context.Classes.Add(new Class(class3BId, 1, "3B", gradeId, academicYearId, 30) { ClassTeacherId = otherClassTeacherId });
            context.Subjects.Add(new Subject(mathsId, 1, "Mathematics", "MATH", true));
            context.ClassSubjects.Add(new ClassSubject(Guid.NewGuid(), 1, class3AId, mathsId) { TeacherId = mathsTeacherId });
            // Teaches maths in the other class too, which must not add a second line for this family.
            context.ClassSubjects.Add(new ClassSubject(Guid.NewGuid(), 1, class3BId, mathsId) { TeacherId = otherClassTeacherId });

            context.Students.Add(new Student(childId, 1, "Ayanda", "Nkosi", new DateTime(2018, 1, 1),
                Gender.Female, "M0001", DateTime.Today, gradeId, class3AId));
            context.Parents.Add(new Parent(parentId, 1, parentUser.Id, "Thandi", "Nkosi", "msg.parent@test.local", "0710000000"));
            context.StudentParents.Add(new StudentParent(Guid.NewGuid(), childId, parentId, RelationshipType.Mother));
            context.Parents.Add(new Parent(Guid.NewGuid(), 1, otherParentUser.Id, "Other", "Parent", "msg.otherparent@test.local", "0720000000"));
            context.SaveChanges();
        });
    }

    private User CreateUser(string userName, string name, string surname)
    {
        return UsingDbContext(context =>
        {
            var user = new User
            {
                TenantId = 1,
                UserName = userName,
                Name = name,
                Surname = surname,
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

    private static CreateMessageDto To(long userId, string content = "Hello", Guid? replyTo = null) =>
        new() { RecipientUserId = userId, Subject = "About Ayanda", Content = content, ParentMessageId = replyTo };

    // ─── Who a parent can write to ───

    [Fact]
    public async Task A_parent_is_offered_their_childs_class_teacher_and_subject_teachers()
    {
        LoginAs(_parentUserId);

        var recipients = (await _messages.GetMyRecipientsAsync()).Items;

        recipients.Select(r => r.UserId).ShouldBe(new[] { _classTeacherUserId, _mathsTeacherUserId }, ignoreOrder: true);
        recipients.Single(r => r.UserId == _classTeacherUserId).Roles.ShouldBe(new[] { "Class teacher, 3A (Ayanda)" });
        recipients.Single(r => r.UserId == _mathsTeacherUserId).Roles.ShouldBe(new[] { "Mathematics, 3A (Ayanda)" });
        recipients.Single(r => r.UserId == _mathsTeacherUserId).Name.ShouldBe("Pieter Botha");
    }

    [Fact]
    public async Task Staff_are_offered_nobody_here()
    {
        LoginAsDefaultTenantAdmin();

        (await _messages.GetMyRecipientsAsync()).Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task A_parent_can_write_to_their_childs_teacher()
    {
        LoginAs(_parentUserId);

        var sent = await _messages.SendAsync(To(_mathsTeacherUserId));

        sent.RecipientUserId.ShouldBe(_mathsTeacherUserId);
        sent.SenderName.ShouldBe("Thandi Nkosi");
        sent.RecipientName.ShouldBe("Pieter Botha");
    }

    [Fact]
    public async Task A_parent_cannot_start_a_conversation_with_another_teacher_or_parent()
    {
        LoginAs(_parentUserId);

        foreach (var userId in new[] { _otherClassTeacherUserId, _otherParentUserId })
        {
            var refusal = await Should.ThrowAsync<UserFriendlyException>(() => _messages.SendAsync(To(userId)));
            refusal.Message.ShouldBe(CommunicationExceptionCodes.RecipientNotAllowed);
        }
    }

    [Fact]
    public async Task A_parent_can_reply_to_anyone_who_wrote_to_them()
    {
        LoginAsDefaultTenantAdmin();
        var adminId = AbpSession.UserId.Value;
        var fromSchool = await _messages.SendAsync(To(_parentUserId, "Please call the office."));

        LoginAs(_parentUserId);
        var reply = await _messages.SendAsync(To(adminId, "Will do.", fromSchool.Id));

        reply.ThreadId.ShouldBe(fromSchool.ThreadId);
        var thread = (await _messages.GetThreadAsync(fromSchool.ThreadId.Value)).Items;
        thread.Select(m => m.Content).ShouldBe(new[] { "Please call the office.", "Will do." });
    }

    // ─── Replies stay in their conversation ───

    [Fact]
    public async Task Nobody_can_reply_into_a_conversation_they_are_not_in()
    {
        LoginAs(_classTeacherUserId);
        var between = await _messages.SendAsync(To(_otherClassTeacherUserId, "Staff matter."));

        LoginAs(_parentUserId);
        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _messages.SendAsync(To(_otherClassTeacherUserId, "Hi", between.Id)));
        refusal.Message.ShouldBe(CommunicationExceptionCodes.ReplyOutsideThread);
    }

    [Fact]
    public async Task A_reply_goes_to_the_other_person_in_the_conversation()
    {
        LoginAs(_parentUserId);
        var first = await _messages.SendAsync(To(_classTeacherUserId));

        // Replying to their own message, but addressing someone else.
        var refusal = await Should.ThrowAsync<UserFriendlyException>(
            () => _messages.SendAsync(To(_otherParentUserId, "Hi", first.Id)));
        refusal.Message.ShouldBe(CommunicationExceptionCodes.ReplyOutsideThread);

        LoginAs(_classTeacherUserId);
        var answer = await _messages.SendAsync(To(_parentUserId, "Happy to help.", first.Id));
        answer.ThreadId.ShouldBe(first.ThreadId);
    }

    // ─── Names on reads ───

    [Fact]
    public async Task The_inbox_and_sent_lists_carry_names()
    {
        LoginAs(_parentUserId);
        await _messages.SendAsync(To(_classTeacherUserId));

        var sent = (await _messages.GetSentAsync(new GetMessagesInput { MaxResultCount = 10 })).Items.ShouldHaveSingleItem();
        sent.RecipientName.ShouldBe("Lerato Mokoena");

        LoginAs(_classTeacherUserId);
        var received = (await _messages.GetInboxAsync(new GetMessagesInput { MaxResultCount = 10 })).Items.ShouldHaveSingleItem();
        received.SenderName.ShouldBe("Thandi Nkosi");
        received.IsRead.ShouldBeFalse();
    }
}
