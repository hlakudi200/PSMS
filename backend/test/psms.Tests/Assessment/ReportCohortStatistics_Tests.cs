using psms.Assessment.Reports;
using psms.Domain.Assessment;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-06. ClassPosition, SubjectPosition, ClassAverage, HighestInClass and
/// LowestInClass existed on the entities, were exposed in the DTOs and were
/// never written by anything. These pin the cohort pass that now writes them,
/// and the tie convention it uses.
/// </summary>
public class ReportCohortStatistics_Tests
{
    private static readonly Guid ClassId = Guid.NewGuid();
    private static readonly Guid Maths = Guid.NewGuid();
    private static readonly Guid English = Guid.NewGuid();

    private static Report ReportWith(params (Guid SubjectId, decimal? Mark)[] subjects) =>
        ReportWith(ReportStatus.Generated, subjects);

    private static Report ReportWith(
        ReportStatus status,
        params (Guid SubjectId, decimal? Mark)[] subjects)
    {
        var report = new Report(
            Guid.NewGuid(), 1, Guid.NewGuid(), ClassId, Guid.NewGuid(), ReportType.Term1);

        foreach (var (subjectId, mark) in subjects)
        {
            var row = new ReportSubject(Guid.NewGuid(), report.Id, subjectId);

            if (mark.HasValue)
                row.RecordMarks(mark.Value, null);

            report.SubjectReports.Add(row);
        }

        report.RecalculateOverall(report.SubjectReports);
        report.Status = status;

        return report;
    }

    private static ReportSubject RowFor(Report report, Guid subjectId) =>
        report.SubjectReports.Single(rs => rs.SubjectId == subjectId);

    // ─── the ranking convention ───

    [Fact]
    public void Positions_run_from_the_highest_mark_down()
    {
        var scores = new[]
        {
            new KeyValuePair<string, decimal?>("bottom", 40m),
            new KeyValuePair<string, decimal?>("top", 90m),
            new KeyValuePair<string, decimal?>("middle", 65m),
        };

        var positions = CohortRanking.Positions(scores);

        Assert.Equal(1, positions["top"]);
        Assert.Equal(2, positions["middle"]);
        Assert.Equal(3, positions["bottom"]);
    }

    [Fact]
    public void A_tie_shares_the_higher_position_and_the_next_one_is_skipped()
    {
        // Competition ranking, the "1224" convention South African schools
        // print, so a position always reads sensibly against the class size.
        var scores = new[]
        {
            new KeyValuePair<string, decimal?>("a", 90m),
            new KeyValuePair<string, decimal?>("b", 82m),
            new KeyValuePair<string, decimal?>("c", 82m),
            new KeyValuePair<string, decimal?>("d", 70m),
        };

        var positions = CohortRanking.Positions(scores);

        Assert.Equal(1, positions["a"]);
        Assert.Equal(2, positions["b"]);
        Assert.Equal(2, positions["c"]);
        Assert.Equal(4, positions["d"]);
    }

    [Fact]
    public void A_learner_with_no_mark_has_no_position_rather_than_last_place()
    {
        var scores = new[]
        {
            new KeyValuePair<string, decimal?>("marked", 55m),
            new KeyValuePair<string, decimal?>("unmarked", null),
        };

        var positions = CohortRanking.Positions(scores);

        Assert.Equal(1, positions["marked"]);
        Assert.False(positions.ContainsKey("unmarked"));
    }

    [Fact]
    public void A_class_with_no_marks_at_all_has_no_figures()
    {
        Assert.Null(CohortRanking.Summarise(new decimal?[] { null, null }));
        Assert.Null(CohortRanking.Summarise(Array.Empty<decimal?>()));
        Assert.Empty(CohortRanking.Positions(Array.Empty<KeyValuePair<Guid, decimal?>>()));
    }

    [Fact]
    public void The_class_figures_come_from_the_marked_learners_only()
    {
        var summary = CohortRanking.Summarise(new decimal?[] { 80m, null, 40m, 60m });

        Assert.NotNull(summary);
        Assert.Equal(60m, summary.Value.Average);
        Assert.Equal(80m, summary.Value.Highest);
        Assert.Equal(40m, summary.Value.Lowest);
        Assert.Equal(3, summary.Value.Count);
    }

    // ─── the pass over a whole class ───

    [Fact]
    public void Every_report_in_the_cohort_gets_its_class_position()
    {
        var top = ReportWith((Maths, 90m), (English, 80m));      // overall 85
        var middle = ReportWith((Maths, 70m), (English, 60m));   // overall 65
        var bottom = ReportWith((Maths, 40m), (English, 50m));   // overall 45

        ReportCohortStatisticsService.Apply(new[] { middle, bottom, top });

        Assert.Equal(1, top.ClassPosition);
        Assert.Equal(2, middle.ClassPosition);
        Assert.Equal(3, bottom.ClassPosition);
    }

    [Fact]
    public void Each_subject_row_carries_that_subjects_class_figures()
    {
        var a = ReportWith((Maths, 90m), (English, 50m));
        var b = ReportWith((Maths, 60m), (English, 70m));
        var c = ReportWith((Maths, 30m), (English, 60m));

        ReportCohortStatisticsService.Apply(new[] { a, b, c });

        // Maths: 90, 60, 30 → average 60, high 90, low 30
        Assert.Equal(60m, RowFor(a, Maths).ClassAverage);
        Assert.Equal(90m, RowFor(a, Maths).HighestInClass);
        Assert.Equal(30m, RowFor(a, Maths).LowestInClass);
        Assert.Equal(1, RowFor(a, Maths).SubjectPosition);
        Assert.Equal(3, RowFor(c, Maths).SubjectPosition);

        // English: 50, 70, 60 → the same learner places differently per subject.
        Assert.Equal(60m, RowFor(a, English).ClassAverage);
        Assert.Equal(3, RowFor(a, English).SubjectPosition);
        Assert.Equal(1, RowFor(b, English).SubjectPosition);
    }

