using Abp.Authorization.Users;
using Abp.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.Communication.AnnouncementReads;
using psms.Communication.Announcements;
using psms.Communication.Announcements.Dto;
using psms.Communication.Shared;
using psms.Domain.Academic.Entities;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Communication;

/// <summary>
/// #203: a parent or learner reads the announcements addressed to them, and
/// nothing else. Before this, both roles were given every announcement in the
/// school: drafts, staff and teacher notices, other classes' notices, and ones
/// not yet due or already expired. The unread badge counted all of them. Staff
/// reads are unchanged.
/// </summary>
public class FamilyAnnouncements_Tests : psmsTestBase
{
    private readonly IAnnouncementAppService _announcements;
    private readonly IAnnouncementReadAppService _reads;

    private long _parentUserId;
    private long _studentUserId;
    private readonly Dictionary<string, Guid> _ids = new();

    public FamilyAnnouncements_Tests()
    {
        _announcements = Resolve<IAnnouncementAppService>();
        _reads = Resolve<IAnnouncementReadAppService>();
        Seed();
    }

    private void Seed()
    {
        var parentUser = CreateUser("ann.parent");
        var studentUser = CreateUser("ann.student");
        _parentUserId = parentUser.Id;
        _studentUserId = studentUser.Id;
        AssignRole(parentUser.Id, StaticRoleNames.Tenants.Parent);
        AssignRole(studentUser.Id, StaticRoleNames.Tenants.Student);

        var academicYearId = Guid.NewGuid();
        var grade3Id = Guid.NewGuid();
        var grade8Id = Guid.NewGuid();
        var class3AId = Guid.NewGuid();
        var class3BId = Guid.NewGuid();
        var class8AId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var parentId = Guid.NewGuid();

        UsingDbContext(1, context =>
        {
            context.AcademicYears.Add(new AcademicYear(academicYearId, 1, 2026, new DateTime(2026, 1, 1), new DateTime(2026, 12, 1)));
            context.Grades.Add(new Grade(grade3Id, 1, SouthAfricanGradeLevel.Grade3, "Grade 3", SouthAfricanSchoolPhase.Foundation));
            context.Grades.Add(new Grade(grade8Id, 1, SouthAfricanGradeLevel.Grade8, "Grade 8", SouthAfricanSchoolPhase.Senior));
            context.Classes.Add(new Class(class3AId, 1, "3A", grade3Id, academicYearId, 30));
            context.Classes.Add(new Class(class3BId, 1, "3B", grade3Id, academicYearId, 30));
            context.Classes.Add(new Class(class8AId, 1, "8A", grade8Id, academicYearId, 30));

            // The parent's child is in 3A. The learner signing in is in 8A, in another family.
            context.Students.Add(new Student(childId, 1, "Ayanda", "Nkosi", new DateTime(2018, 1, 1),
                Gender.Female, "A0001", DateTime.Today, grade3Id, class3AId));
            context.Students.Add(new Student(Guid.NewGuid(), 1, "Sipho", "Dube", new DateTime(2013, 1, 1),
                Gender.Male, "S0001", DateTime.Today, grade8Id, class8AId) { UserId = studentUser.Id });
            context.Parents.Add(new Parent(parentId, 1, parentUser.Id, "Parent", "Nkosi", "ann.parent@test.local", "0710000000"));
            context.StudentParents.Add(new StudentParent(Guid.NewGuid(), childId, parentId, RelationshipType.Guardian));

            var now = DateTime.UtcNow;
            Add(context, "all", AnnouncementAudience.All);
            Add(context, "parents", AnnouncementAudience.Parents);
            Add(context, "students", AnnouncementAudience.Students);
            Add(context, "staff", AnnouncementAudience.Staff);
            Add(context, "teachers", AnnouncementAudience.Teachers);
            Add(context, "grade3", AnnouncementAudience.Grade, a => a.TargetGradeId = grade3Id);
            Add(context, "grade8", AnnouncementAudience.Grade, a => a.TargetGradeId = grade8Id);
            Add(context, "class3A", AnnouncementAudience.Class, a => a.TargetClassId = class3AId);
            Add(context, "class3B", AnnouncementAudience.Class, a => a.TargetClassId = class3BId);
            Add(context, "draft", AnnouncementAudience.All, a => a.IsPublished = false);
            Add(context, "scheduled", AnnouncementAudience.All, a => a.PublishDate = now.AddDays(1));
            Add(context, "expired", AnnouncementAudience.All, a =>
            {
                a.PublishDate = now.AddDays(-10);
                a.ExpiryDate = now.AddDays(-1);
            });
            context.SaveChanges();
        });
    }

