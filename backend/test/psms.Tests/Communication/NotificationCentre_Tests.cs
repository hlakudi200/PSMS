using Abp.Authorization.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using psms.Authorization.Roles;
using psms.Authorization.Users;
using psms.Communication.Notifications;
using psms.Communication.Notifications.Dto;
using psms.Domain.Communication.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Communication;

/// <summary>
/// #201: what the mobile notification centre reads. The list carries what each
/// notification is about (EntityType, EntityId), so a report card notification,
/// which deliberately has no ActionUrl, can still open the card. A parent sees
/// only their own notifications, and marking all read leaves nobody else's.
/// </summary>
public class NotificationCentre_Tests : psmsTestBase
{
    private readonly INotificationAppService _notifications;
    private long _parentUserId;
    private long _otherParentUserId;
    private readonly Guid _reportId = Guid.NewGuid();

    public NotificationCentre_Tests()
    {
        _notifications = Resolve<INotificationAppService>();

        _parentUserId = CreateUser("nc.parent").Id;
        _otherParentUserId = CreateUser("nc.otherparent").Id;
        AssignRole(_parentUserId, StaticRoleNames.Tenants.Parent);
        AssignRole(_otherParentUserId, StaticRoleNames.Tenants.Parent);

        UsingDbContext(1, context =>
        {
            var report = new Notification(Guid.NewGuid(), 1, _parentUserId,
                "Term 3 report available for Ayanda", "Ayanda's term 3 report has been published.", NotificationType.Academic);
            report.LinkToEntity(nameof(psms.Domain.Assessment.Entities.Report), _reportId);
            context.Notifications.Add(report);
            context.Notifications.Add(new Notification(Guid.NewGuid(), 1, _parentUserId,
                "Sports day", "Sports day is on Friday.", NotificationType.General));
            context.Notifications.Add(new Notification(Guid.NewGuid(), 1, _otherParentUserId,
                "Someone else's", "Not for this parent.", NotificationType.General));
            context.SaveChanges();
        });
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

    [Fact]
    public async Task The_list_says_what_a_notification_is_about()
    {
        LoginAs(_parentUserId);

        var items = (await _notifications.GetAllAsync(new GetNotificationsInput { MaxResultCount = 10 })).Items;

        items.Count.ShouldBe(2);
        var report = items.Single(n => n.Title.StartsWith("Term 3 report"));
        report.EntityType.ShouldBe("Report");
        report.EntityId.ShouldBe(_reportId);
        report.ActionUrl.ShouldBeNull();
        items.Single(n => n.Title == "Sports day").EntityType.ShouldBeNull();
    }

    [Fact]
    public async Task Marking_all_read_clears_only_the_callers_own()
    {
        LoginAs(_parentUserId);
        (await _notifications.GetUnreadCountAsync()).ShouldBe(2);

        await _notifications.MarkAllAsReadAsync();

        (await _notifications.GetUnreadCountAsync()).ShouldBe(0);
        LoginAs(_otherParentUserId);
        (await _notifications.GetUnreadCountAsync()).ShouldBe(1);
    }
}
