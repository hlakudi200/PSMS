using psms.Domain.Shared.Enums;

namespace psms.Domain.Assessment
{
    /// <summary>
    /// What a report card is allowed to say about a learner's performance, by
    /// phase.
    /// <para>
    /// National Protocol for Assessment Grades R–12, §17(4): "(a) Foundation
    /// Phase (Grades R–3): Record and report in national codes and their
    /// achievement descriptions. (b) Intermediate Phase (Grades 4–6): …
    /// descriptions <i>and percentages</i>. (c) Senior Phase (Grades 7–9): …
    /// descriptions and percentages. (d) Grades 10–12: Record in marks and
    /// report in percentages."
    /// </para>
    /// <para>
    /// A percentage is provided for from Grade 4 onward. For Grades R–3 the
    /// Protocol asks for the code and its achievement description and nothing
    /// else, so a Foundation Phase card reports "5 — Substantial achievement"
    /// where the rest of the school reports "68.0%".
    /// </para>
    /// </summary>
    public static class ReportingScale
    {
        /// <summary>
        /// Whether a card for this grade may print percentages at all — marks
        /// per subject, a class average, or an overall aggregate. False only
        /// for the Foundation Phase.
        /// </summary>
        public static bool ReportsPercentages(SouthAfricanGradeLevel grade) =>
            AssessmentWeightingDefaults.BandFor(grade) != AssessmentWeightingBand.Foundation;

        /// <summary>
        /// The same question where the grade is not known — an older card whose
        /// class or grade has since been removed. Percentages are the behaviour
        /// for every phase but one, so an unknown grade keeps them rather than
        /// silently stripping a card that is probably Grade 8's.
        /// </summary>
        public static bool ReportsPercentages(SouthAfricanGradeLevel? grade) =>
            !grade.HasValue || ReportsPercentages(grade.Value);
    }
}
