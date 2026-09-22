using psms.Domain.Assessment;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-05 / RC-15. How a learner's assessment marks become the mark on the card.
/// The rules are national policy, quoted on
/// <see cref="SubjectMarkAggregator"/>, so a failure here means either policy
/// moved or something was changed by accident.
/// </summary>
public class SubjectMarkAggregator_Tests
{
    private static AssessmentContribution Sba(decimal percentage, decimal weight = 1m) =>
        new AssessmentContribution(percentage, weight, isExamination: false);

    private static AssessmentContribution Exam(decimal percentage, decimal weight = 1m) =>
        new AssessmentContribution(percentage, weight, isExamination: true);

    // ─── the ordinary case ───

    [Fact]
    public void The_two_components_are_averaged_separately_then_combined()
    {
        // Senior Phase under Circular S8 of 2023: 60% SBA, 40% examination.
        var result = SubjectMarkAggregator.Aggregate(
            new[] { Sba(70m), Sba(80m), Exam(50m) },
            sbaPercentage: 60,
            examPercentage: 40,
            examinationIsExternal: false);

        Assert.Equal(75m, result.SchoolBasedMark);   // (70 + 80) / 2
        Assert.Equal(50m, result.ExaminationMark);
        Assert.Equal(65m, result.FinalMark);          // 75 * 0.6 + 50 * 0.4
        Assert.False(result.AwaitsExternalExamination);
    }

    [Fact]
    public void Tasks_are_weighted_within_their_own_component()
    {
        // A project worth three times a class test.
        var result = SubjectMarkAggregator.Aggregate(
            new[] { Sba(40m, weight: 1m), Sba(80m, weight: 3m) },
            sbaPercentage: 100,
            examPercentage: 0,
            examinationIsExternal: false);

        // (40*1 + 80*3) / 4
        Assert.Equal(70m, result.SchoolBasedMark);
    }

    [Fact]
    public void With_no_weights_configured_it_falls_back_to_a_plain_mean()
    {
        var result = SubjectMarkAggregator.Aggregate(
            new[] { Sba(40m, weight: 0m), Sba(60m, weight: 0m) },
            sbaPercentage: 100,
            examPercentage: 0,
            examinationIsExternal: false);

        Assert.Equal(50m, result.SchoolBasedMark);
    }

    // ─── a missing component is dropped, not zeroed ───

    [Fact]
    public void A_subject_with_no_examination_is_marked_out_of_the_school_work_alone()
    {
        // National Protocol §12(2): a learner who cannot sit the examination is
        // awarded "a mark based on the School-Based Assessment … obtained by the
        // learner". Scoring the missing paper zero would put 45 on the card.
        var result = SubjectMarkAggregator.Aggregate(
            new[] { Sba(75m) },
            sbaPercentage: 60,
            examPercentage: 40,
            examinationIsExternal: false);

        Assert.Equal(75m, result.FinalMark);
        Assert.Equal(100m, result.AppliedSbaWeight);
        Assert.Equal(0m, result.AppliedExamWeight);
    }

    [Fact]
    public void An_examination_with_no_school_work_behind_it_still_counts()
    {
        var result = SubjectMarkAggregator.Aggregate(
            new[] { Exam(64m) },
            sbaPercentage: 60,
            examPercentage: 40,
            examinationIsExternal: false);

        Assert.Equal(64m, result.FinalMark);
        Assert.Equal(0m, result.AppliedSbaWeight);
        Assert.Equal(100m, result.AppliedExamWeight);
    }

    [Fact]
    public void Nothing_at_all_leaves_the_subject_blank()
    {
        var result = SubjectMarkAggregator.Aggregate(
            Array.Empty<AssessmentContribution>(), 60, 40, false);

        Assert.Null(result.SchoolBasedMark);
        Assert.Null(result.ExaminationMark);
        Assert.Null(result.FinalMark);

        Assert.Null(SubjectMarkAggregator.Aggregate(null, 60, 40, false).FinalMark);
    }

    // ─── Grade 12: the examination is not the school's ───

