using psms.Domain.Assessment.Promotion;
using psms.Domain.Shared.Enums;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-16. The national promotion requirements, per phase. Every expectation
/// here traces to a clause of the NPPPPR quoted on <see cref="PromotionRules"/>:
/// a failure means either policy moved or the rules were edited by accident.
/// </summary>
public class PromotionRules_Tests
{
    private static PromotionSubject Subject(
        string name, SubjectRole role, decimal? mark, bool sbaComplete = true, bool additional = false) =>
        new PromotionSubject(name, role, mark, sbaComplete, additional);

    private static PromotionSubject Home(decimal? mark) =>
        Subject("English Home Language", SubjectRole.HomeLanguage, mark);

    private static PromotionSubject Fal(decimal? mark) =>
        Subject("isiZulu First Additional Language", SubjectRole.FirstAdditionalLanguage, mark);

    private static PromotionSubject Maths(decimal? mark) =>
        Subject("Mathematics", SubjectRole.Mathematics, mark);

    private static PromotionSubject Other(string name, decimal? mark, bool sbaComplete = true) =>
        Subject(name, SubjectRole.Other, mark, sbaComplete);

    private static void AssertUnmet(PromotionEvaluation evaluation, string clause)
    {
        Assert.False(evaluation.MeetsRequirements);
        Assert.Contains(evaluation.Unmet, r => r.Clause == clause);
    }

    // ─── Grade R ─────────────────────────────────────────────────────────────

    [Fact]
    public void Grade_R_needs_a_home_language_at_level_four_and_maths_at_level_three()
    {
        var pass = PromotionRules.Evaluate(SouthAfricanGradeLevel.GradeR,
            new List<PromotionSubject> { Home(52m), Maths(41m) });

        Assert.True(pass.MeetsRequirements);
        Assert.Equal(PromotionDecision.Promoted, pass.Recommended);
    }

    [Fact]
    public void Grade_R_a_home_language_at_level_three_is_not_enough()
    {
        // 49% is Moderate; the clause asks for Adequate.
        var evaluation = PromotionRules.Evaluate(SouthAfricanGradeLevel.GradeR,
            new List<PromotionSubject> { Home(49m), Maths(60m) });

        AssertUnmet(evaluation, "NPPPPR §8(2)(a)");
        Assert.Equal(PromotionDecision.Retained, evaluation.Recommended);
    }

    // ─── Foundation Phase ────────────────────────────────────────────────────

    [Fact]
    public void The_foundation_phase_adds_the_first_additional_language()
    {
        var subjects = new List<PromotionSubject> { Home(55m), Fal(41m), Maths(44m) };

        Assert.True(PromotionRules.Evaluate(SouthAfricanGradeLevel.Grade2, subjects).MeetsRequirements);

        var withoutFal = new List<PromotionSubject> { Home(55m), Fal(35m), Maths(44m) };
        AssertUnmet(
            PromotionRules.Evaluate(SouthAfricanGradeLevel.Grade2, withoutFal),
            "NPPPPR §8(3)(b)");
    }

    // ─── Intermediate Phase ──────────────────────────────────────────────────

    [Fact]
    public void The_intermediate_phase_adds_two_of_the_remaining_subjects_at_level_three()
    {
        var meets = new List<PromotionSubject>
        {
            Home(55m), Fal(45m), Maths(40m),
            Other("Natural Sciences and Technology", 48m),
            Other("Social Sciences", 42m),
            Other("Life Skills", 25m),
        };

        Assert.True(PromotionRules.Evaluate(SouthAfricanGradeLevel.Grade5, meets).MeetsRequirements);

        var onlyOne = new List<PromotionSubject>
        {
            Home(55m), Fal(45m), Maths(40m),
            Other("Natural Sciences and Technology", 48m),
            Other("Social Sciences", 31m),
            Other("Life Skills", 25m),
        };

        AssertUnmet(
            PromotionRules.Evaluate(SouthAfricanGradeLevel.Grade5, onlyOne),
            "NPPPPR §14(2)(d)");
    }

