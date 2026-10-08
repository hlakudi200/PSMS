using Abp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using psms.EntityFrameworkCore;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace psms.Tests.Assessment;

/// <summary>
/// A unique index over a soft-deletable table has to exclude deleted rows.
/// <para>
/// Nothing else in the system agrees with an index that does not. Every query
/// runs through ABP's soft-delete filter, so application code cannot see a
/// deleted row — it checks for a duplicate, finds none, and inserts. The
/// database still holds the row, the insert violates the index, and the request
/// dies with a 500 that names nothing.
/// </para>
/// <para>
/// That is exactly what happened to report cards: deleting a learner's card
/// left its slot in IX_Reports_StudentId_TermId_ReportType occupied for good,
/// so that learner could never be given another card for the same term. The
/// index is checked here for every entity rather than only that one, because
/// the fault is invisible until someone deletes a row and tries again.
/// </para>
/// </summary>
public class SoftDeleteUniqueIndex_Tests : psmsTestBase
{
    private readonly ITestOutputHelper _output;

    public SoftDeleteUniqueIndex_Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Indexes that are allowed to ignore this rule, with the reason. An entry
    /// here is a decision, not a silence: a unique index that counts deleted
    /// rows means the value can never be reused after a delete.
    /// </summary>
    private static readonly Dictionary<string, string> Exempt = new()
    {
        // These two are issued numbers on a financial or legal record. Burning
        // the number when the row is deleted is the point: a receipt number or
        // an application number that can be handed out twice is an audit
        // problem, not a convenience. Both are generated rather than typed, so
        // the constraint is never reached in normal use.
        ["IX_Payments_ReceiptNumber"] = "a receipt number is never reused, even after a payment is voided",
        ["IX_Applications_ApplicationNumber"] = "an application number is never reused",
    };

    [Fact]
    public void Every_unique_index_on_a_soft_deletable_table_ignores_deleted_rows()
    {
        var offenders = new List<string>();
        var checkedCount = 0;

        UsingDbContext(context =>
        {
            foreach (var entity in context.Model.GetEntityTypes())
            {
                if (!typeof(ISoftDelete).IsAssignableFrom(entity.ClrType))
                    continue;

                foreach (var index in entity.GetIndexes().Where(i => i.IsUnique))
                {
                    var name = index.GetDatabaseName()
                        ?? $"{entity.ClrType.Name}({string.Join(",", index.Properties.Select(p => p.Name))})";

                    if (Exempt.ContainsKey(name))
                        continue;

                    checkedCount++;
                    var filter = index.GetFilter();

                    // The filter must actually narrow on IsDeleted. A null filter
                    // means the index covers deleted rows too.
                    if (filter == null || filter.IndexOf("IsDeleted", StringComparison.OrdinalIgnoreCase) < 0)
                        offenders.Add($"{name} on {entity.ClrType.Name} — filter: {filter ?? "(none)"}");
                }
            }
        });

        _output.WriteLine($"checked {checkedCount} unique indexes on soft-deletable entities");
        foreach (var o in offenders) _output.WriteLine("  OFFENDER: " + o);

        checkedCount.ShouldBeGreaterThan(0, "the model should have unique indexes to check");
        offenders.ShouldBeEmpty(
            "a unique index that counts soft-deleted rows makes the value unusable forever after a delete");
    }

    [Fact]
    public void The_one_report_card_per_learner_per_term_index_ignores_deleted_cards()
    {
        UsingDbContext(context =>
        {
            var index = context.Model
                .GetEntityTypes()
                .Single(e => e.ClrType == typeof(psms.Domain.Assessment.Entities.Report))
                .GetIndexes()
                .Single(i => i.GetDatabaseName() == "IX_Reports_StudentId_TermId_ReportType");

            index.IsUnique.ShouldBeTrue();
            index.GetFilter().ShouldContain("IsDeleted",
                Case.Insensitive,
                "deleting a card must free the learner's slot for that term");
        });
    }
}
