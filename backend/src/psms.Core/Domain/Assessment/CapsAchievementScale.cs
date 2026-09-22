using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;

namespace psms.Domain.Assessment
{
    /// <summary>
    /// The CAPS seven-level achievement scale — the single place a percentage
    /// becomes a level, and a level becomes words.
    /// <para>
    /// Source: <b>National Protocol for Assessment Grades R-12</b> (as published
    /// with the National Policy Pertaining to the Programme and Promotion
    /// Requirements, February 2021), Section 4, "Scale of achievement for the
    /// National Curriculum Statement Grades R-12".
    /// </para>
    /// <list type="table">
    ///   <item><description>7 — Outstanding achievement — 80-100%</description></item>
    ///   <item><description>6 — Meritorious achievement — 70-79%</description></item>
    ///   <item><description>5 — Substantial achievement — 60-69%</description></item>
    ///   <item><description>4 — Adequate achievement — 50-59%</description></item>
    ///   <item><description>3 — Moderate achievement — 40-49%</description></item>
    ///   <item><description>2 — Elementary achievement — 30-39%</description></item>
    ///   <item><description>1 — Not achieved — 0-29%</description></item>
    /// </list>
    /// <para>
    /// RC-07: this logic used to be copied into <c>Mark</c>, <c>ReportSubject</c>,
    /// <c>ReportAppService</c>, <c>ReportSubjectAppService</c> and the PDF data
    /// loader. All five agreed, but nothing kept them in step. Everything that
    /// needs a level now calls here.
    /// </para>
    /// </summary>
    public static class CapsAchievementScale
    {
        /// <summary>The lowest percentage that still earns each level.</summary>
        public static readonly IReadOnlyDictionary<CapsAchievementLevel, int> Floor =
            new Dictionary<CapsAchievementLevel, int>
            {
                [CapsAchievementLevel.Level7] = 80,
                [CapsAchievementLevel.Level6] = 70,
                [CapsAchievementLevel.Level5] = 60,
                [CapsAchievementLevel.Level4] = 50,
                [CapsAchievementLevel.Level3] = 40,
                [CapsAchievementLevel.Level2] = 30,
                [CapsAchievementLevel.Level1] = 0
            };

        /// <summary>
        /// The achievement level for a percentage. A percentage outside 0-100 is
        /// still placed on the scale rather than rejected — the caller that
        /// produced it is the one that should have validated it, and a report
        /// card must not fail to render over it.
        /// </summary>
        public static CapsAchievementLevel LevelFor(decimal percentage)
        {
            if (percentage >= 80) return CapsAchievementLevel.Level7;
            if (percentage >= 70) return CapsAchievementLevel.Level6;
            if (percentage >= 60) return CapsAchievementLevel.Level5;
            if (percentage >= 50) return CapsAchievementLevel.Level4;
            if (percentage >= 40) return CapsAchievementLevel.Level3;
            if (percentage >= 30) return CapsAchievementLevel.Level2;
            return CapsAchievementLevel.Level1;
        }

        /// <summary>As <see cref="LevelFor(decimal)"/>, passing null through.</summary>
        public static CapsAchievementLevel? LevelFor(decimal? percentage) =>
            percentage.HasValue ? LevelFor(percentage.Value) : (CapsAchievementLevel?)null;

        /// <summary>
        /// The policy descriptor for a level — "Outstanding achievement" — as
        /// the National Protocol words it.
        /// </summary>
        public static string DescriptorFor(CapsAchievementLevel level) => level switch
        {
            CapsAchievementLevel.Level7 => "Outstanding achievement",
            CapsAchievementLevel.Level6 => "Meritorious achievement",
            CapsAchievementLevel.Level5 => "Substantial achievement",
            CapsAchievementLevel.Level4 => "Adequate achievement",
            CapsAchievementLevel.Level3 => "Moderate achievement",
            CapsAchievementLevel.Level2 => "Elementary achievement",
            CapsAchievementLevel.Level1 => "Not achieved",
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };

        /// <summary>
        /// The descriptor shortened to what fits a report-card column —
        /// "Outstanding", "Not Achieved".
        /// </summary>
        public static string ShortDescriptorFor(CapsAchievementLevel level) => level switch
        {
            CapsAchievementLevel.Level7 => "Outstanding",
            CapsAchievementLevel.Level6 => "Meritorious",
            CapsAchievementLevel.Level5 => "Substantial",
            CapsAchievementLevel.Level4 => "Adequate",
            CapsAchievementLevel.Level3 => "Moderate",
            CapsAchievementLevel.Level2 => "Elementary",
            CapsAchievementLevel.Level1 => "Not Achieved",
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };

        /// <summary>
        /// How a level is printed on a report card — "7 - Outstanding". Null in,
        /// null out, so a subject with no mark prints its own placeholder.
        /// </summary>
        public static string LabelFor(CapsAchievementLevel? level) =>
            level.HasValue ? $"{(int)level.Value} - {ShortDescriptorFor(level.Value)}" : null;

        /// <summary>The seven levels, highest first, as a report-card legend reads.</summary>
        public static IReadOnlyList<CapsAchievementLevel> Descending { get; } = new[]
        {
            CapsAchievementLevel.Level7,
            CapsAchievementLevel.Level6,
            CapsAchievementLevel.Level5,
            CapsAchievementLevel.Level4,
            CapsAchievementLevel.Level3,
            CapsAchievementLevel.Level2,
            CapsAchievementLevel.Level1
        };

        /// <summary>
        /// The percentage band a level covers, for the legend printed at the
        /// foot of a report card.
        /// </summary>
        public static (int Low, int High) RangeFor(CapsAchievementLevel level)
        {
            var low = Floor[level];
            var high = level == CapsAchievementLevel.Level7 ? 100 : Floor[level + 1] - 1;

            return (low, high);
        }
    }
}