    // ─── Senior Phase ────────────────────────────────────────────────────────

    /// <summary>The nine subjects of NPPPPR §19, with marks supplied per test.</summary>
    private static List<PromotionSubject> SeniorNine(
        decimal home, decimal fal, decimal maths, params decimal[] others)
    {
        var names = new[]
        {
            "Natural Sciences", "Life Orientation", "Social Sciences",
            "Technology", "Creative Arts", "Economic and Management Sciences",
        };

        var subjects = new List<PromotionSubject> { Home(home), Fal(fal), Maths(maths) };
        subjects.AddRange(others.Select((mark, i) => Other(names[i], mark)));

        return subjects;
    }

    [Fact]
    public void A_senior_phase_learner_who_meets_every_clause_is_promoted()
    {
        // 3 others at level 3 (40+), 2 more at level 2 (30+), the ninth at 20.
        var evaluation = PromotionRules.Evaluate(
            SouthAfricanGradeLevel.Grade9,
            SeniorNine(55m, 45m, 42m, 48m, 44m, 41m, 35m, 32m, 20m));

        Assert.True(evaluation.MeetsRequirements);
        Assert.Equal(PromotionDecision.Promoted, evaluation.Recommended);
        Assert.Empty(evaluation.Unmet);
    }

    [Fact]
    public void A_subject_counted_at_level_three_is_not_counted_again_at_level_two()
    {
        // Three others at 40+, and only ONE more above 30. Reading the same
        // three twice would satisfy (e) and wrongly promote.
        var evaluation = PromotionRules.Evaluate(
            SouthAfricanGradeLevel.Grade9,
            SeniorNine(55m, 45m, 42m, 48m, 44m, 41m, 35m, 12m, 10m));

        AssertUnmet(evaluation, "NPPPPR §21(1)(e)");
    }

    [Fact]
    public void The_senior_phase_home_language_must_reach_level_four_not_three()
    {
        var evaluation = PromotionRules.Evaluate(
            SouthAfricanGradeLevel.Grade8,
            SeniorNine(45m, 45m, 42m, 48m, 44m, 41m, 35m, 32m, 20m));

        AssertUnmet(evaluation, "NPPPPR §21(1)(a)");

        // and the reason names the subject and the mark it fell short at
        var failed = evaluation.Unmet.First(r => r.Clause == "NPPPPR §21(1)(a)");
        Assert.Contains("English Home Language", failed.Detail);
    }

    [Fact]
    public void The_ninth_subject_still_needs_its_school_based_assessment()
    {
        var subjects = SeniorNine(55m, 45m, 42m, 48m, 44m, 41m, 35m, 32m, 20m);
        subjects.Add(Other("Additional Subject", 10m, sbaComplete: false));

        AssertUnmet(
            PromotionRules.Evaluate(SouthAfricanGradeLevel.Grade9, subjects),
            "NPPPPR §21(1)");
    }

    [Fact]
    public void An_optional_extra_subject_is_left_out_of_the_requirements()
    {
        // §19(9): an additional language "will be regarded as an additional
        // subject not to be taken into account for promotion requirements".
        var subjects = SeniorNine(55m, 45m, 42m, 48m, 44m, 41m, 35m, 32m, 20m);
        subjects.Add(Subject("German Second Additional Language", SubjectRole.SecondAdditionalLanguage,
            5m, sbaComplete: false, additional: true));

        Assert.True(PromotionRules.Evaluate(SouthAfricanGradeLevel.Grade9, subjects).MeetsRequirements);
    }

    // ─── Further Education and Training ──────────────────────────────────────

