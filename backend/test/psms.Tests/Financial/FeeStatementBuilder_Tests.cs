using System;
using System.Collections.Generic;
using System.Linq;
using psms.Domain.Shared.Enums;
using psms.Financial.Statements;
using psms.Financial.Statements.Dto;
using Shouldly;
using Xunit;

namespace psms.Tests.Financial;

/// <summary>
/// MOB-BE-01 (#215): the statement arithmetic, on its own — opening balance,
/// period movements, running balance, overdue, and what a parent still owes.
/// </summary>
public class FeeStatementBuilder_Tests
{
    private static readonly Guid Ann = Guid.NewGuid();
    private static readonly Guid Ben = Guid.NewGuid();

    private static readonly List<StatementStudent> Family = new()
    {
        new StatementStudent(Ann, "Ann Adams", "A001", "Grade 8"),
        new StatementStudent(Ben, "Ben Adams", "A002", "Grade 5"),
    };

    // Term 2 of 2026.
    private static readonly StatementPeriod Term2 =
        new(new DateTime(2026, 4, 1), new DateTime(2026, 6, 30), "Term 2 · 2026");

    private static readonly DateTime Today = new(2026, 10, 11);

    private static StatementFee Fee(Guid student, string name, DateTime chargedOn, DateTime dueDate,
        decimal amount, decimal discount = 0m, FeeStatus status = FeeStatus.Pending, DateTime? waivedOn = null)
        => new(Guid.NewGuid(), student, name, chargedOn, dueDate, amount, discount, status, waivedOn, "ZAR");

    private static StatementPayment Paid(Guid student, DateTime on, decimal amount, string receipt,
        PaymentStatus status = PaymentStatus.Completed, DateTime? refundedOn = null,
        params (StatementFee fee, decimal amount)[] allocations)
        => new(Guid.NewGuid(), student, on, amount, receipt, status, refundedOn,
            allocations.Select(a => new StatementAllocation(a.fee.Id, a.amount, on)).ToList());

    [Fact]
    public void Opening_balance_is_everything_before_the_period_and_closing_adds_the_period()
    {
        var term1 = Fee(Ann, "Term 1 tuition", new DateTime(2026, 1, 10), new DateTime(2026, 2, 1), 3000m);
        var term2 = Fee(Ann, "Term 2 tuition", new DateTime(2026, 4, 5), new DateTime(2026, 5, 1), 3000m);
        var payments = new[]
        {
            Paid(Ann, new DateTime(2026, 2, 1), 2000m, "R-1", allocations: (term1, 2000m)),
            Paid(Ann, new DateTime(2026, 5, 2), 2500m, "R-2", allocations: (term2, 2500m)),
        };

        var statement = FeeStatementBuilder.Build(Term2, Today, Family.Take(1).ToList(),
            new[] { term1, term2 }, payments);

        statement.OpeningBalance.ShouldBe(1000m);
        statement.TotalCharged.ShouldBe(3000m);
        statement.TotalPaid.ShouldBe(2500m);
        statement.ClosingBalance.ShouldBe(1500m);

        statement.Lines.Select(l => l.Type).ShouldBe(new[] { FeeStatementLineType.Charge, FeeStatementLineType.Payment });
        statement.Lines.Select(l => l.Balance).ShouldBe(new[] { 4000m, 1500m });
        statement.Lines[1].Reference.ShouldBe("R-2");
        statement.Lines[1].Allocations.Single().FeeName.ShouldBe("Term 2 tuition");
    }

    [Fact]
    public void A_fee_lands_on_the_day_it_was_billed_and_says_when_it_is_due()
    {
        // Billed in March, due in April: it belongs to Term 1's statement.
        var fee = Fee(Ann, "Camp", new DateTime(2026, 3, 20), new DateTime(2026, 4, 15), 800m);

        var statement = FeeStatementBuilder.Build(Term2, Today, Family.Take(1).ToList(), new[] { fee },
            Array.Empty<StatementPayment>());

        statement.OpeningBalance.ShouldBe(800m);
        statement.Lines.ShouldBeEmpty();
        statement.ClosingBalance.ShouldBe(800m);
    }

