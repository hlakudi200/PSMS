namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// The grade bands that carry their own School-Based Assessment to
    /// examination split, as DBE Circular S8 of 2023 expresses them.
    /// <para>
    /// Deliberately not <see cref="SouthAfricanSchoolPhase"/>: the FET phase
    /// splits, with Grades 10 and 11 weighted differently from Grade 12, so a
    /// phase alone cannot identify the weighting.
    /// </para>
    /// </summary>
    public enum AssessmentWeightingBand
    {
        /// <summary>Grades R-3. No examination component.</summary>
        Foundation = 1,

        /// <summary>Grades 4-6.</summary>
        Intermediate = 2,

        /// <summary>Grades 7-9.</summary>
        Senior = 3,

        /// <summary>Grades 10 and 11.</summary>
        Grade10And11 = 4,

        /// <summary>Grade 12. The examination is external.</summary>
        Grade12 = 5
    }
}
