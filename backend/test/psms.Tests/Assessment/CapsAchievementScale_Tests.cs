using psms.Domain.Assessment;
using psms.Domain.Shared.Enums;
using Xunit;

namespace psms.Tests.Assessment;

/// <summary>
/// RC-07. The CAPS seven-level scale, which used to be copied into five places.
/// The boundaries come from the National Protocol for Assessment and are not
/// ours to move: a failure here means either policy changed and the change was
/// deliberate, or the table was edited by accident.
/// </summary>
public class CapsAchievementScale_Tests
{
    [Theory]
    // Each band's floor, its ceiling, and a value inside it.
    [InlineData(0, CapsAchievementLevel.Level1)]
    [InlineData(29.99, CapsAchievementLevel.Level1)]
    [InlineData(30, CapsAchievementLevel.Level2)]
    [InlineData(39.99, CapsAchievementLevel.Level2)]
    [InlineData(40, CapsAchievementLevel.Level3)]
    [InlineData(49.99, CapsAchievementLevel.Level3)]
    [InlineData(50, CapsAchievementLevel.Level4)]
    [InlineData(59.99, CapsAchievementLevel.Level4)]
    [InlineData(60, CapsAchievementLevel.Level5)]
    [InlineData(69.99, CapsAchievementLevel.Level5)]
    [InlineData(70, CapsAchievementLevel.Level6)]
    [InlineData(79.99, CapsAchievementLevel.Level6)]
    [InlineData(80, CapsAchievementLevel.Level7)]
    [InlineData(100, CapsAchievementLevel.Level7)]
    public void A_percentage_lands_in_its_band(double percentage, CapsAchievementLevel expected)
    {
        Assert.Equal(expected, CapsAchievementScale.LevelFor((decimal)percentage));
    }

    [Fact]
    public void No_mark_is_no_level()
    {
        Assert.Null(CapsAchievementScale.LevelFor((decimal?)null));
        Assert.Null(CapsAchievementScale.LabelFor(null));
    }

    [Fact]
    public void A_percentage_outside_the_scale_is_still_placed()
    {
        // Nothing should be producing these, but a report card must render.
        Assert.Equal(CapsAchievementLevel.Level1, CapsAchievementScale.LevelFor(-5m));
        Assert.Equal(CapsAchievementLevel.Level7, CapsAchievementScale.LevelFor(140m));
    }

    [Theory]
    [InlineData(CapsAchievementLevel.Level7, "7 - Outstanding")]
    [InlineData(CapsAchievementLevel.Level4, "4 - Adequate")]
    [InlineData(CapsAchievementLevel.Level1, "1 - Not Achieved")]
    public void The_printed_label_is_unchanged(CapsAchievementLevel level, string expected)
    {
        // These strings already appear on issued report cards.
        Assert.Equal(expected, CapsAchievementScale.LabelFor(level));
    }

    [Theory]
    [InlineData(CapsAchievementLevel.Level7, 80, 100)]
    [InlineData(CapsAchievementLevel.Level6, 70, 79)]
    [InlineData(CapsAchievementLevel.Level4, 50, 59)]
    [InlineData(CapsAchievementLevel.Level1, 0, 29)]
    public void The_legend_band_matches_the_boundary_that_awards_it(
        CapsAchievementLevel level, int low, int high)
    {
        var (actualLow, actualHigh) = CapsAchievementScale.RangeFor(level);

        Assert.Equal(low, actualLow);
        Assert.Equal(high, actualHigh);

        // The printed band and the awarded level cannot disagree.
        Assert.Equal(level, CapsAchievementScale.LevelFor(actualLow));
        Assert.Equal(level, CapsAchievementScale.LevelFor(actualHigh));
    }

    [Fact]
    public void The_seven_levels_tile_the_whole_scale_without_a_gap()
    {
        var expectedNextFloor = 101;

        foreach (var level in CapsAchievementScale.Descending)
        {
            var (low, high) = CapsAchievementScale.RangeFor(level);

            Assert.Equal(expectedNextFloor - 1, high);
            expectedNextFloor = low;
        }

        Assert.Equal(0, expectedNextFloor);
    }
}