    [Fact]
    public void An_unmarked_row_still_shows_the_class_figures_but_no_position()
    {
        var marked = ReportWith((Maths, 80m));
        var alsoMarked = ReportWith((Maths, 60m));
        var blank = ReportWith((Maths, null));

        ReportCohortStatisticsService.Apply(new[] { marked, alsoMarked, blank });

        var row = RowFor(blank, Maths);

        Assert.Null(row.SubjectPosition);
        Assert.Equal(70m, row.ClassAverage);
        Assert.Equal(80m, row.HighestInClass);
        Assert.Null(blank.ClassPosition);
    }

    [Fact]
    public void Stale_figures_are_cleared_when_the_subject_loses_its_last_mark()
    {
        var only = ReportWith((Maths, 75m));
        ReportCohortStatisticsService.Apply(new[] { only });
        Assert.Equal(75m, RowFor(only, Maths).ClassAverage);

        // The mark is withdrawn and the cohort recalculated.
        RowFor(only, Maths).RecordMarks(null, null);
        only.RecalculateOverall(only.SubjectReports);
        ReportCohortStatisticsService.Apply(new[] { only });

        Assert.Null(RowFor(only, Maths).ClassAverage);
        Assert.Null(RowFor(only, Maths).HighestInClass);
        Assert.Null(RowFor(only, Maths).SubjectPosition);
        Assert.Null(only.ClassPosition);
    }

    [Fact]
    public void A_single_learner_is_first_of_one()
    {
        var alone = ReportWith((Maths, 55m));

        ReportCohortStatisticsService.Apply(new[] { alone });

        Assert.Equal(1, alone.ClassPosition);
        Assert.Equal(1, alone.TotalStudentsInClass);
        Assert.Equal(1, RowFor(alone, Maths).SubjectPosition);
        Assert.Equal(55m, RowFor(alone, Maths).ClassAverage);
    }

    [Fact]
    public void The_denominator_is_the_cohort_that_was_ranked()
    {
        // "Position 3 of 30" has to be internally consistent. It used to be a
        // class headcount frozen at generation time, so a learner arriving or
        // leaving could print a position larger than the class.
        var reports = new[]
        {
            ReportWith((Maths, 80m)),
            ReportWith((Maths, 60m)),
            ReportWith((Maths, 40m)),
        };

        ReportCohortStatisticsService.Apply(reports);

        Assert.All(reports, r => Assert.Equal(3, r.TotalStudentsInClass));
        Assert.Equal(3, reports.Last().ClassPosition);
    }

    // ─── an issued card is not rewritten ───

    [Theory]
    [InlineData(ReportStatus.Published)]
    [InlineData(ReportStatus.Approved)]
    public void An_issued_card_counts_in_the_cohort_but_is_never_restated(ReportStatus status)
    {
        // A published card may already be a PDF in a parent's hands, and the
        // National Protocol §25(3) requires it to carry no corrections that
        // compromise its legal status. A classmate's later mark must not move
        // the position printed on it.
        var issued = ReportWith(status, (Maths, 90m));
        var draft = ReportWith((Maths, 50m));

        var restated = ReportCohortStatisticsService.Apply(new[] { issued, draft });

        Assert.Equal(1, restated);

        Assert.Null(issued.ClassPosition);
        Assert.Null(issued.TotalStudentsInClass);
        Assert.Null(RowFor(issued, Maths).SubjectPosition);
        Assert.Null(RowFor(issued, Maths).ClassAverage);

        // ...but the 90 still counts: the draft is second of two, and the class
        // average and highest include the issued learner's mark.
        Assert.Equal(2, draft.ClassPosition);
        Assert.Equal(2, draft.TotalStudentsInClass);
        Assert.Equal(2, RowFor(draft, Maths).SubjectPosition);
        Assert.Equal(70m, RowFor(draft, Maths).ClassAverage);
        Assert.Equal(90m, RowFor(draft, Maths).HighestInClass);
    }

    [Fact]
    public void A_cohort_that_is_entirely_issued_is_left_alone()
    {
        var a = ReportWith(ReportStatus.Published, (Maths, 90m));
        var b = ReportWith(ReportStatus.Approved, (Maths, 50m));

        Assert.Equal(0, ReportCohortStatisticsService.Apply(new[] { a, b }));
        Assert.Null(a.ClassPosition);
        Assert.Null(b.ClassPosition);
    }

    [Fact]
    public void Two_identical_learners_tie_on_the_overall_too()
    {
        var a = ReportWith((Maths, 70m), (English, 70m));
        var b = ReportWith((Maths, 70m), (English, 70m));
        var c = ReportWith((Maths, 50m), (English, 50m));

        ReportCohortStatisticsService.Apply(new[] { a, b, c });

        Assert.Equal(1, a.ClassPosition);
        Assert.Equal(1, b.ClassPosition);
        Assert.Equal(3, c.ClassPosition);
    }

    [Fact]
    public void An_empty_cohort_is_a_no_op()
    {
        Assert.Equal(0, ReportCohortStatisticsService.Apply(Array.Empty<Report>()));
        Assert.Equal(0, ReportCohortStatisticsService.Apply(null));
    }
}
