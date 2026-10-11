using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using psms.Domain.Shared.Enums;
using psms.Financial.Statements.Dto;

namespace psms.Financial.Statements;

/// <summary>A learner the statement is drawn for.</summary>
public record StatementStudent(Guid Id, string Name, string AdmissionNumber, string GradeName);

/// <summary>
/// A fee as the statement needs it. <paramref name="ChargedOn"/> is when the
/// school billed it, which is when it lands on the statement; the due date only
/// decides whether it is overdue.
/// </summary>
public record StatementFee(
    Guid Id,
    Guid StudentId,
    string FeeName,
    DateTime ChargedOn,
    DateTime DueDate,
    decimal AmountDue,
    decimal DiscountAmount,
    FeeStatus Status,
    DateTime? WaivedOn,
    string Currency);

public record StatementAllocation(Guid StudentFeeId, decimal Amount, DateTime AllocatedDate);

public record StatementPayment(
    Guid Id,
    Guid StudentId,
    DateTime PaymentDate,
    decimal Amount,
    string ReceiptNumber,
    PaymentStatus Status,
    DateTime? RefundedOn,
    IReadOnlyList<StatementAllocation> Allocations);

/// <summary>Inclusive by day at both ends.</summary>
public record StatementPeriod(DateTime Start, DateTime End, string Label);

/// <summary>
/// Turns a family's fees and payments into a statement. Pure: no database, no
/// clock other than the <c>today</c> it is given, so the arithmetic can be
/// tested on its own.
/// <para>
/// The ledger is built from what actually moved money:
/// <list type="bullet">
/// <item>a fee is a debit on the day it was billed; a cancelled fee never
/// happened and is left out;</item>
/// <item>a discount is a credit on the same day as its fee (the fee does not
/// record when it was given);</item>
/// <item>a waived fee is credited whatever was still unpaid when it was
/// waived;</item>
/// <item>a completed payment is a credit; a refunded one is a credit and then a
/// debit when it was refunded, so it nets to nothing. Pending, failed and
/// cancelled payments never reached the school and are left out.</item>
/// </list>
/// What a fee has been paid is summed from the allocations of completed
/// payments rather than read from <c>StudentFee.AmountPaid</c>: refunding a
/// payment does not reverse its allocations, so that column still counts money
/// the family got back.
/// </para>
/// </summary>
public static class FeeStatementBuilder
{
    public const string DefaultCurrency = "ZAR";

    private record Entry(
        DateTime Date,
        Guid StudentId,
        FeeStatementLineType Type,
        string Description,
        string Reference,
        decimal Debit,
        decimal Credit,
        List<FeeStatementAllocationDto> Allocations);

