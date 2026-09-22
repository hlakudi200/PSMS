using psms.Domain.Assessment;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// Pins the national weighting table and the grade-to-band mapping.
///
/// These numbers come from DBE Circular S8 of 2023 and are not ours to change:
/// if a test here fails, either policy moved and the change is deliberate, or
/// something was edited by accident.
/// </summary>
public class AssessmentWeightingDefaults_Tests
{
    [Theory]
    [InlineData(AssessmentWeightingBand.Foundation, 100, 0)]
    [InlineData(AssessmentWeightingBand.Intermediate, 80, 20)]
    [InlineData(AssessmentWeightingBand.Senior, 60, 40)]
    [InlineData(AssessmentWeightingBand.Grade10And11, 40, 60)]
    [InlineData(AssessmentWeightingBand.Grade12, 25, 75)]
    public void Matches_Circular_S8_of_2023(AssessmentWeightingBand band, int sba, int exam)
    {
        Assert.Equal(sba, AssessmentWeightingDefaults.SbaFor(band));
        Assert.Equal(exam, AssessmentWeightingDefaults.ExamFor(band));
    }

    [Theory]
    [InlineData(SouthAfricanGradeLevel.GradeR, AssessmentWeightingBand.Foundation)]
    [InlineData(SouthAfricanGradeLevel.Grade3, AssessmentWeightingBand.Foundation)]
    [InlineData(SouthAfricanGradeLevel.Grade4, AssessmentWeightingBand.Intermediate)]
    [InlineData(SouthAfricanGradeLevel.Grade6, AssessmentWeightingBand.Intermediate)]
    [InlineData(SouthAfricanGradeLevel.Grade7, AssessmentWeightingBand.Senior)]
    [InlineData(SouthAfricanGradeLevel.Grade9, AssessmentWeightingBand.Senior)]
    [InlineData(SouthAfricanGradeLevel.Grade10, AssessmentWeightingBand.Grade10And11)]
    [InlineData(SouthAfricanGradeLevel.Grade11, AssessmentWeightingBand.Grade10And11)]
    [InlineData(SouthAfricanGradeLevel.Grade12, AssessmentWeightingBand.Grade12)]
    public void Maps_each_grade_to_its_band(SouthAfricanGradeLevel grade, AssessmentWeightingBand expected)
    {
        Assert.Equal(expected, AssessmentWeightingDefaults.BandFor(grade));
    }

    [Fact]
    public void Foundation_phase_sits_no_examination()
    {
        // NPPPPR §8(2): "the school-based assessment mark as determined during
        // the school year will be 100% of the total mark."
        Assert.Equal(0, AssessmentWeightingDefaults.ExamFor(AssessmentWeightingBand.Foundation));
    }

    [Fact]
    public void A_new_row_starts_at_the_policy_default()
    {
        var row = new AssessmentWeighting(Guid.NewGuid(), 1, AssessmentWeightingBand.Senior);

        Assert.Equal(60, row.SbaPercentage);
        Assert.Equal(40, row.ExamPercentage);
        Assert.True(row.MatchesPolicyDefault());
    }

    [Theory]
    [InlineData(60, 40, true)]
    [InlineData(100, 0, true)]
    [InlineData(0, 100, true)]
    [InlineData(60, 50, false)]   // over 100
    [InlineData(30, 40, false)]   // under 100
    [InlineData(-10, 110, false)] // out of range
    public void A_split_has_to_account_for_the_whole_mark(int sba, int exam, bool valid)
    {
        Assert.Equal(valid, AssessmentWeighting.IsValidSplit(sba, exam));
    }

    [Fact]
    public void A_school_may_depart_from_policy_and_it_is_visible()
    {
        var row = new AssessmentWeighting(Guid.NewGuid(), 1, AssessmentWeightingBand.Grade12);
        row.SetSplit(50, 50);

        Assert.Equal(50, row.SbaPercentage);
        Assert.False(row.MatchesPolicyDefault());

        row.ResetToPolicyDefault();

        Assert.Equal(25, row.SbaPercentage);
        Assert.Equal(75, row.ExamPercentage);
        Assert.True(row.MatchesPolicyDefault());
    }

    [Fact]
    public void An_invalid_split_is_refused_by_the_entity()
    {
        var row = new AssessmentWeighting(Guid.NewGuid(), 1, AssessmentWeightingBand.Senior);

        Assert.Throws<InvalidOperationException>(() => row.SetSplit(70, 40));
    }
}
