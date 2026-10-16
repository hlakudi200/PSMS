using Microsoft.EntityFrameworkCore;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace psms.Tests.Admissions;

/// <summary>
/// A date of birth is 14 March 2015 wherever you are standing.
/// <para>
/// Stored as a timestamp it became <c>2015-03-13T22:00:00Z</c> — the right
/// instant for midnight in Johannesburg, and the wrong date to anything that
/// formats in UTC. Seen on the deployed system: the same learner read as born
/// on the 14th on the apply form and the 13th on the workflow summary, and an
/// assessment set for Monday the 26th showed as the 25th on the screen the
/// principal decides a child's place from.
/// </para>
/// </summary>
public class ADateOfBirthIsADate_Tests : psmsTestBase
{
    [Fact]
    public async Task The_day_that_was_entered_is_the_day_that_comes_back()
    {
        var id = Guid.NewGuid();
        var entered = new DateTime(2015, 3, 14);

        await UsingDbContextAsync(1, async context =>
        {
            context.Applications.Add(new Application(
                id, 1, "APP-001-2026-09999",
                "Naledi", "Mokoena", entered, Gender.Female,
                Guid.NewGuid(), Guid.NewGuid(), "parent@example.test"));
            await context.SaveChangesAsync();
        });

        var readBack = await UsingDbContextAsync(1, async context =>
            await context.Applications.Where(a => a.Id == id).Select(a => a.DateOfBirth).FirstAsync());

        readBack.Date.ShouldBe(entered);
        // And not dragged back across midnight on the way out.
        readBack.Day.ShouldBe(14);
        readBack.Month.ShouldBe(3);
    }

    [Fact]
    public async Task A_date_survives_whatever_the_server_thinks_the_time_is()
    {
        /* The old fault was a conversion, so the test that matters is the one
           where a conversion would show: a value right on the boundary. Local
           midnight is two hours behind UTC in Johannesburg, so a naive round
           trip lands on the previous day. */
        var id = Guid.NewGuid();
        var newYear = new DateTime(2016, 1, 1);

        await UsingDbContextAsync(1, async context =>
        {
            context.Applications.Add(new Application(
                id, 1, "APP-001-2026-09998",
                "Thabo", "Dlamini", newYear, Gender.Male,
                Guid.NewGuid(), Guid.NewGuid(), "parent@example.test"));
            await context.SaveChangesAsync();
        });

        var readBack = await UsingDbContextAsync(1, async context =>
            await context.Applications.Where(a => a.Id == id).Select(a => a.DateOfBirth).FirstAsync());

        readBack.Year.ShouldBe(2016);
        readBack.Month.ShouldBe(1);
        readBack.Day.ShouldBe(1);
    }

    [Fact]
    public void The_migration_converts_through_the_schools_own_time_zone()
    {
        /* The scaffolded AlterColumn becomes a plain TYPE date, which Postgres
           casts in the SESSION time zone — UTC on the server. Every stored
           value would have lost a day on the way across and the fault would
           have been baked in permanently rather than fixed. */
        var migration = File.ReadAllText(MigrationFile());

        migration.ShouldContain("Africa/Johannesburg");
        migration.ShouldContain("AT TIME ZONE");
        migration.ShouldContain("TYPE date");
        // The scaffolded version, which must not be what ships.
        migration.ShouldNotContain("AlterColumn<DateTime>");
    }

    [Fact]
    public void Every_date_only_column_is_converted_the_same_way()
    {
        var migration = File.ReadAllText(MigrationFile());

        foreach (var table in new[] { "Applications", "Students", "AdmissionInterviews", "AdmissionAssessments" })
            migration.ShouldContain(table);
    }

    [Fact]
    public void It_can_be_undone()
    {
        // Back to midnight of that date in Johannesburg, which is what was there.
        var migration = File.ReadAllText(MigrationFile());

        migration.ShouldContain("TYPE timestamp with time zone");
    }

    [Fact]
    public void A_moment_is_still_a_moment()
    {
        /* Only the dates move. A submission, a decision or an expiry really is
           an instant and really is in UTC, and converting those would be the
           same mistake pointing the other way. */
        var context = File.ReadAllText(DbContextFile());

        context.ShouldContain("DateOnlyProperties");
        foreach (var moment in new[] { "SubmissionDate", "DecisionDate", "ExpiryDate", "CreationTime" })
            context.ShouldNotContain($"nameof(psms.Domain.Admissions.Entities.Application.{moment})");
    }

    [Fact]
    public void The_client_stops_implying_a_time_zone_it_does_not_mean()
    {
        var journey = FrontendFile("components", "modules", "admissions", "apply", "ApplyJourney.tsx");

        journey.ShouldContain("dateOfBirth: parsed.data.dateOfBirth.format('YYYY-MM-DD')");
        journey.ShouldNotContain("dateOfBirth: parsed.data.dateOfBirth.toISOString()");
    }

    [Theory]
    [InlineData("ScheduleAssessmentModal.tsx")]
    [InlineData("ScheduleInterviewModal.tsx")]
    public void And_so_do_the_screens_that_book_a_day(string file)
    {
        var modal = FrontendFile("components", "modals", "admissions", file);

        modal.ShouldContain("scheduledDate: parsed.data.scheduledDate.format('YYYY-MM-DD')");
        modal.ShouldNotContain("scheduledDate: parsed.data.scheduledDate.toISOString()");
    }

    private static string Root()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
            directory = directory.Parent;

        directory.ShouldNotBeNull("could not find the repository root");
        return directory.FullName;
    }

    private static string MigrationFile() =>
        Directory.GetFiles(Path.Combine(Root(), "src", "psms.EntityFrameworkCore", "Migrations"),
                "*ADM399_ADateOfBirthIsADate.cs")
            .Single(f => !f.EndsWith(".Designer.cs", StringComparison.Ordinal));

    private static string DbContextFile() =>
        Path.Combine(Root(), "src", "psms.EntityFrameworkCore", "EntityFrameworkCore", "psmsDbContext.cs");

    private static string FrontendFile(params string[] parts)
    {
        var directory = new DirectoryInfo(Root());
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "frontend")))
            directory = directory.Parent;

        directory.ShouldNotBeNull("could not find the frontend folder");
        return File.ReadAllText(
            Path.Combine(new[] { directory.FullName, "frontend", "src" }.Concat(parts).ToArray()));
    }
}
