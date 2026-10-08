using System;

namespace psms.Domain.Assessment
{
    /// <summary>
    /// RC-26. The two deadlines on a retention appeal — NPPPPR §(2c).
    /// <para>
    /// "the parent/guardian must submit a written request, <b>not later than
    /// three (3) days</b> after the official opening of schools, to the school
    /// principal … within <b>fourteen (14) working days</b> of receiving a
    /// request to appeal, the head of department or his/her designee shall make
    /// a final determination."
    /// </para>
    /// <para>
    /// <b>A stated limit.</b> "Working days" here excludes weekends only. South
    /// African public holidays are not modelled anywhere in this system, so a
    /// determination deadline that falls near Easter or the April holidays will
    /// read a day or two early. That is the safe direction to be wrong in — it
    /// shows the department less time than it has, rather than more — but it is
    /// a limit, not an accuracy, and a holiday calendar would remove it.
    /// </para>
    /// </summary>
    public static class AppealDeadlines
    {
        /// <summary>§(2c): three days after the official opening of schools.</summary>
        public const int DaysToLodge = 3;

        /// <summary>§(2c): fourteen working days to determine it.</summary>
        public const int WorkingDaysToDetermine = 14;

        /// <summary>
        /// The last day a parent may lodge an appeal: three calendar days after
        /// schools officially open.
        /// <para>
        /// Calendar days, not working days — §(2c) says "three (3) days" where
        /// it says "fourteen (14) working days" a clause later, and the
        /// difference is deliberate in the policy.
        /// </para>
        /// </summary>
        public static DateTime LodgingDeadline(DateTime schoolsOpenOn) =>
            schoolsOpenOn.Date.AddDays(DaysToLodge);

        /// <summary>
        /// The last day the head of department has to determine an appeal:
        /// fourteen working days from the day it was received.
        /// </summary>
        public static DateTime DeterminationDeadline(DateTime appealLodgedOn) =>
            AddWorkingDays(appealLodgedOn, WorkingDaysToDetermine);

        /// <summary>
        /// <paramref name="count"/> working days after a date, counting from the
        /// day after it and skipping weekends.
        /// <para>
        /// The day the appeal arrives is day zero: "within fourteen working days
        /// <i>of receiving</i> a request" starts the clock on receipt, so the
        /// first working day of the fourteen is the next one.
        /// </para>
        /// </summary>
        public static DateTime AddWorkingDays(DateTime from, int count)
        {
            if (count <= 0) return from.Date;

            var date = from.Date;
            var added = 0;

            while (added < count)
            {
                date = date.AddDays(1);

                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    added++;
            }

            return date;
        }
    }
}
