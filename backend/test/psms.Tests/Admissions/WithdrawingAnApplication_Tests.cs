using psms.Admissions.Applications;
using psms.Authorization;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// A parent changing their mind.
/// <para>
/// They moved town, they took a place somewhere else, or they started one by
/// mistake. The endpoint and the rule had been written; nothing on any screen
/// called them, so the only way to withdraw was to phone the school and hope
/// somebody did it by hand.
/// </para>
/// <para>
/// These pin the two things the UI relies on: that an applicant may reach the
/// call at all, and exactly which statuses the entity will and will not let go
/// of — because the apply page decides whether to offer the button from its own
/// copy of that rule, and the two must agree.
/// </para>
/// </summary>
public class WithdrawingAnApplication_Tests
{
    private static Application AtStatus(ApplicationStatus status)
    {
        var application = new Application(
            Guid.NewGuid(),
            1,
            "APP-001-2026-00001",
            "Thandiwe",
            "Mokoena",
            new DateTime(2019, 4, 2),
            Gender.Female,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "parent@example.test");

        application.Status = status;
        return application;
    }

    [Fact]
    public void An_applicant_holds_the_permission_it_asks_for()
    {
        var guard = typeof(ApplicationAppService)
            .GetMethod(nameof(ApplicationAppService.WithdrawAsync))
            .GetCustomAttribute<Abp.Authorization.AbpAuthorizeAttribute>()
            ?.Permissions?.FirstOrDefault();

        guard.ShouldBe(PermissionNames.Admissions_Applications_Withdraw);
        PsmsRolePermissionSeeder.ApplicantPermissions()
            .ShouldContain(PermissionNames.Admissions_Applications_Withdraw);
    }

    [Theory]
    [InlineData(ApplicationStatus.Draft)]
    [InlineData(ApplicationStatus.Submitted)]
    [InlineData(ApplicationStatus.PaymentPending)]
    [InlineData(ApplicationStatus.UnderReview)]
    [InlineData(ApplicationStatus.DocumentsRequired)]
    [InlineData(ApplicationStatus.InterviewScheduled)]
    [InlineData(ApplicationStatus.AssessmentScheduled)]
    [InlineData(ApplicationStatus.UnderConsideration)]
    [InlineData(ApplicationStatus.Approved)]
    [InlineData(ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Waitlisted)]
    public void Can_be_taken_back_right_up_to_enrolment(ApplicationStatus status)
    {
        var application = AtStatus(status);

        application.Withdraw("We have taken a place elsewhere.");

        application.Status.ShouldBe(ApplicationStatus.Withdrawn);
        application.DecisionReason.ShouldBe("We have taken a place elsewhere.");
    }

    [Fact]
    public void But_not_once_the_learner_is_enrolled()
    {
        // At that point this is no longer an application; it is a child at the
        // school, and leaving is a different conversation.
        Should.Throw<InvalidOperationException>(() => AtStatus(ApplicationStatus.Enrolled).Withdraw());
    }

    [Theory]
    [InlineData(ApplicationStatus.Withdrawn)]
    [InlineData(ApplicationStatus.Expired)]
    public void And_not_twice(ApplicationStatus status)
    {
        Should.Throw<InvalidOperationException>(() => AtStatus(status).Withdraw());
    }

    [Fact]
    public void A_reason_is_optional()
    {
        var application = AtStatus(ApplicationStatus.Submitted);

        application.Withdraw();

        application.Status.ShouldBe(ApplicationStatus.Withdrawn);
        application.DecisionReason.ShouldBeNull();
    }
}
