using psms.Admissions.Enrollments;
using psms.Authorization;
using psms.Authorization.Roles;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// A place is given, not taken.
/// <para>
/// Acceptance was inferred: the mapper reported every approved application as
/// an accepted offer, dated the day the school itself decided. The school's own
/// queue said a family had said yes when nobody had asked them, and enrolment —
/// which creates a learner record and a portal sign-in for somebody's child —
/// ran off that. <c>AcceptOfferAsync</c> meanwhile validated the offer with some
/// care and then saved nothing at all.
/// </para>
/// <para>
/// These pin the three things the screens now rely on: that a yes is recorded
/// rather than deduced, that a no ends the application and frees the place, and
/// that both the family and the school can reach the call.
/// </para>
/// </summary>
public class AFamilySaysYesOrNo_Tests
{
    private static Application Offered(int expiresInDays = 14)
    {
        var application = new Application(
            Guid.NewGuid(),
            1,
            "APP-001-2026-00001",
            "Zodwa",
            "Mokoena",
            new DateTime(2015, 3, 14),
            Gender.Female,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "parent@example.test");

        application.Status = ApplicationStatus.Approved;
        application.Decision = AdmissionDecision.Accepted;
        application.ExpiryDate = DateTime.UtcNow.AddDays(expiresInDays);
        return application;
    }

    [Fact]
    public void An_approved_application_is_not_an_accepted_one()
    {
        var application = Offered();

        application.OfferAcceptedDate.ShouldBeNull();
        application.OfferDeclinedDate.ShouldBeNull();
    }

    [Fact]
    public void Saying_yes_is_recorded_against_whoever_said_it()
    {
        var application = Offered();

        application.AcceptOffer(acceptedByUserId: 42);

        application.OfferAcceptedDate.ShouldNotBeNull();
        application.OfferAcceptedByUserId.ShouldBe(42);
        // Still approved: the child is not on the roll until somebody enrols them.
        application.Status.ShouldBe(ApplicationStatus.Approved);
    }

    [Fact]
    public void Saying_yes_twice_does_not_move_the_date()
    {
        var application = Offered();
        application.AcceptOffer(1);
        var first = application.OfferAcceptedDate;

        application.AcceptOffer(2);

        application.OfferAcceptedDate.ShouldBe(first);
        application.OfferAcceptedByUserId.ShouldBe(1);
    }

    [Fact]
    public void Saying_no_ends_the_application_and_gives_the_place_back()
    {
        var application = Offered();

        application.DeclineOffer("They have taken a place at another school.");

        application.OfferDeclinedDate.ShouldNotBeNull();
        application.OfferDeclineReason.ShouldBe("They have taken a place at another school.");
        // No longer Approved, so it stops being counted against the grade's places.
        application.Status.ShouldBe(ApplicationStatus.Withdrawn);
    }

    [Fact]
    public void A_reason_is_not_demanded()
    {
        var application = Offered();

        Should.NotThrow(() => application.DeclineOffer());
        application.OfferDeclinedDate.ShouldNotBeNull();
    }

    [Fact]
    public void The_two_answers_do_not_overwrite_each_other()
    {
        var accepted = Offered();
        accepted.AcceptOffer(1);
        Should.Throw<InvalidOperationException>(() => accepted.DeclineOffer("changed our minds"));

        var declined = Offered();
        declined.DeclineOffer();
        Should.Throw<InvalidOperationException>(() => declined.AcceptOffer(1));
    }

    [Fact]
    public void An_expired_offer_cannot_be_accepted()
    {
        var application = Offered(expiresInDays: -1);

        Should.Throw<InvalidOperationException>(() => application.AcceptOffer(1));
    }

    [Theory]
    [InlineData(ApplicationStatus.UnderReview)]
    [InlineData(ApplicationStatus.Waitlisted)]
    [InlineData(ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Enrolled)]
    public void There_is_nothing_to_answer_without_an_offer(ApplicationStatus status)
    {
        var application = Offered();
        application.Status = status;

        Should.Throw<InvalidOperationException>(() => application.AcceptOffer(1));
        Should.Throw<InvalidOperationException>(() => application.DeclineOffer());
    }

    [Fact]
    public void The_family_may_reach_both_calls()
    {
        var applicant = PsmsRolePermissionSeeder.ApplicantPermissions();

        applicant.ShouldContain(PermissionNames.Admissions_Enrollment_AcceptOffer);
        applicant.ShouldContain(PermissionNames.Admissions_Enrollment_DeclineOffer);
    }

    [Fact]
    public void So_may_the_staff_who_enrol_people()
    {
        /* A family without email telephones instead, and somebody has to be able
           to write that down — otherwise requiring acceptance before enrolment
           would strand them. Whoever may finish an enrolment may record the
           answer that allows it. */
        var methods = typeof(IEnrollmentAppService);

        methods.GetMethod(nameof(IEnrollmentAppService.AcceptOfferAsync)).ShouldNotBeNull();
        methods.GetMethod(nameof(IEnrollmentAppService.DeclineOfferAsync)).ShouldNotBeNull();

        var seeder = typeof(PsmsRolePermissionSeeder);
        var principal = (System.Collections.Generic.List<string>)seeder
            .GetMethod("GetPrincipalPermissions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Invoke(null, null);

        principal.ShouldContain(PermissionNames.Admissions_Enrollment_Complete);
        principal.ShouldContain(PermissionNames.Admissions_Enrollment_AcceptOffer);
    }
}