    private void Add(psms.EntityFrameworkCore.psmsDbContext context, string key, AnnouncementAudience audience,
        Action<Announcement> shape = null)
    {
        var id = Guid.NewGuid();
        _ids[key] = id;
        var announcement = new Announcement(id, 1, $"Notice: {key}", "Body", AnnouncementType.General, audience, 1)
        {
            IsPublished = true,
            PublishDate = DateTime.UtcNow.AddHours(-1),
        };
        shape?.Invoke(announcement);
        context.Announcements.Add(announcement);
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

    private async Task<string[]> FeedKeysAsync()
    {
        var page = await _announcements.GetAllAsync(new GetAnnouncementsInput { MaxResultCount = 100 });
        page.TotalCount.ShouldBe(page.Items.Count);
        return page.Items.Select(i => _ids.Single(kv => kv.Value == i.Id).Key).OrderBy(k => k).ToArray();
    }

    [Fact]
    public async Task A_parent_sees_everyone_parents_and_their_childs_grade_and_class()
    {
        LoginAs(_parentUserId);

        (await FeedKeysAsync()).ShouldBe(new[] { "all", "class3A", "grade3", "parents" });
    }

    [Fact]
    public async Task A_learner_sees_everyone_learners_and_their_own_grade()
    {
        LoginAs(_studentUserId);

        (await FeedKeysAsync()).ShouldBe(new[] { "all", "grade8", "students" });
    }

    [Fact]
    public async Task Staff_still_see_every_announcement()
    {
        LoginAsDefaultTenantAdmin();

        (await FeedKeysAsync()).Length.ShouldBe(_ids.Count);
    }

    [Theory]
    [InlineData("staff")]
    [InlineData("teachers")]
    [InlineData("students")]
    [InlineData("class3B")]
    [InlineData("draft")]
    [InlineData("scheduled")]
    [InlineData("expired")]
    public async Task A_parent_cannot_open_or_mark_one_not_addressed_to_them(string key)
    {
        LoginAs(_parentUserId);

        var opened = await Should.ThrowAsync<UserFriendlyException>(() => _announcements.GetAsync(_ids[key]));
        opened.Message.ShouldBe(CommunicationExceptionCodes.AnnouncementNotFound);

        var marked = await Should.ThrowAsync<UserFriendlyException>(() => _reads.MarkAsReadAsync(_ids[key]));
        marked.Message.ShouldBe(CommunicationExceptionCodes.AnnouncementNotFound);
    }

    [Fact]
    public async Task The_unread_count_is_the_parents_feed_and_falls_as_they_read()
    {
        LoginAs(_parentUserId);
        (await _reads.GetUnreadCountAsync()).ShouldBe(4);

        await _reads.MarkAsReadAsync(_ids["parents"]);

        (await _reads.GetUnreadCountAsync()).ShouldBe(3);
        (await _announcements.GetAsync(_ids["parents"])).IsReadByMe.ShouldBeTrue();
        (await _announcements.GetAsync(_ids["all"])).IsReadByMe.ShouldBeFalse();

        var feed = await _announcements.GetAllAsync(new GetAnnouncementsInput { MaxResultCount = 100 });
        feed.Items.Single(i => i.Id == _ids["parents"]).IsReadByMe.ShouldBeTrue();
        feed.Items.Where(i => i.Id != _ids["parents"]).ShouldAllBe(i => !i.IsReadByMe);
    }

    [Fact]
    public async Task Pinned_first_then_newest_is_a_sort_the_feed_accepts()
    {
        UsingDbContext(1, context =>
        {
            context.Announcements.Single(a => a.Id == _ids["grade3"]).IsPinned = true;
        });
        LoginAs(_parentUserId);

        var feed = await _announcements.GetAllAsync(new GetAnnouncementsInput
        {
            MaxResultCount = 100,
            Sorting = "IsPinned DESC, PublishDate DESC",
        });

        feed.Items.First().Id.ShouldBe(_ids["grade3"]);
    }
}
