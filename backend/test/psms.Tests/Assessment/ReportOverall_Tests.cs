using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-07. The overall average used to be worked out in three places — stored on
/// generation, recomputed by the PDF, and recomputed again by the browser print
/// view — which could disagree on the same card. There is one definition now,
/// on the entity, and these pin what it does.
/// </summary>
public class ReportOverall_Tests
{
    private static Report NewReport() => new Report(
        Guid.NewGuid(), 1, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ReportType.Term1);

    private static ReportSubject Subject(decimal? finalMark)
    {
        var subject = new ReportSubject(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        if (finalMark.HasValue)
            subject.RecordMarks(finalMark.Value, null);

        return subject;
    }

    [Fact]
    public void The_overall_is_the_mean_of_the_subject_final_marks()
    {
        var report = NewReport();

        report.RecalculateOverall(new decimal?[] { 60m, 70m, 80m });

        Assert.Equal(70m, report.OverallPercentage);
        Assert.Equal(CapsAchievementLevel.Level6, report.OverallAchievementLevel);
    }

    [Fact]
    public void A_subject_with_no_mark_is_left_out_rather_than_counted_as_zero()
    {
        var report = NewReport();

        report.RecalculateOverall(new decimal?[] { 80m, null, 60m });

        // 70, not 46.67 — an unmarked subject must not depress the average.
        Assert.Equal(70m, report.OverallPercentage);
    }

    [Fact]
    public void With_nothing_marked_the_overall_is_cleared_not_zero()
    {
        var report = NewReport();
        report.RecalculateOverall(new decimal?[] { 55m });
        Assert.NotNull(report.OverallPercentage);

        report.RecalculateOverall(new decimal?[] { null, null });

        Assert.Null(report.OverallPercentage);
        Assert.Null(report.OverallAchievementLevel);
    }

    [Fact]
    public void An_empty_report_has_no_overall()
    {
        var report = NewReport();

        report.RecalculateOverall(Array.Empty<decimal?>());

        Assert.Null(report.OverallPercentage);
        Assert.Null(report.OverallAchievementLevel);
    }

    [Fact]
    public void The_stored_overall_is_rounded_to_what_the_column_holds()
    {
        var report = NewReport();

        // 200/3 = 66.666..., which decimal(5,2) cannot hold. Round here so the
        // value in memory is the value in the database, and the PDF rendering
        // it prints the same number as the screen.
        report.RecalculateOverall(new decimal?[] { 66m, 67m, 67m });

        Assert.Equal(66.67m, report.OverallPercentage);
    }

    [Fact]
    public void A_boundary_average_takes_the_higher_level()
    {
        var report = NewReport();

        report.RecalculateOverall(new decimal?[] { 79m, 81m });

        Assert.Equal(80m, report.OverallPercentage);
        Assert.Equal(CapsAchievementLevel.Level7, report.OverallAchievementLevel);
    }

    [Fact]
    public void The_subject_row_overload_reads_the_same_marks()
    {
        var report = NewReport();
        var subjects = new List<ReportSubject> { Subject(50m), Subject(null), Subject(70m) };

        report.RecalculateOverall(subjects);

        Assert.Equal(60m, report.OverallPercentage);
    }

    [Fact]
    public void Nothing_at_all_is_treated_as_nothing_marked()
    {
        var report = NewReport();

        report.RecalculateOverall((IEnumerable<decimal?>)null);

        Assert.Null(report.OverallPercentage);
    }
}
