using psms.Domain.Academic;
using psms.Domain.Shared.Enums;
using Shouldly;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-25. The retention limits — NPPPPR §8(4), §21(2) and §29(2).
/// <para>
/// "A learner may only be retained <b>once in the intermediate phase</b> in
/// order to prevent the learner being retained in this phase for longer than
/// four years", and the same of the senior and further education and training
/// phases.
/// </para>
/// <para>
/// The rule is written per <i>phase</i>, not per grade, so the phase a grade
/// belongs to is the thing that has to be right. These pin that mapping and the
/// two limits; the standing itself is read from enrolment and report records in
/// <c>ReportAppService</c>.
/// </para>
/// </summary>
public class RetentionLimits_Tests
{
    [Theory]
    [InlineData(SouthAfricanGradeLevel.GradeR, SouthAfricanSchoolPhase.Foundation)]
    [InlineData(SouthAfricanGradeLevel.Grade1, SouthAfricanSchoolPhase.Foundation)]
    [InlineData(SouthAfricanGradeLevel.Grade3, SouthAfricanSchoolPhase.Foundation)]
    [InlineData(SouthAfricanGradeLevel.Grade4, SouthAfricanSchoolPhase.Intermediate)]
    [InlineData(SouthAfricanGradeLevel.Grade6, SouthAfricanSchoolPhase.Intermediate)]
    [InlineData(SouthAfricanGradeLevel.Grade7, SouthAfricanSchoolPhase.Senior)]
    [InlineData(SouthAfricanGradeLevel.Grade9, SouthAfricanSchoolPhase.Senior)]
    [InlineData(SouthAfricanGradeLevel.Grade10, SouthAfricanSchoolPhase.FET)]
    [InlineData(SouthAfricanGradeLevel.Grade12, SouthAfricanSchoolPhase.FET)]
    public void A_grade_belongs_to_its_phase(
        SouthAfricanGradeLevel grade, SouthAfricanSchoolPhase expected)
    {
        SchoolPhases.PhaseFor(grade).ShouldBe(expected);
    }

    [Fact]
    public void The_phase_boundaries_are_where_the_policy_puts_them()
    {
        // The two that matter most, because a learner retained either side of
        // them is counted against a different limit.
        SchoolPhases.PhaseFor(SouthAfricanGradeLevel.Grade3)
            .ShouldNotBe(SchoolPhases.PhaseFor(SouthAfricanGradeLevel.Grade4));

        SchoolPhases.PhaseFor(SouthAfricanGradeLevel.Grade6)
            .ShouldNotBe(SchoolPhases.PhaseFor(SouthAfricanGradeLevel.Grade7));

        SchoolPhases.PhaseFor(SouthAfricanGradeLevel.Grade9)
            .ShouldNotBe(SchoolPhases.PhaseFor(SouthAfricanGradeLevel.Grade10));
    }

    [Fact]
    public void A_learner_may_be_retained_once_in_a_phase()
    {
        SchoolPhases.MaxRetentionsPerPhase.ShouldBe(1);
    }

    [Fact]
    public void And_may_not_spend_longer_than_four_years_in_one()
    {
        // Which is what the single retention exists to enforce: a three-year
        // phase plus one repeated year.
        SchoolPhases.MaxYearsPerPhase.ShouldBe(4);
    }

    [Theory]
    [InlineData(SouthAfricanSchoolPhase.Foundation, "Foundation Phase")]
    [InlineData(SouthAfricanSchoolPhase.Intermediate, "Intermediate Phase")]
    [InlineData(SouthAfricanSchoolPhase.Senior, "Senior Phase")]
    [InlineData(SouthAfricanSchoolPhase.FET, "Further Education and Training Phase")]
    public void Each_phase_is_named_as_a_reader_would_name_it(
        SouthAfricanSchoolPhase phase, string expected)
    {
        SchoolPhases.NameFor(phase).ShouldBe(expected);
    }
}