    public static FeeStatementDto Build(
        StatementPeriod period,
        DateTime today,
        IReadOnlyList<StatementStudent> students,
        IReadOnlyList<StatementFee> fees,
        IReadOnlyList<StatementPayment> payments)
    {
        var start = period.Start.Date;
        var end = period.End.Date;
        // A statement cannot speak for days that have not happened yet.
        var statementDate = end < today.Date ? end : today.Date;

        var feesById = fees.ToDictionary(f => f.Id);
        var studentsById = students.ToDictionary(s => s.Id);

        var counted = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();
        var paidOnFee = counted
            .SelectMany(p => p.Allocations)
            .GroupBy(a => a.StudentFeeId)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.Amount));

        var entries = BuildEntries(fees, payments, feesById, paidOnFee);

        var statement = new FeeStatementDto
        {
            PeriodStart = start,
            PeriodEnd = end,
            PeriodLabel = period.Label,
            StatementDate = statementDate,
            Currency = fees.Select(f => f.Currency).FirstOrDefault(c => !string.IsNullOrWhiteSpace(c))
                ?? DefaultCurrency,
        };

        foreach (var student in students.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase))
        {
            var mine = entries.Where(e => e.StudentId == student.Id).ToList();
            var child = Summarise(student, mine, start, end);

            var studentFees = fees.Where(f => f.StudentId == student.Id).ToList();
            child.AmountOverdue = Overdue(studentFees, counted, statementDate, child.ClosingBalance);
            child.OutstandingFees = Outstanding(studentFees, paidOnFee, today.Date);

            statement.Children.Add(child);
        }

        statement.OpeningBalance = statement.Children.Sum(c => c.OpeningBalance);
        statement.TotalCharged = statement.Children.Sum(c => c.Charged);
        statement.TotalCredits = statement.Children.Sum(c => c.Credits);
        statement.TotalPaid = statement.Children.Sum(c => c.Paid);
        statement.TotalRefunded = statement.Children.Sum(c => c.Refunded);
        statement.ClosingBalance = statement.Children.Sum(c => c.ClosingBalance);
        statement.AmountOverdue = statement.Children.Sum(c => c.AmountOverdue);

        var running = statement.OpeningBalance;
        foreach (var entry in entries
                     .Where(e => e.Date.Date >= start && e.Date.Date <= end)
                     .OrderBy(e => e.Date.Date)
                     .ThenBy(e => (int)e.Type)
                     .ThenBy(e => e.Date)
                     .ThenBy(e => studentsById.TryGetValue(e.StudentId, out var s) ? s.Name : string.Empty,
                         StringComparer.OrdinalIgnoreCase))
        {
            running += entry.Debit - entry.Credit;
            statement.Lines.Add(new FeeStatementLineDto
            {
                Date = entry.Date,
                StudentId = entry.StudentId,
                StudentName = studentsById.TryGetValue(entry.StudentId, out var s) ? s.Name : null,
                Type = entry.Type,
                Description = entry.Description,
                Reference = entry.Reference,
                Debit = entry.Debit,
                Credit = entry.Credit,
                Balance = running,
                Allocations = entry.Allocations,
            });
        }

        return statement;
    }

    private static List<Entry> BuildEntries(
        IReadOnlyList<StatementFee> fees,
        IReadOnlyList<StatementPayment> payments,
        IReadOnlyDictionary<Guid, StatementFee> feesById,
        IReadOnlyDictionary<Guid, decimal> paidOnFee)
    {
        var entries = new List<Entry>();

        foreach (var fee in fees.Where(f => f.Status != FeeStatus.Cancelled))
        {
            entries.Add(new Entry(fee.ChargedOn, fee.StudentId, FeeStatementLineType.Charge,
                $"{fee.FeeName} (due {fee.DueDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)})",
                null, fee.AmountDue, 0m, new()));

            var discount = Math.Min(fee.DiscountAmount, fee.AmountDue);
            if (discount > 0)
                entries.Add(new Entry(fee.ChargedOn, fee.StudentId, FeeStatementLineType.Discount,
                    $"Discount: {fee.FeeName}", null, 0m, discount, new()));

            if (fee.Status == FeeStatus.Waived)
            {
                var unpaid = fee.AmountDue - discount - paidOnFee.GetValueOrDefault(fee.Id);
                if (unpaid > 0)
                    entries.Add(new Entry(fee.WaivedOn ?? fee.ChargedOn, fee.StudentId, FeeStatementLineType.Waiver,
                        $"Waived: {fee.FeeName}", null, 0m, unpaid, new()));
            }
        }

        foreach (var payment in payments.Where(p =>
                     p.Status == PaymentStatus.Completed || p.Status == PaymentStatus.Refunded))
        {
            var allocations = payment.Allocations
                .Select(a => new FeeStatementAllocationDto
                {
                    StudentFeeId = a.StudentFeeId,
                    FeeName = feesById.TryGetValue(a.StudentFeeId, out var f) ? f.FeeName : null,
                    Amount = a.Amount,
                })
                .ToList();

            entries.Add(new Entry(payment.PaymentDate, payment.StudentId, FeeStatementLineType.Payment,
                "Payment received", payment.ReceiptNumber, 0m, payment.Amount, allocations));

            if (payment.Status == PaymentStatus.Refunded)
                entries.Add(new Entry(payment.RefundedOn ?? payment.PaymentDate, payment.StudentId,
                    FeeStatementLineType.Refund, "Payment refunded", payment.ReceiptNumber,
                    payment.Amount, 0m, new()));
        }

        return entries;
    }

    private static FeeStatementChildDto Summarise(
        StatementStudent student, List<Entry> entries, DateTime start, DateTime end)
    {
        var before = entries.Where(e => e.Date.Date < start).ToList();
        var during = entries.Where(e => e.Date.Date >= start && e.Date.Date <= end).ToList();

        decimal Sum(FeeStatementLineType type, Func<Entry, decimal> amount) =>
            during.Where(e => e.Type == type).Sum(amount);

        var child = new FeeStatementChildDto
        {
            StudentId = student.Id,
            StudentName = student.Name,
            AdmissionNumber = student.AdmissionNumber,
            GradeName = student.GradeName,
            OpeningBalance = before.Sum(e => e.Debit - e.Credit),
            Charged = Sum(FeeStatementLineType.Charge, e => e.Debit),
            Credits = Sum(FeeStatementLineType.Discount, e => e.Credit) + Sum(FeeStatementLineType.Waiver, e => e.Credit),
            Paid = Sum(FeeStatementLineType.Payment, e => e.Credit),
            Refunded = Sum(FeeStatementLineType.Refund, e => e.Debit),
        };

        child.ClosingBalance = child.OpeningBalance + child.Charged - child.Credits - child.Paid + child.Refunded;
        return child;
    }

    /// <summary>
    /// What was past its due date and still unpaid on the statement date. Never
    /// more than the closing balance: money paid in but not yet put against a
    /// fee still reduces what the family owes.
    /// </summary>
    private static decimal Overdue(
        List<StatementFee> fees, List<StatementPayment> counted, DateTime statementDate, decimal closingBalance)
    {
        var paidByThen = counted
            .SelectMany(p => p.Allocations)
            .Where(a => a.AllocatedDate.Date <= statementDate)
            .GroupBy(a => a.StudentFeeId)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.Amount));

        var overdue = fees
            .Where(f => f.Status != FeeStatus.Cancelled && f.Status != FeeStatus.Waived)
            .Where(f => f.ChargedOn.Date <= statementDate && f.DueDate.Date < statementDate)
            .Sum(f => Math.Max(0m,
                f.AmountDue - Math.Min(f.DiscountAmount, f.AmountDue) - paidByThen.GetValueOrDefault(f.Id)));

        return Math.Max(0m, Math.Min(overdue, closingBalance));
    }

    private static List<OutstandingFeeDto> Outstanding(
        List<StatementFee> fees, IReadOnlyDictionary<Guid, decimal> paidOnFee, DateTime today)
    {
        return fees
            .Where(f => f.Status != FeeStatus.Cancelled && f.Status != FeeStatus.Waived)
            .Select(f =>
            {
                var discount = Math.Min(f.DiscountAmount, f.AmountDue);
                var paid = paidOnFee.GetValueOrDefault(f.Id);
                return new OutstandingFeeDto
                {
                    StudentFeeId = f.Id,
                    FeeName = f.FeeName,
                    DueDate = f.DueDate,
                    AmountDue = f.AmountDue,
                    DiscountAmount = discount,
                    AmountPaid = paid,
                    Outstanding = f.AmountDue - discount - paid,
                    Status = f.Status,
                    IsOverdue = f.DueDate.Date < today,
                };
            })
            .Where(f => f.Outstanding > 0)
            .OrderBy(f => f.DueDate)
            .ThenBy(f => f.FeeName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