    [Fact]
    public void An_external_examination_is_left_off_the_card_entirely()
    {
        // NPPPPR §31(1). The school holds the 25% SBA; the 75% is the NSC paper,
        // which the school neither sets nor marks. Blending a trial paper in at
        // 75% would produce a number that reads as an NSC result and is not one.
        var result = SubjectMarkAggregator.Aggregate(
            new[] { Sba(68m), Exam(55m) },   // the 55 is the school's own trial
            sbaPercentage: 25,
            examPercentage: 75,
            examinationIsExternal: true);

        Assert.Equal(68m, result.SchoolBasedMark);
        Assert.Null(result.ExaminationMark);
        Assert.Equal(68m, result.FinalMark);
        Assert.True(result.AwaitsExternalExamination);

        // Had the trial been treated as the examination: 68*0.25 + 55*0.75 = 58.25.
        Assert.NotEqual(58.25m, result.FinalMark);
    }

    [Fact]
    public void An_external_band_with_no_school_work_is_not_flagged_as_pending()
    {
        // Nothing to present, so there is nothing to footnote.
        var result = SubjectMarkAggregator.Aggregate(
            new[] { Exam(55m) }, 25, 75, examinationIsExternal: true);

        Assert.Null(result.FinalMark);
        Assert.False(result.AwaitsExternalExamination);
    }

    // ─── a term mark is not composed against the examination ───

    [Fact]
    public void A_term_mark_is_the_weighted_mean_of_everything_done_in_the_term()
    {
        // National Protocol §17(1): "reporting is against the total mark
        // obtained in all tasks completed in a term". The band split composes
        // the YEAR mark against the end-of-year examination; applying it in
        // term 1 would re-weight a class test as though it were the final paper.
        var result = SubjectMarkAggregator.AggregateTerm(new[]
        {
            Sba(70m, weight: 1m),
            Sba(80m, weight: 1m),
            Exam(60m, weight: 2m),   // a term test the teacher typed as an exam
        });

        // (70 + 80 + 60*2) / 4 — one mean over the term's tasks, at the weights
        // the teacher gave them.
        Assert.Equal(67.5m, result.FinalMark);
        Assert.Equal(67.5m, result.SchoolBasedMark);
        Assert.Null(result.ExaminationMark);
        Assert.False(result.AwaitsExternalExamination);

        // The Senior Phase split would have made this 76: 75*0.6 + 60*0.4.
        Assert.NotEqual(72m, result.FinalMark);
    }

    [Fact]
    public void A_term_with_nothing_recorded_is_blank()
    {
        Assert.Null(SubjectMarkAggregator.AggregateTerm(Array.Empty<AssessmentContribution>()).FinalMark);
        Assert.Null(SubjectMarkAggregator.AggregateTerm(null).FinalMark);
    }

    // ─── a band with no examination component ───

    [Fact]
    public void Where_the_band_has_no_examination_an_exam_task_is_school_work()
    {
        // The Foundation Phase is 100% school-based (Circular S8 of 2023), and
        // Life Orientation in the FET phase likewise (NPPPPR §31(2)). There is
        // nothing for an examination to be weighted against, so multiplying it
        // by zero would drop a real mark off the card without saying so.
        var result = SubjectMarkAggregator.Aggregate(
            new[] { Sba(60m), Exam(80m) },
            sbaPercentage: 100,
            examPercentage: 0,
            examinationIsExternal: false);

        Assert.Equal(70m, result.SchoolBasedMark);
        Assert.Equal(70m, result.FinalMark);

        // and nothing is left hanging in an examination column the card prints
        Assert.Null(result.ExaminationMark);
        Assert.False(result.AwaitsExternalExamination);
    }

    // ─── defensive ───

    [Fact]
    public void A_split_that_does_not_total_one_hundred_is_not_used_to_invent_a_mark()
    {
        // The settings screen refuses to save one, and the entity refuses it
        // too, but a mark must not silently come out under- or over-stated if
        // one ever reaches here.
        var result = SubjectMarkAggregator.Aggregate(
            new[] { Sba(80m), Exam(40m) },
            sbaPercentage: 80,
            examPercentage: 80,
            examinationIsExternal: false);

        Assert.Equal(60m, result.FinalMark);   // the even split, not 96
        Assert.Equal(50m, result.AppliedSbaWeight);
        Assert.Equal(50m, result.AppliedExamWeight);
    }

