using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// Not every school charges to look at an application.
/// <para>
/// Submitting had one destination: PaymentPending. A school that charges
/// nothing would have had every application sit there waiting for money nobody
/// had asked for, and — because paying is what started the admissions workflow
/// — never reach an admissions officer at all.
/// </para>
/// </summary>
public class ApplicationFeeIsOptional_Tests
{
    private static Domain.Admissions.Entities.AdmissionSettings Settings(decimal amount, bool required)
    {
        var settings = new Domain.Admissions.Entities.AdmissionSettings(
            Guid.NewGuid(), 1, Guid.NewGuid(), amount);
        settings.IsApplicationFeeRequired = required;
        return settings;
    }

    private static Application Draft() => new Application(
        Guid.NewGuid(), 1, "APP-2026-00001", "Ayanda", "Nkosi",
        new DateTime(2019, 4, 2), Gender.Female, Guid.NewGuid(), Guid.NewGuid(),
        "parent@example.test");

    /* ─────────── what the school charges ─────────── */

    [Fact]
    public void A_school_that_charges_requires_the_fee()
    {
        Settings(500m, required: true).RequiresApplicationFee().ShouldBeTrue();
    }

    [Fact]
    public void A_school_that_has_turned_it_off_does_not()
    {
        Settings(500m, required: false).RequiresApplicationFee().ShouldBeFalse();
    }

    [Fact]
    public void Turning_it_off_keeps_the_amount()
    {
        // So a school waiving the fee for a year does not have to retype what
        // it costs when they turn it back on.
        var settings = Settings(500m, required: false);

        settings.ApplicationFeeAmount.ShouldBe(500m);
    }

    [Fact]
    public void A_fee_of_nothing_is_not_a_fee_whatever_the_flag_says()
    {
        // Otherwise an application waits in PaymentPending for a payment of
        // R0.00 that no screen can take.
        Settings(0m, required: true).RequiresApplicationFee().ShouldBeFalse();
    }

    [Fact]
    public void New_settings_charge_only_where_an_amount_was_given()
    {
        new Domain.Admissions.Entities.AdmissionSettings(Guid.NewGuid(), 1, Guid.NewGuid(), 450m)
            .IsApplicationFeeRequired.ShouldBeTrue();

        new Domain.Admissions.Entities.AdmissionSettings(Guid.NewGuid(), 1, Guid.NewGuid(), 0m)
            .IsApplicationFeeRequired.ShouldBeFalse();
    }

    /* ─────────── where a submitted application lands ─────────── */

    [Fact]
    public void Where_there_is_a_fee_a_submitted_application_waits_for_it()
    {
        var application = Draft();

        application.Submit(feeRequired: true);

        application.Status.ShouldBe(ApplicationStatus.PaymentPending);
    }

    [Fact]
    public void Where_there_is_none_it_goes_straight_to_review()
    {
        var application = Draft();

        application.Submit(feeRequired: false);

        application.Status.ShouldBe(ApplicationStatus.UnderReview);
    }

    [Fact]
    public void Either_way_it_is_stamped_with_when_it_was_handed_in()
    {
        var paid = Draft();
        var free = Draft();

        paid.Submit(feeRequired: true);
        free.Submit(feeRequired: false);

        paid.SubmissionDate.ShouldNotBeNull();
        free.SubmissionDate.ShouldNotBeNull();
    }

    [Fact]
    public void Only_a_draft_can_be_submitted_either_way()
    {
        var application = Draft();
        application.Submit(feeRequired: false);

        Should.Throw<InvalidOperationException>(() => application.Submit(feeRequired: false));
        Should.Throw<InvalidOperationException>(() => application.Submit(feeRequired: true));
    }

    [Fact]
    public void A_fee_free_application_cannot_then_be_marked_paid()
    {
        // It never owed anything. MarkPaymentReceived is the PaymentPending
        // transition and nothing else, and a receipt against an application
        // that was never charged would be a false record.
        var application = Draft();
        application.Submit(feeRequired: false);

        Should.Throw<InvalidOperationException>(() => application.MarkPaymentReceived());
    }
}
