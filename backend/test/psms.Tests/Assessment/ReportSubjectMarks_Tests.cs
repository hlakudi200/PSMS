using psms.Domain.Assessment;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-07. Recording marks on a subject row: the weighting has to be a real
/// split, and clearing a mark has to clear what was derived from it.
/// </summary>
public class ReportSubjectMarks_Tests
{
    private static ReportSubject NewSubject() =>
        new ReportSubject(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void The_final_mark_is_the_weighted_split_of_the_two()
    {
        var subject = NewSubject();

        subject.RecordMarks(termMark: 60m, examMark: 80m, termWeight: 40m, examWeight: 60m);

        // 60 * 0.4 + 80 * 0.6
        Assert.Equal(72m, subject.FinalMark);
        Assert.Equal(CapsAchievementLevel.Level6, subject.AchievementLevel);
    }

    [Fact]
    public void One_mark_on_its_own_is_the_final_mark()
    {
        var termOnly = NewSubject();
        termOnly.RecordMarks(termMark: 55m, examMark: null);
        Assert.Equal(55m, termOnly.FinalMark);

        var examOnly = NewSubject();
        examOnly.RecordMarks(termMark: null, examMark: 45m);
        Assert.Equal(45m, examOnly.FinalMark);
    }

    [Theory]
    [InlineData(80, 80)]   // the reported case: a final mark of 160
    [InlineData(40, 50)]   // adds to less than 100
    [InlineData(-10, 110)] // adds to 100 but neither side is a real weight
    public void A_split_that_is_not_a_split_is_refused(decimal termWeight, decimal examWeight)
    {
        var subject = NewSubject();

        Assert.False(ReportSubject.IsValidWeighting(termWeight, examWeight));
        Assert.Throws<ArgumentException>(() =>
            subject.RecordMarks(70m, 70m, termWeight, examWeight));
    }

    [Theory]
    [InlineData(0, 100)]   // Grade 12 Life Orientation is the other way round,
    [InlineData(100, 0)]   // and Foundation Phase is 100% school-based.
    [InlineData(25, 75)]
    [InlineData(40, 60)]
    public void The_national_splits_are_all_accepted(decimal termWeight, decimal examWeight)
    {
        Assert.True(ReportSubject.IsValidWeighting(termWeight, examWeight));

        var subject = NewSubject();
        subject.RecordMarks(50m, 90m, termWeight, examWeight);

        Assert.NotNull(subject.FinalMark);
        Assert.InRange(subject.FinalMark.Value, 0m, 100m);
    }

    [Fact]
    public void A_refused_split_leaves_the_row_untouched()
    {
        var subject = NewSubject();
        subject.RecordMarks(60m, 60m);

        Assert.Throws<ArgumentException>(() => subject.RecordMarks(90m, 90m, 80m, 80m));

        Assert.Equal(60m, subject.FinalMark);
        Assert.Equal(60m, subject.TermMark);
    }

    [Fact]
    public void Blanking_both_marks_clears_the_final_mark_and_the_level()
    {
        var subject = NewSubject();
        subject.RecordMarks(72m, null);
        Assert.Equal(72m, subject.FinalMark);

        subject.RecordMarks(null, null);

        // It used to keep 72, so a mark entered by mistake stayed on the card.
        Assert.Null(subject.FinalMark);
        Assert.Null(subject.AchievementLevel);
    }

    [Fact]
    public void The_final_mark_is_stored_at_the_precision_the_column_holds()
    {
        var subject = NewSubject();

        // 55 * 0.4 + 71 * 0.6 = 64.6 exactly; the awkward one is a split that
        // does not land on two decimals.
        subject.RecordMarks(termMark: 55m, examMark: 71.555m, termWeight: 40m, examWeight: 60m);

        // The column is decimal(5,2). Rounding here means the value in memory,
        // which generation reads straight back out, is the value an edit later
        // re-reads from the database — they used to differ by a cent.
        Assert.Equal(decimal.Round(subject.FinalMark.Value, 2), subject.FinalMark);
        Assert.Equal(64.93m, subject.FinalMark);
    }

    // --- RC-05 / RC-15: recording an aggregated mark ---

    [Fact]
    public void A_term_card_keeps_the_two_decimals_it_is_reported_to()
    {
        var subject = NewSubject();
        var aggregate = SubjectMarkAggregator.Aggregate(
            new[]
            {
                new AssessmentContribution(70m, 1m, false),
                new AssessmentContribution(65m, 1m, true),
            },
            sbaPercentage: 60, examPercentage: 40, examinationIsExternal: false);

        subject.RecordAggregate(aggregate, asPromotionMark: false);

        Assert.Equal(68m, subject.FinalMark);      // 70*0.6 + 65*0.4
        Assert.Equal(70m, subject.TermMark);
        Assert.Equal(65m, subject.ExamMark);
        Assert.Equal(60m, subject.TermWeight);
        Assert.Equal(40m, subject.ExamWeight);
        Assert.False(subject.AwaitsExternalExamination);
    }

    [Fact]
    public void A_year_end_card_carries_the_promotion_mark_as_a_whole_number()
    {
        var subject = NewSubject();
        var aggregate = SubjectMarkAggregator.Aggregate(
            new[]
            {
                new AssessmentContribution(71m, 1m, false),
                new AssessmentContribution(69m, 1m, true),
            },
            sbaPercentage: 60, examPercentage: 40, examinationIsExternal: false);

        // 71*0.6 + 69*0.4 = 70.2 -> 70 under NPPPPR 31(3).
        subject.RecordAggregate(aggregate, asPromotionMark: true);

        Assert.Equal(70m, subject.FinalMark);
        Assert.Equal(CapsAchievementLevel.Level6, subject.AchievementLevel);
    }

    [Fact]
    public void A_promotion_mark_on_the_boundary_rounds_up_and_can_change_the_level()
    {
        var subject = NewSubject();
        var aggregate = SubjectMarkAggregator.Aggregate(
            new[]
            {
                new AssessmentContribution(49m, 1m, false),
                new AssessmentContribution(50m, 1m, true),
            },
            sbaPercentage: 50, examPercentage: 50, examinationIsExternal: false);

        // 49.5 -> 50, which is Adequate rather than Moderate. Banker rounding
        // would have left it at 49 and one level down.
        subject.RecordAggregate(aggregate, asPromotionMark: true);

        Assert.Equal(50m, subject.FinalMark);
        Assert.Equal(CapsAchievementLevel.Level4, subject.AchievementLevel);
    }

    [Fact]
    public void A_grade_twelve_row_records_the_school_based_mark_and_says_so()
    {
        var subject = NewSubject();
        var aggregate = SubjectMarkAggregator.Aggregate(
            new[]
            {
                new AssessmentContribution(68m, 1m, false),
                new AssessmentContribution(55m, 1m, true),   // the school trial
            },
            sbaPercentage: 25, examPercentage: 75, examinationIsExternal: true);

        subject.RecordAggregate(aggregate, asPromotionMark: true);

        Assert.Equal(68m, subject.FinalMark);
        Assert.Equal(68m, subject.TermMark);
        Assert.Null(subject.ExamMark);
        Assert.Equal(100m, subject.TermWeight);
        Assert.True(subject.AwaitsExternalExamination);
    }

    [Fact]
    public void Capturing_a_mark_by_hand_clears_the_external_examination_note()
    {
        var subject = NewSubject();
        subject.RecordAggregate(
            SubjectMarkAggregator.Aggregate(
                new[] { new AssessmentContribution(68m, 1m, false) }, 25, 75, true),
            asPromotionMark: true);

        Assert.True(subject.AwaitsExternalExamination);

        // A teacher enters the marks directly. Whatever this mark is, it is the
        // school's own — and the card must not print "the examination is not
        // included in this mark" beside a mark that now includes one.
        subject.RecordMarks(70m, 60m, 40m, 60m);

        Assert.False(subject.AwaitsExternalExamination);
        Assert.Equal(64m, subject.FinalMark);
    }

    [Fact]
    public void A_year_end_mark_can_be_rounded_back_to_a_whole_number_after_an_edit()
    {
        var subject = NewSubject();
        subject.RecordMarks(74m, 75m, 50m, 50m);
        Assert.Equal(74.5m, subject.FinalMark);

        subject.RoundToPromotionMark();

        Assert.Equal(75m, subject.FinalMark);
        Assert.Equal(CapsAchievementLevel.Level6, subject.AchievementLevel);
    }

    [Fact]
    public void Rounding_an_unmarked_subject_does_nothing()
    {
        var subject = NewSubject();

        subject.RoundToPromotionMark();

        Assert.Null(subject.FinalMark);
    }

    [Fact]
    public void Class_statistics_can_be_stamped_on_and_cleared()
    {
        var subject = NewSubject();

        subject.SetClassStatistics(3, 61.5m, 88m, 22m);

        Assert.Equal(3, subject.SubjectPosition);
        Assert.Equal(61.5m, subject.ClassAverage);
        Assert.Equal(88m, subject.HighestInClass);
        Assert.Equal(22m, subject.LowestInClass);

        subject.ClearClassStatistics();

        Assert.Null(subject.SubjectPosition);
        Assert.Null(subject.ClassAverage);
        Assert.Null(subject.HighestInClass);
        Assert.Null(subject.LowestInClass);
    }
}
