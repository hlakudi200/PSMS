using System;

namespace psms.Domain.Assessment
{
    /// <summary>
    /// How a mark is rounded. Prescribed, not a matter of taste.
    /// </summary>
    public static class CapsRounding
    {
        /// <summary>
        /// The promotion mark, as a whole number.
        /// <para>
        /// <b>NPPPPR §31(3)</b>: "the final mark is <i>rounded down if the first
        /// decimal is less than 5 and rounded up if the decimal is 5 and
        /// above</i>, e.g. a final mark of 70,3 will be rounded down to 70 and a
        /// final mark of 70.6 is rounded up to 71."
        /// </para>
        /// <para>
        /// This is half-up, and it is worth spelling out rather than reaching
        /// for <see cref="Math.Round(decimal)"/>, whose default is banker's
        /// rounding: that would turn 70.5 into 70, which the policy says must be
        /// 71.
        /// </para>
        /// </summary>
        public static int PromotionMark(decimal mark) => (int)Math.Floor(mark + 0.5m);

        /// <summary>
        /// A mark as it is stored and reported during the year: two decimals,
        /// half away from zero, which is what the columns hold. The promotion
        /// mark at year end is a whole number — see
        /// <see cref="PromotionMark"/>.
        /// </summary>
        public static decimal ReportedMark(decimal mark) =>
            Math.Round(mark, 2, MidpointRounding.AwayFromZero);
    }
}