    private static List<PromotionSubject> SevenSubjects(
        decimal home, decimal fal, decimal maths, decimal a, decimal b, decimal c, decimal d) =>
        new List<PromotionSubject>
        {
            Home(home), Fal(fal), Maths(maths),
            Other("Life Orientation", a),
            Other("Physical Sciences", b),
            Other("Geography", c),
            Other("History", d),
        };

    [Fact]
    public void An_FET_learner_with_three_at_forty_and_three_at_thirty_is_promoted()
    {
        // §29(1)(a), stated in percentages rather than levels.
        var evaluation = PromotionRules.Evaluate(
            SouthAfricanGradeLevel.Grade11,
            SevenSubjects(52m, 41m, 40m, 35m, 33m, 31m, 12m));

        Assert.True(evaluation.MeetsRequirements);
    }

    [Fact]
    public void The_forty_percent_subjects_must_include_a_home_language()
    {
        // Three subjects clear 40, but none of them is the Home Language.
        var evaluation = PromotionRules.Evaluate(
            SouthAfricanGradeLevel.Grade11,
            SevenSubjects(38m, 41m, 40m, 44m, 33m, 31m, 30m));

        AssertUnmet(evaluation, "NPPPPR §29(1)(a)");
        Assert.Contains(
            evaluation.Unmet,
            r => r.Detail != null && r.Detail.Contains("Home Language level reaches 40%"));
    }

    [Fact]
    public void Fewer_than_seven_completed_subjects_fails_on_its_own_clause()
    {
        var six = SevenSubjects(52m, 41m, 40m, 35m, 33m, 31m, 12m).Take(6).ToList();

        AssertUnmet(PromotionRules.Evaluate(SouthAfricanGradeLevel.Grade10, six), "NPPPPR §29(1)");
    }

    [Fact]
    public void A_failed_subject_with_no_school_based_assessment_submitted_blocks_promotion()
    {
        // "provided the school-based assessment component is submitted in the
        // subject failed".
        var subjects = SevenSubjects(52m, 41m, 40m, 35m, 33m, 31m, 12m);
        subjects[6] = Other("History", 12m, sbaComplete: false);

        var evaluation = PromotionRules.Evaluate(SouthAfricanGradeLevel.Grade12, subjects);

        Assert.False(evaluation.MeetsRequirements);
        Assert.Contains(evaluation.Unmet, r => r.Description.Contains("subject failed"));
    }

    // ─── shape of the result ─────────────────────────────────────────────────

    [Fact]
    public void Every_requirement_is_reported_whether_or_not_it_is_met()
    {
        var evaluation = PromotionRules.Evaluate(
            SouthAfricanGradeLevel.Grade9,
            SeniorNine(55m, 45m, 42m, 48m, 44m, 41m, 35m, 32m, 20m));

        Assert.Equal(6, evaluation.Requirements.Count);
        Assert.All(evaluation.Requirements, r =>
        {
            Assert.StartsWith("NPPPPR", r.Clause);
            Assert.False(string.IsNullOrWhiteSpace(r.Description));
        });
    }

    [Fact]
    public void A_report_with_nothing_on_it_meets_nothing()
    {
        var evaluation = PromotionRules.Evaluate(SouthAfricanGradeLevel.Grade9, null);

        Assert.False(evaluation.MeetsRequirements);
        Assert.NotEmpty(evaluation.Unmet);
    }

    [Fact]
    public void Progression_is_never_recommended_by_the_rules()
    {
        // Moving a learner on despite not meeting the requirements, to keep them
        // from spending more than four years in a phase, is a judgement made in
        // a meeting — NPPPPR §(2b) — not something a rule can reach.
        var evaluation = PromotionRules.Evaluate(
            SouthAfricanGradeLevel.Grade9,
            SeniorNine(20m, 20m, 20m, 20m, 20m, 20m, 20m, 20m, 20m));

        Assert.Equal(PromotionDecision.Retained, evaluation.Recommended);
        Assert.NotEqual(PromotionDecision.ProgressedWithSupport, evaluation.Recommended);
    }
}
