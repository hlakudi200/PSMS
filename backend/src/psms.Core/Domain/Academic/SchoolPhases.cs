using psms.Domain.Shared.Enums;

namespace psms.Domain.Academic
{
    /// <summary>
    /// RC-25. Which phase a grade belongs to, and what the retention limit in
    /// that phase is.
    /// <para>
    /// The phase matters because the retention rules are written per phase, not
    /// per grade. NPPPPR §8(4): "a learner may only be retained <b>once in the
    /// intermediate phase</b> in order to prevent the learner being retained in
    /// this phase for longer than four years." §21(2) says the same of the
    /// senior phase and §29(2) of the further education and training phase.
    /// </para>
    /// <para>
    /// Derived from the grade rather than read off <c>Grade.SchoolPhase</c>: the
    /// stored value is a school's own data and can be wrong, and a wrong phase
    /// here would mis-answer "has this learner already been retained?" — which
    /// is a question about a child's year.
    /// </para>
    /// </summary>
    public static class SchoolPhases
    {
        /// <summary>The phase a grade falls in.</summary>
        public static SouthAfricanSchoolPhase PhaseFor(SouthAfricanGradeLevel grade) => grade switch
        {
            SouthAfricanGradeLevel.GradeR
                or SouthAfricanGradeLevel.Grade1
                or SouthAfricanGradeLevel.Grade2
                or SouthAfricanGradeLevel.Grade3 => SouthAfricanSchoolPhase.Foundation,

            SouthAfricanGradeLevel.Grade4
                or SouthAfricanGradeLevel.Grade5
                or SouthAfricanGradeLevel.Grade6 => SouthAfricanSchoolPhase.Intermediate,

            SouthAfricanGradeLevel.Grade7
                or SouthAfricanGradeLevel.Grade8
                or SouthAfricanGradeLevel.Grade9 => SouthAfricanSchoolPhase.Senior,

            _ => SouthAfricanSchoolPhase.FET,
        };

        /// <summary>How a phase is named to a reader.</summary>
        public static string NameFor(SouthAfricanSchoolPhase phase) => phase switch
        {
            SouthAfricanSchoolPhase.Foundation => "Foundation Phase",
            SouthAfricanSchoolPhase.Intermediate => "Intermediate Phase",
            SouthAfricanSchoolPhase.Senior => "Senior Phase",
            _ => "Further Education and Training Phase",
        };

        /// <summary>
        /// How many times a learner may be retained in a phase. One, everywhere
        /// the rule is stated.
        /// </summary>
        public const int MaxRetentionsPerPhase = 1;

        /// <summary>
        /// The longest a learner may spend in one phase, which is what the
        /// single-retention rule exists to enforce: a three-year phase plus one
        /// repeated year.
        /// </summary>
        public const int MaxYearsPerPhase = 4;
    }
}
