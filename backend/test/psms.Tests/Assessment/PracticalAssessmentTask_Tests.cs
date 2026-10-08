using psms.Domain.Assessment;
using psms.Domain.Shared.Enums;
using Shouldly;
using System.Collections.Generic;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-23. The Practical Assessment Task, National Protocol §7.
/// <para>
/// §7(1): "A Practical Assessment Task mark is a <b>compulsory component of the
/// final promotion mark</b>" for a named list of National Senior Certificate
/// subjects. §7(2): "The Practical Assessment Tasks mark must count <b>25% of
/// the end-of-year examination mark</b>."
/// </para>
/// <para>
/// So in those subjects the practical is not School-Based Assessment — it is a
/// quarter of the examination. Counting it as SBA, which is what we did, pays
/// the learner for it on the wrong side of the split and produces a promotion
/// mark the policy does not define.
/// </para>
/// </summary>
public class PracticalAssessmentTask_Tests
{
    private static AssessmentContribution Sba(decimal mark, decimal weight = 1m) =>
        new AssessmentContribution(mark, weight, isExamination: false);

    private static AssessmentContribution Practical(decimal mark, decimal weight = 1m) =>
        new AssessmentContribution(mark, weight, isExamination: false, isPracticalAssessmentTask: true);

    private static AssessmentContribution Exam(decimal mark, decimal weight = 1m) =>
        new AssessmentContribution(mark, weight, isExamination: true);

    /* ─────────────── which subjects §7(1) names ─────────────── */

    [Theory]
    [InlineData("Visual Arts", "VART")]
    [InlineData("Dramatic Arts", "DRA")]
    [InlineData("Dance Studies", "DANC")]
    [InlineData("Music", "MUS")]
    [InlineData("Design", "DSGN")]
    [InlineData("Agricultural Management Practices", "AGMP")]
    [InlineData("Agricultural Technology", "AGTE")]
    [InlineData("Civil Technology", "CIVT")]
    [InlineData("Electrical Technology", "ELTE")]
    [InlineData("Mechanical Technology", "MECT")]
    [InlineData("Engineering Graphics and Design", "EGD")]
    [InlineData("Computer Applications Technology", "CAT")]
    [InlineData("Information Technology", "IT")]
    [InlineData("Consumer Studies", "CONS")]
    [InlineData("Hospitality Studies", "HOSP")]
    [InlineData("Tourism", "TOUR")]
    public void A_section_seven_subject_requires_a_practical_assessment_task(string name, string code)
    {
        SubjectAssessmentRules
            .RequiresPracticalAssessmentTask(name, code, SouthAfricanGradeLevel.Grade11)
            .ShouldBeTrue();
    }

    [Theory]
    [InlineData("English Home Language")]
    [InlineData("IsiZulu First Additional Language")]
    [InlineData("Afrikaans Second Additional Language")]
    public void Every_language_carries_an_oral_mark(string name)
    {
        // §7(1)(c) "Languages: Oral mark".
        SubjectAssessmentRules
            .RequiresPracticalAssessmentTask(name, "LANG", SouthAfricanGradeLevel.Grade10)
            .ShouldBeTrue();
    }

    [Theory]
    [InlineData("Mathematics")]
    [InlineData("Mathematical Literacy")]
    [InlineData("Physical Sciences")]
    [InlineData("Life Sciences")]
    [InlineData("Geography")]
    [InlineData("History")]
    [InlineData("Accounting")]
    [InlineData("Business Studies")]
    public void A_subject_the_policy_does_not_name_does_not(string name)
    {
        SubjectAssessmentRules
            .RequiresPracticalAssessmentTask(name, "X", SouthAfricanGradeLevel.Grade11)
            .ShouldBeFalse();
    }

    [Theory]
    [InlineData(SouthAfricanGradeLevel.GradeR)]
    [InlineData(SouthAfricanGradeLevel.Grade3)]
    [InlineData(SouthAfricanGradeLevel.Grade6)]
    [InlineData(SouthAfricanGradeLevel.Grade9)]
    public void Below_grade_ten_a_practical_is_ordinary_school_work(SouthAfricanGradeLevel grade)
    {
        // §7 is about National Senior Certificate subjects.
        SubjectAssessmentRules
            .RequiresPracticalAssessmentTask("Visual Arts", "VART", grade)
            .ShouldBeFalse();
    }

    /* ─────────────── what §7(2) does to the mark ─────────────── */

    [Fact]
    public void The_practical_is_a_quarter_of_the_examination_not_part_of_the_sba()
    {
        // Grade 11: 40 SBA : 60 examination.
        // SBA        = 60
        // Practical  = 80, written = 40  ->  exam = 0.25*80 + 0.75*40 = 50
        // Final      = 0.40*60 + 0.60*50 = 24 + 30 = 54
        var result = SubjectMarkAggregator.Aggregate(
            new List<AssessmentContribution> { Sba(60), Practical(80), Exam(40) },
            sbaPercentage: 40,
            examPercentage: 60,
            examinationIsExternal: false,
            practicalCountsTowardExamination: true);

        result.SchoolBasedMark.ShouldBe(60m);
        result.ExaminationMark.ShouldBe(50m);          // 0.25*80 + 0.75*40
        result.FinalMark.ShouldBe(54m);
    }

