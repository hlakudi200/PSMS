using psms.Domain.Shared.Enums;
using System;
using System.Globalization;

namespace psms.Domain.Assessment
{
    /// <summary>
    /// RC-21. How a card says what the learner did last term.
    /// <para>
    /// National Protocol §25(8)(d): feedback "should contain comments about the
    /// learner's performance <i>in relation to his or her previous
    /// performance</i>". The card cannot write the teacher's commentary, but it
    /// can put last term's result beside this one so the comparison is on the
    /// page rather than in the reader's memory.
    /// </para>
    /// <para>
    /// The phrasing lives here rather than in the PDF generator because the
    /// browser print view has to say exactly the same thing — RC-07 — and two
    /// copies of a sentence drift.
    /// </para>
    /// </summary>
    public static class PreviousPerformance
    {
        /// <summary>
        /// "Term 1: 64.2% (up 3.2)", or for the Foundation Phase "Term 1: 5 —
        /// Substantial achievement". Null when there is nothing to compare with,
        /// which is the normal case for a learner's first card of the year.
        /// </summary>
        public static string Describe(
            string previousTermName,
            decimal? previousOverall,
            CapsAchievementLevel? previousLevel,
            decimal? currentOverall,
            bool reportsPercentages)
        {
            var label = string.IsNullOrWhiteSpace(previousTermName)
                ? "Previous term"
                : previousTermName;

            // RC-19 applies to the comparison as much as to the marks: the
            // Foundation Phase does not report a percentage, so it does not
            // report last term's percentage either.
            if (!reportsPercentages)
            {
                return previousLevel.HasValue
                    ? $"{label}: {(int)previousLevel.Value} — "
                      + CapsAchievementScale.DescriptorFor(previousLevel.Value)
                    : null;
            }

            if (!previousOverall.HasValue) return null;

            var then = previousOverall.Value;
            var movement = currentOverall.HasValue
                ? Movement(currentOverall.Value - then)
                : null;

            return $"{label}: {then.ToString("F1", CultureInfo.InvariantCulture)}%"
                + (movement == null ? string.Empty : $" ({movement})");
        }

        /// <summary>
        /// "up 3.2", "down 1.5" or "unchanged". A signed number on a report card
        /// reads as a mark; words do not.
        /// </summary>
        private static string Movement(decimal delta)
        {
            var rounded = decimal.Round(delta, 1, MidpointRounding.AwayFromZero);
            if (rounded == 0) return "unchanged";

            var size = Math.Abs(rounded).ToString("F1", CultureInfo.InvariantCulture);

            return rounded > 0 ? $"up {size}" : $"down {size}";
        }
    }
}
