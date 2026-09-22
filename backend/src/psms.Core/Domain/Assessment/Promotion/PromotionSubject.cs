using psms.Domain.Shared.Enums;

namespace psms.Domain.Assessment.Promotion
{
    /// <summary>
    /// One subject as the promotion rules see it.
    /// </summary>
    public sealed class PromotionSubject
    {
        public PromotionSubject(
            string subjectName,
            SubjectRole role,
            decimal? finalMark,
            bool schoolBasedAssessmentComplete = true,
            bool isAdditionalSubject = false)
        {
            SubjectName = subjectName;
            Role = role;
            FinalMark = finalMark;
            SchoolBasedAssessmentComplete = schoolBasedAssessmentComplete;
            IsAdditionalSubject = isAdditionalSubject;
        }

        public string SubjectName { get; }

        public SubjectRole Role { get; }

        /// <summary>The promotion mark, or null when the subject was not marked.</summary>
        public decimal? FinalMark { get; }

        /// <summary>
        /// Whether the School-Based Assessment for this subject is complete.
        /// Both the Senior Phase and the FET clauses turn on it: the ninth
        /// subject in Grades 7-9 needs only its SBA completed (§21(1)), and in
        /// Grades 10-12 a subject may only be failed "provided the school-based
        /// assessment component is submitted in the subject failed" (§29(1)(a)).
        /// </summary>
        public bool SchoolBasedAssessmentComplete { get; }

        /// <summary>
        /// An optional extra subject rather than one of the required ones.
        /// NPPPPR §19(9): an additional language "will be regarded as an
        /// additional subject not to be taken into account for promotion
        /// requirements".
        /// </summary>
        public bool IsAdditionalSubject { get; }

        /// <summary>The CAPS level this mark earns, or null when unmarked.</summary>
        public CapsAchievementLevel? Level => CapsAchievementScale.LevelFor(FinalMark);

        /// <summary>Whether the mark reaches a given level.</summary>
        public bool Reaches(CapsAchievementLevel level) =>
            Level.HasValue && (int)Level.Value >= (int)level;

        /// <summary>Whether the mark reaches a given percentage.</summary>
        public bool Reaches(decimal percentage) =>
            FinalMark.HasValue && FinalMark.Value >= percentage;
    }
}