    [Theory]
    [InlineData(100, 0)]   // Foundation Phase, and Life Orientation in the FET phase
    [InlineData(80, 20)]   // Intermediate
    [InlineData(60, 40)]   // Senior
    [InlineData(40, 60)]   // Grades 10 and 11
    public void The_national_splits_all_produce_a_mark_on_the_scale(int sba, int exam)
    {
        var result = SubjectMarkAggregator.Aggregate(
            new[] { Sba(66m), Exam(44m) }, sba, exam, false);

        Assert.NotNull(result.FinalMark);
        Assert.InRange(result.FinalMark.Value, 0m, 100m);
        Assert.Equal(100m, result.AppliedSbaWeight + result.AppliedExamWeight);
    }
}

/// <summary>
/// RC-15. The prescribed rounding, and the subject-level exception to the band
/// split.
/// </summary>
public class CapsRoundingAndSubjectRules_Tests
{
    [Theory]
    // NPPPPR §31(3)'s own two examples.
    [InlineData(70.3, 70)]
    [InlineData(70.6, 71)]
    // The midpoint is the one that matters: Math.Round's default is banker's
    // rounding, which would make this 70, and the policy says 71.
    [InlineData(70.5, 71)]
    [InlineData(69.5, 70)]
    [InlineData(49.5, 50)]   // and this one decides a pass
    [InlineData(49.49, 49)]
    [InlineData(100, 100)]
    [InlineData(0, 0)]
    public void The_promotion_mark_rounds_half_up(double mark, int expected)
    {
        Assert.Equal(expected, CapsRounding.PromotionMark((decimal)mark));
    }

    [Fact]
    public void A_reported_mark_keeps_the_two_decimals_the_column_holds()
    {
        Assert.Equal(66.67m, CapsRounding.ReportedMark(200m / 3m));
        Assert.Equal(70.5m, CapsRounding.ReportedMark(70.495m));
    }

    [Theory]
    [InlineData("Life Orientation", "LFOR")]
    [InlineData("life orientation", null)]
    [InlineData("LIFE ORIENTATION", "")]
    [InlineData("Lewensoriëntering", null)]
    [InlineData("Lewensorientering", null)]
    [InlineData("Anything", "LO")]
    [InlineData("Anything", "lo")]
    public void Life_orientation_is_recognised_by_the_names_schools_use(string name, string code)
    {
        Assert.True(SubjectAssessmentRules.IsLifeOrientation(name, code));
    }

    [Theory]
    [InlineData("Life Sciences", "LSCI")]
    [InlineData("Mathematical Literacy", "MLIT")]
    [InlineData("Orientation Studies", "ORST")]
    [InlineData(null, null)]
    public void Other_subjects_are_not(string name, string code)
    {
        Assert.False(SubjectAssessmentRules.IsLifeOrientation(name, code));
    }

    [Theory]
    [InlineData(SouthAfricanGradeLevel.Grade12)]
    [InlineData(SouthAfricanGradeLevel.Grade11)]
    [InlineData(SouthAfricanGradeLevel.Grade10)]
    public void Life_orientation_is_fully_school_based_in_the_FET_phase(SouthAfricanGradeLevel grade)
    {
        // NPPPPR §31(2): "the school-based assessment component will be 100% of
        // the total mark."
        Assert.True(SubjectAssessmentRules.IsFullySchoolBased("Life Orientation", "LO", grade));
    }

    [Theory]
    [InlineData(SouthAfricanGradeLevel.Grade9)]
    [InlineData(SouthAfricanGradeLevel.Grade4)]
    public void The_exception_does_not_reach_down_into_the_GET_phase(SouthAfricanGradeLevel grade)
    {
        // Below Grade 10 the band split already applies to every subject; the
        // §31(2) exception is written about the FET phase.
        Assert.False(SubjectAssessmentRules.IsFullySchoolBased("Life Orientation", "LO", grade));
    }

    [Fact]
    public void An_ordinary_subject_never_takes_the_exception()
    {
        Assert.False(SubjectAssessmentRules.IsFullySchoolBased(
            "Mathematics", "MATH", SouthAfricanGradeLevel.Grade12));
    }
}
