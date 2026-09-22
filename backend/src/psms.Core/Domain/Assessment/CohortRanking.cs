using System;
using System.Collections.Generic;
using System.Linq;

namespace psms.Domain.Assessment
{
    /// <summary>
    /// RC-06. How a learner's placing in the class is worked out, and how the
    /// class comparison figures printed beside their mark are derived.
    /// <para>
    /// Kept free of the database so the convention can be pinned by tests: the
    /// service that loads the cohort passes marks in and writes the answers back.
    /// </para>
    /// </summary>
    public static class CohortRanking
    {
        /// <summary>
        /// Positions, by <b>competition ranking</b> — the "1224" convention.
        /// Equal marks share the higher position and the next position skips
        /// the tie, so two learners on 82% are both 2nd and the next is 4th.
        /// <para>
        /// This is the convention South African schools print ("Position 2 of
        /// 30"): it keeps the position readable against the class size, which
        /// dense ranking ("1223") does not. Marks are compared as stored, to
        /// two decimals, so a tie is a genuine tie rather than a rounding
        /// artefact of the printed figure.
        /// </para>
        /// <para>
        /// Entries with no mark are absent from the result — an unranked
        /// learner has no position, rather than last place.
        /// </para>
        /// </summary>
        public static Dictionary<TKey, int> Positions<TKey>(
            IEnumerable<KeyValuePair<TKey, decimal?>> scores)
        {
            var positions = new Dictionary<TKey, int>();

            if (scores == null)
                return positions;

            var ordered = scores
                .Where(s => s.Value.HasValue)
                .OrderByDescending(s => s.Value.Value)
                .ToList();

            var position = 0;
            var seen = 0;
            decimal? previous = null;

            foreach (var entry in ordered)
            {
                seen++;

                // A new mark takes the position its ordinal gives it; a repeat
                // of the previous mark keeps that mark's position.
                if (previous == null || entry.Value.Value != previous.Value)
                    position = seen;

                positions[entry.Key] = position;
                previous = entry.Value;
            }

            return positions;
        }

        /// <summary>
        /// The class figures for one subject, or null when nobody in the class
        /// has a mark in it — better an empty column than "0.0" implying the
        /// whole class scored zero.
        /// </summary>
        public static CohortStatistics? Summarise(IEnumerable<decimal?> marks)
        {
            var values = (marks ?? Enumerable.Empty<decimal?>())
                .Where(m => m.HasValue)
                .Select(m => m.Value)
                .ToList();

            if (values.Count == 0)
                return null;

            return new CohortStatistics(
                Math.Round(values.Sum() / values.Count, 2, MidpointRounding.AwayFromZero),
                values.Max(),
                values.Min(),
                values.Count);
        }
    }

    /// <summary>How a class did in one subject.</summary>
    public readonly struct CohortStatistics
    {
        public CohortStatistics(decimal average, decimal highest, decimal lowest, int count)
        {
            Average = average;
            Highest = highest;
            Lowest = lowest;
            Count = count;
        }

        /// <summary>Mean of the marked learners, to two decimals.</summary>
        public decimal Average { get; }

        public decimal Highest { get; }

        public decimal Lowest { get; }

        /// <summary>How many learners the figures are drawn from.</summary>
        public int Count { get; }
    }
}
