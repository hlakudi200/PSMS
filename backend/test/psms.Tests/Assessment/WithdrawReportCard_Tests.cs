using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// Taking an issued report card back.
/// <para>
/// A published card could not be deleted — right, it is the school's legal
/// record of the learner's year (§25(3)) — and nothing else reversed a publish
/// either. So a school that issued a card with the wrong marks on it, or under
/// the wrong learner's name, had no recourse at all: the card stayed issued,
/// the verification page went on confirming it as genuine, and the only remedy
/// was to issue a second card contradicting the first.
/// </para>
/// <para>
/// Withdrawing returns it to Approved — off the family's list, no longer
/// confirmed, correctable, and issuable again once it is right.
/// </para>
/// </summary>
public class WithdrawReportCard_Tests
{
    private const long Principal = 7;

    private static Report Issued()
    {
        var report = new Report(Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), ReportType.Term1);
        report.Generate();
        report.SubmitForApproval();
        report.Approve(Principal);
        report.Publish();
        return report;
    }

    [Fact]
    public void An_issued_card_goes_back_to_approved()
    {
        var report = Issued();

        report.Withdraw(Principal, "The Mathematics mark was captured against the wrong learner.");

        report.Status.ShouldBe(ReportStatus.Approved);
    }

    [Fact]
    public void The_withdrawal_is_stamped_with_who_and_why()
    {
        var report = Issued();

        report.Withdraw(Principal, "The Mathematics mark was captured against the wrong learner.");

        report.WithdrawnDate.ShouldNotBeNull();
        report.WithdrawnByUserId.ShouldBe(Principal);
        report.WithdrawalReason.ShouldBe("The Mathematics mark was captured against the wrong learner.");
        report.WasWithdrawn().ShouldBeTrue();
    }

    [Fact]
    public void The_day_it_was_issued_survives_the_withdrawal()
    {
        // The card was issued. Taking it back does not unmake that, and a
        // record that pretended otherwise would be the wrong kind of tidy.
        var report = Issued();
        var issuedOn = report.PublishedDate;

        report.Withdraw(Principal, "Wrong term printed on the card.");

        report.PublishedDate.ShouldBe(issuedOn);
    }

    [Fact]
    public void A_withdrawal_has_to_say_why()
    {
        var report = Issued();

        Should.Throw<ArgumentException>(() => report.Withdraw(Principal, "   "));
        Should.Throw<ArgumentException>(() => report.Withdraw(Principal, null));

        report.Status.ShouldBe(ReportStatus.Published, "a refused withdrawal changes nothing");
    }

    [Fact]
    public void A_reason_is_trimmed_and_capped_at_the_column_length()
    {
        var report = Issued();

        report.Withdraw(Principal, "  spacing  ");
        report.WithdrawalReason.ShouldBe("spacing");

        var again = Issued();
        again.Withdraw(Principal, new string('x', Report.MaxWithdrawalReasonLength + 50));
        again.WithdrawalReason.Length.ShouldBe(Report.MaxWithdrawalReasonLength);
    }

    [Fact]
    public void A_card_that_was_never_issued_cannot_be_withdrawn()
    {
        // There is nothing to take back. A card the school is still working on
        // is corrected, not withdrawn.
        var draft = new Report(Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), ReportType.Term1);

        Should.Throw<InvalidOperationException>(
            () => draft.Withdraw(Principal, "Nothing to withdraw."));
    }

    [Fact]
    public void An_approved_card_cannot_be_withdrawn_either()
    {
        var approved = new Report(Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), ReportType.Term1);
        approved.Generate();
        approved.SubmitForApproval();
        approved.Approve(Principal);

        Should.Throw<InvalidOperationException>(
            () => approved.Withdraw(Principal, "Not issued yet."));
    }

    [Fact]
    public void Withdrawing_twice_is_refused()
    {
        var report = Issued();
        report.Withdraw(Principal, "First.");

        Should.Throw<InvalidOperationException>(() => report.Withdraw(Principal, "Second."));
    }

    [Fact]
    public void A_withdrawn_card_can_be_corrected_and_issued_again()
    {
        var report = Issued();
        report.Withdraw(Principal, "Wrong marks.");

        report.Publish();

        report.Status.ShouldBe(ReportStatus.Published);
    }

    [Fact]
    public void Issuing_it_again_settles_the_earlier_withdrawal()
    {
        // The card in the family's hands is this one. Leaving the old
        // withdrawal stamped on it would have the screen saying a currently
        // issued card had been taken back.
        var report = Issued();
        report.Withdraw(Principal, "Wrong marks.");

        report.Publish();

        report.WasWithdrawn().ShouldBeFalse();
        report.WithdrawnDate.ShouldBeNull();
        report.WithdrawnByUserId.ShouldBeNull();
        report.WithdrawalReason.ShouldBeNull();
    }

    [Fact]
    public void A_card_that_was_never_withdrawn_says_so()
    {
        Issued().WasWithdrawn().ShouldBeFalse();
    }
}