    [Fact]
    public void Cancelled_fees_are_left_out_and_discounts_and_waivers_are_credits()
    {
        var cancelled = Fee(Ann, "Trip", new DateTime(2026, 4, 2), new DateTime(2026, 4, 30), 500m, status: FeeStatus.Cancelled);
        var discounted = Fee(Ann, "Tuition", new DateTime(2026, 4, 3), new DateTime(2026, 4, 30), 3000m, discount: 300m);
        var waived = Fee(Ann, "Sport levy", new DateTime(2026, 4, 4), new DateTime(2026, 4, 30), 400m,
            status: FeeStatus.Waived, waivedOn: new DateTime(2026, 5, 10));
        // R100 was paid on the levy before the rest of it was waived.
        var payment = Paid(Ann, new DateTime(2026, 4, 20), 100m, "R-3", allocations: (waived, 100m));

        var statement = FeeStatementBuilder.Build(Term2, Today, Family.Take(1).ToList(),
            new[] { cancelled, discounted, waived }, new[] { payment });

        statement.TotalCharged.ShouldBe(3400m);
        statement.TotalCredits.ShouldBe(300m + 300m);
        statement.TotalPaid.ShouldBe(100m);
        statement.ClosingBalance.ShouldBe(2700m);
        statement.Lines.ShouldNotContain(l => l.Description.Contains("Trip"));
        statement.Lines.Single(l => l.Type == FeeStatementLineType.Waiver).Date.ShouldBe(new DateTime(2026, 5, 10));
    }

    [Fact]
    public void A_refunded_payment_nets_to_nothing_and_its_allocation_does_not_count_as_paid()
    {
        var fee = Fee(Ann, "Tuition", new DateTime(2026, 4, 1), new DateTime(2026, 4, 30), 3000m);
        var refunded = Paid(Ann, new DateTime(2026, 4, 10), 1000m, "R-4",
            PaymentStatus.Refunded, new DateTime(2026, 4, 20), (fee, 1000m));

        var statement = FeeStatementBuilder.Build(Term2, Today, Family.Take(1).ToList(), new[] { fee }, new[] { refunded });

        statement.TotalPaid.ShouldBe(1000m);
        statement.TotalRefunded.ShouldBe(1000m);
        statement.ClosingBalance.ShouldBe(3000m);
        statement.AmountOverdue.ShouldBe(3000m);
        statement.Children.Single().OutstandingFees.Single().Outstanding.ShouldBe(3000m);
    }

    [Fact]
    public void Overdue_is_only_what_was_past_due_and_never_more_than_the_balance()
    {
        var pastDue = Fee(Ann, "Tuition", new DateTime(2026, 4, 1), new DateTime(2026, 4, 30), 3000m);
        var notYetDue = Fee(Ann, "Exam fee", new DateTime(2026, 4, 1), new DateTime(2026, 7, 31), 500m);
        // R1000 paid in but not yet put against any fee.
        var unallocated = Paid(Ann, new DateTime(2026, 6, 1), 1000m, "R-5");

        var statement = FeeStatementBuilder.Build(Term2, Today, Family.Take(1).ToList(),
            new[] { pastDue, notYetDue }, new[] { unallocated });

        // As at 30 June: tuition is overdue, the exam fee is not due yet.
        statement.StatementDate.ShouldBe(new DateTime(2026, 6, 30));
        statement.ClosingBalance.ShouldBe(2500m);
        statement.AmountOverdue.ShouldBe(2500m);
    }

    [Fact]
    public void A_family_statement_totals_its_children_and_runs_one_balance()
    {
        var annFee = Fee(Ann, "Tuition", new DateTime(2026, 4, 1), new DateTime(2026, 4, 30), 3000m);
        var benFee = Fee(Ben, "Tuition", new DateTime(2026, 4, 1), new DateTime(2026, 4, 30), 2000m);
        var benPaid = Paid(Ben, new DateTime(2026, 4, 15), 2000m, "R-6", allocations: (benFee, 2000m));

        var statement = FeeStatementBuilder.Build(Term2, Today, Family, new[] { annFee, benFee }, new[] { benPaid });

        statement.Children.Select(c => c.StudentName).ShouldBe(new[] { "Ann Adams", "Ben Adams" });
        statement.Children.Single(c => c.StudentId == Ann).ClosingBalance.ShouldBe(3000m);
        statement.Children.Single(c => c.StudentId == Ben).ClosingBalance.ShouldBe(0m);
        statement.ClosingBalance.ShouldBe(3000m);
        statement.Lines.Last().Balance.ShouldBe(3000m);
        statement.Children.Single(c => c.StudentId == Ben).OutstandingFees.ShouldBeEmpty();
    }

    [Fact]
    public void Outstanding_fees_are_as_of_today_whatever_the_period()
    {
        var later = Fee(Ann, "Term 4 tuition", new DateTime(2026, 9, 1), new DateTime(2026, 11, 1), 3000m);

        var statement = FeeStatementBuilder.Build(Term2, Today, Family.Take(1).ToList(), new[] { later },
            Array.Empty<StatementPayment>());

        // Not on a Term 2 statement, but still something to pay.
        statement.ClosingBalance.ShouldBe(0m);
        var outstanding = statement.Children.Single().OutstandingFees.Single();
        outstanding.Outstanding.ShouldBe(3000m);
        outstanding.IsOverdue.ShouldBeFalse();
    }
}
