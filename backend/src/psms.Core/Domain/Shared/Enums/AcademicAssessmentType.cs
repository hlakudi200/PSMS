namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// The kind of academic assessment a teacher sets for a class subject.
    /// <para>
    /// Mirrors the frontend AssessmentType, which has always sent these
    /// meanings; the backend was reading them through the admissions enum
    /// instead (RC-13).
    /// </para>
    /// <para>
    /// The split between <see cref="Exam"/> and the rest is what makes a
    /// year-end mark computable: DBE Circular S8 of 2023 weights School-Based
    /// Assessment against the end-of-year examination by phase.
    /// </para>
    /// </summary>
    public enum AcademicAssessmentType
    {
        /// <summary>A class test. Counts towards School-Based Assessment.</summary>
        Test = 1,

        /// <summary>Counts towards School-Based Assessment.</summary>
        Assignment = 2,

        /// <summary>
        /// A formal examination. Weighted against the SBA total rather than
        /// counted inside it.
        /// </summary>
        Exam = 3,

        /// <summary>Counts towards School-Based Assessment.</summary>
        Practical = 4,

        /// <summary>Counts towards School-Based Assessment.</summary>
        Oral = 5,

        /// <summary>Counts towards School-Based Assessment.</summary>
        Project = 6,

        /// <summary>Counts towards School-Based Assessment.</summary>
        Other = 7
    }
}