    [Fact]
    public void Without_the_rule_the_same_marks_compose_the_old_way()
    {
        // The practical stays in the SBA: SBA = mean(60, 80) = 70, exam = 40.
        // Final = 0.40*70 + 0.60*40 = 28 + 24 = 52.
        var result = SubjectMarkAggregator.Aggregate(
            new List<AssessmentContribution> { Sba(60), Practical(80), Exam(40) },
            sbaPercentage: 40,
            examPercentage: 60,
            examinationIsExternal: false,
            practicalCountsTowardExamination: false);

        result.SchoolBasedMark.ShouldBe(70m);
        result.ExaminationMark.ShouldBe(40m);
        result.FinalMark.ShouldBe(52m);
    }

    [Fact]
    public void A_practical_counted_in_the_examination_is_not_counted_again_in_the_sba()
    {
        var result = SubjectMarkAggregator.Aggregate(
            new List<AssessmentContribution> { Sba(50), Practical(100), Exam(50) },
            sbaPercentage: 40,
            examPercentage: 60,
            examinationIsExternal: false,
            practicalCountsTowardExamination: true);

        // The SBA is the one SBA task, untouched by the practical.
        result.SchoolBasedMark.ShouldBe(50m);
    }

    [Fact]
    public void A_subject_with_no_practical_recorded_is_marked_on_the_paper()
    {
        // A missing half is dropped, not scored zero — the same way a missing
        // component is treated everywhere else in this aggregator.
        var result = SubjectMarkAggregator.Aggregate(
            new List<AssessmentContribution> { Sba(60), Exam(40) },
            sbaPercentage: 40,
            examPercentage: 60,
            examinationIsExternal: false,
            practicalCountsTowardExamination: true);

        result.ExaminationMark.ShouldBe(40m);
        result.FinalMark.ShouldBe(48m);     // 0.4*60 + 0.6*40
    }

    [Fact]
    public void A_learner_who_sat_no_written_paper_is_marked_on_the_practical()
    {
        var result = SubjectMarkAggregator.Aggregate(
            new List<AssessmentContribution> { Sba(60), Practical(80) },
            sbaPercentage: 40,
            examPercentage: 60,
            examinationIsExternal: false,
            practicalCountsTowardExamination: true);

        result.ExaminationMark.ShouldBe(80m);
        result.FinalMark.ShouldBe(72m);     // 0.4*60 + 0.6*80
    }

    [Fact]
    public void Grade_twelve_is_unaffected_because_the_examination_is_not_the_schools()
    {
        // NPPPPR §31(1): the paper is the external NSC one, and the PAT is
        // moderated into it by the Department. The school holds the SBA, and
        // the practical stays in it.
        var result = SubjectMarkAggregator.Aggregate(
            new List<AssessmentContribution> { Sba(60), Practical(80), Exam(40) },
            sbaPercentage: 25,
            examPercentage: 75,
            examinationIsExternal: true,
            practicalCountsTowardExamination: true);

        result.SchoolBasedMark.ShouldBe(70m);          // the practical is still SBA
        result.ExaminationMark.ShouldBeNull();
        result.AwaitsExternalExamination.ShouldBeTrue();
    }

    [Fact]
    public void Life_orientation_keeps_its_practical_in_the_school_based_total()
    {
        // §7(1)(e) names Life Orientation, but NPPPPR §31(2) makes it 100%
        // school-based in Grades 10-12 — so there is no examination mark for a
        // task to be a quarter of, and the Physical Education Task is simply
        // one of its school-based tasks.
        SubjectAssessmentRules
            .IsFullySchoolBased("Life Orientation", "LO", SouthAfricanGradeLevel.Grade11)
            .ShouldBeTrue();

        var result = SubjectMarkAggregator.Aggregate(
            new List<AssessmentContribution> { Sba(60), Practical(80) },
            sbaPercentage: 100,
            examPercentage: 0,
            examinationIsExternal: false,
            practicalCountsTowardExamination: true);

        result.SchoolBasedMark.ShouldBe(70m);
        result.ExaminationMark.ShouldBeNull();
        result.FinalMark.ShouldBe(70m);
    }

    [Fact]
    public void The_practical_average_is_weighted_like_any_other_component()
    {
        // Two practicals, one carrying three times the weight of the other:
        // (90*3 + 50*1) / 4 = 80. Exam = 0.25*80 + 0.75*60 = 65.
        var result = SubjectMarkAggregator.Aggregate(
            new List<AssessmentContribution> { Sba(70), Practical(90, 3m), Practical(50, 1m), Exam(60) },
            sbaPercentage: 40,
            examPercentage: 60,
            examinationIsExternal: false,
            practicalCountsTowardExamination: true);

        // the practical average is 80, so the examination is 0.25*80 + 0.75*60
        result.ExaminationMark.ShouldBe(65m);
    }
}
