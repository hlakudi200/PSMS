using psms.Domain.Shared.Enums;
using System.Collections.Generic;
using System.Linq;

namespace psms.Domain.Assessment.Promotion
{
    /// <summary>
    /// RC-16. The national promotion and progression requirements, per phase.
    /// <para>
    /// Source: <b>National policy pertaining to the programme and promotion
    /// requirements of the National Curriculum Statement Grades R-12</b>
    /// (NPPPPR, February 2021). Every clause below is quoted where it is
    /// applied, so a reader can check the code against the policy without
    /// leaving the file.
    /// </para>
    /// <para>
    /// This produces <b>advice</b>, never a decision. NPPPPR §(2b) requires a
    /// special meeting of subject staff to evaluate a learner holistically, and
    /// then a meeting with the parent, before a learner is retained. The rules
    /// say whether the requirements are met and which are not; a person records
    /// the outcome.
    /// </para>
    /// <para>
    /// <b>Not modelled here:</b> the deaf-learner South African Sign Language
    /// substitutions (§19(1a), §21(1)(ba), §21(1)(ea)), the immigrant-learner
    /// exemptions (§21(1)(f)), the Grade 12 condonation of up to 2% in one
    /// subject (§29(1)(b), which applies to the external NSC examination and not
    /// to anything a school computes), and the practical music programme ratings
    /// (§29(1)(c)). Each is a real clause; each needs data we do not hold.
    /// </para>
    /// </summary>
    public static class PromotionRules
    {
        private const CapsAchievementLevel Adequate = CapsAchievementLevel.Level4;   // 50-59%
        private const CapsAchievementLevel Moderate = CapsAchievementLevel.Level3;   // 40-49%
        private const CapsAchievementLevel Elementary = CapsAchievementLevel.Level2; // 30-39%

        public static PromotionEvaluation Evaluate(
            SouthAfricanGradeLevel grade,
            IReadOnlyList<PromotionSubject> subjects)
        {
            var required = (subjects ?? new List<PromotionSubject>())
                .Where(s => !s.IsAdditionalSubject)
                .ToList();

            var requirements = grade switch
            {
                SouthAfricanGradeLevel.GradeR => GradeR(required),
                SouthAfricanGradeLevel.Grade1 or SouthAfricanGradeLevel.Grade2 or SouthAfricanGradeLevel.Grade3
                    => FoundationPhase(required),
                SouthAfricanGradeLevel.Grade4 or SouthAfricanGradeLevel.Grade5 or SouthAfricanGradeLevel.Grade6
                    => IntermediatePhase(required),
                SouthAfricanGradeLevel.Grade7 or SouthAfricanGradeLevel.Grade8 or SouthAfricanGradeLevel.Grade9
                    => SeniorPhase(required),
                _ => FurtherEducationAndTraining(required)
            };

            var meets = requirements.All(r => r.IsMet);

            return new PromotionEvaluation(
                grade,
                meets,
                meets ? PromotionDecision.Promoted : PromotionDecision.Retained,
                requirements);
        }

        // ── Grade R ──────────────────────────────────────────────────────────

        /// <summary>
        /// NPPPPR §8(2): "the following are guidelines for determining a
        /// learner's progress in grade R: (a) adequate achievement (level 4)
        /// (50%-59%) in one official language at home language level; and
        /// (b) moderate achievement (level 3) (40%-49%) in mathematics."
        /// </summary>
        private static List<PromotionRequirement> GradeR(IReadOnlyList<PromotionSubject> subjects) =>
            new List<PromotionRequirement>
            {
                AtLevel(subjects, SubjectRole.HomeLanguage, Adequate, 1,
                    "NPPPPR §8(2)(a)", "Adequate achievement (level 4) in one official language at Home Language level"),
                AtLevel(subjects, SubjectRole.Mathematics, Moderate, 1,
                    "NPPPPR §8(2)(b)", "Moderate achievement (level 3) in Mathematics"),
            };

        // ── Grades 1-3 ───────────────────────────────────────────────────────

        /// <summary>
        /// NPPPPR §8(3), the guidelines for progressing from Grade 1 to 2 and
        /// Grade 2 to 3: adequate achievement (level 4) in one official language
        /// at Home Language level; moderate achievement (level 3) in the second
        /// required official language at First Additional Language level; and
        /// moderate achievement (level 3) in Mathematics.
        /// </summary>
        private static List<PromotionRequirement> FoundationPhase(IReadOnlyList<PromotionSubject> subjects) =>
            new List<PromotionRequirement>
            {
                AtLevel(subjects, SubjectRole.HomeLanguage, Adequate, 1,
                    "NPPPPR §8(3)(a)", "Adequate achievement (level 4) in one official language at Home Language level"),
                AtLevel(subjects, SubjectRole.FirstAdditionalLanguage, Moderate, 1,
                    "NPPPPR §8(3)(b)", "Moderate achievement (level 3) in the second official language at First Additional Language level"),
                AtLevel(subjects, SubjectRole.Mathematics, Moderate, 1,
                    "NPPPPR §8(3)(c)", "Moderate achievement (level 3) in Mathematics"),
            };

        // ── Grades 4-6 ───────────────────────────────────────────────────────

        /// <summary>
        /// NPPPPR §14(2), progression from Grade 4 to 6: as the Foundation
        /// Phase, plus "(d) … moderate achievement (level 3) (40%-49%) in any
        /// other two (2) of the remaining approved subjects".
        /// </summary>
        private static List<PromotionRequirement> IntermediatePhase(IReadOnlyList<PromotionSubject> subjects) =>
            new List<PromotionRequirement>
            {
                AtLevel(subjects, SubjectRole.HomeLanguage, Adequate, 1,
                    "NPPPPR §14(2)(a)", "Adequate achievement (level 4) in one official language at Home Language level"),
                AtLevel(subjects, SubjectRole.FirstAdditionalLanguage, Moderate, 1,
                    "NPPPPR §14(2)(b)", "Moderate achievement (level 3) in the second official language at First Additional Language level"),
                AtLevel(subjects, SubjectRole.Mathematics, Moderate, 1,
                    "NPPPPR §14(2)(c)", "Moderate achievement (level 3) in Mathematics"),
                OtherSubjectsAtLevel(subjects, Moderate, 2, Enumerable.Empty<string>(),
                    "NPPPPR §14(2)(d)", "Moderate achievement (level 3) in any other two of the remaining approved subjects"),
            };

        // ── Grades 7-9 ───────────────────────────────────────────────────────

        /// <summary>
        /// NPPPPR §21(1): "learners in grades 7-9 will be promoted from grade to
        /// grade if they have offered nine (9) subjects … and have complied with
        /// the promotion requirements in eight (8) of the subjects …, provided
        /// the school-based assessment component of the ninth subject has been
        /// completed."
        /// <para>
        /// The eight are the clauses themselves: one Home Language, one First
        /// Additional Language, Mathematics, three of the other required
        /// subjects at level 3, and two more at level 2.
        /// </para>
        /// </summary>
        private static List<PromotionRequirement> SeniorPhase(IReadOnlyList<PromotionSubject> subjects)
        {
            var atModerate = TakeOthersAtLevel(subjects, Moderate, 3);

            return new List<PromotionRequirement>
            {
                AtLevel(subjects, SubjectRole.HomeLanguage, Adequate, 1,
                    "NPPPPR §21(1)(a)", "Adequate achievement (level 4) in one language at Home Language level"),
                AtLevel(subjects, SubjectRole.FirstAdditionalLanguage, Moderate, 1,
                    "NPPPPR §21(1)(b)", "Moderate achievement (level 3) in the second official language at First Additional Language level"),
                AtLevel(subjects, SubjectRole.Mathematics, Moderate, 1,
                    "NPPPPR §21(1)(c)", "Moderate achievement (level 3) in Mathematics"),
                OtherSubjectsAtLevel(subjects, Moderate, 3, Enumerable.Empty<string>(),
                    "NPPPPR §21(1)(d)", "Moderate achievement (level 3) in any three of the other required subjects"),
                // The two at level 2 must be subjects other than the three
                // already counted at level 3 — "any two (2) of the OTHER
                // required subjects".
                OtherSubjectsAtLevel(subjects, Elementary, 2, atModerate,
                    "NPPPPR §21(1)(e)", "At least elementary achievement (level 2) in any two of the other required subjects"),
                NinthSubjectSbaComplete(subjects),
            };
        }

        /// <summary>
        /// The proviso in §21(1): whichever subject is not counted among the
        /// eight must still have its School-Based Assessment completed.
        /// <para>
        /// Checked over every required subject rather than only the uncounted
        /// ninth, which is stricter than the clause. Working out which subject
        /// is "the ninth" means fixing an assignment of subjects to clauses, and
        /// there is usually more than one; reporting every subject with
        /// outstanding School-Based Assessment says more, and the description
        /// says that is what it is doing.
        /// </para>
        /// </summary>
        private static PromotionRequirement NinthSubjectSbaComplete(IReadOnlyList<PromotionSubject> subjects)
        {
            var outstanding = subjects.Where(s => !s.SchoolBasedAssessmentComplete).ToList();

            return new PromotionRequirement(
                "NPPPPR §21(1)",
                "The School-Based Assessment component of every required subject is complete",
                outstanding.Count == 0,
                outstanding.Count == 0
                    ? null
                    : "Outstanding: " + string.Join(", ", outstanding.Select(s => s.SubjectName)));
        }

        // ── Grades 10-12 ─────────────────────────────────────────────────────

        /// <summary>
        /// NPPPPR §29(1): promoted if the learner "offered and completed the
        /// school-based assessment, practical assessment tasks, where
        /// applicable, oral assessment and end-of-year examination requirements
        /// in not fewer than seven (7) subjects", and "(a) achieved 40% in three
        /// subjects, one of which is an official language at home language
        /// level, and 30% in three subjects, provided the school-based
        /// assessment component is submitted in the subject failed."
        /// <para>
        /// Stated in percentages rather than levels, so it is evaluated that
        /// way: 40% is the level 3 floor and 30% the level 2 floor, but the
        /// clause says percentages and the code follows it.
        /// </para>
        /// </summary>
        private static List<PromotionRequirement> FurtherEducationAndTraining(IReadOnlyList<PromotionSubject> subjects)
        {
            var homeLanguageAt40 = subjects
                .FirstOrDefault(s => s.Role == SubjectRole.HomeLanguage && s.Reaches(40m));

            // The Home Language counts as one of the three at 40%.
            var othersAt40 = subjects
                .Where(s => s.Reaches(40m) && s != homeLanguageAt40)
                .OrderByDescending(s => s.FinalMark)
                .Take(2)
                .ToList();

            var countedAt40 = new List<PromotionSubject>();
            if (homeLanguageAt40 != null) countedAt40.Add(homeLanguageAt40);
            countedAt40.AddRange(othersAt40);

            var at30 = subjects
                .Where(s => s.Reaches(30m) && !countedAt40.Contains(s))
                .OrderByDescending(s => s.FinalMark)
                .Take(3)
                .ToList();

            var failedWithoutSba = subjects
                .Where(s => !s.Reaches(30m) && !s.SchoolBasedAssessmentComplete)
                .ToList();

            return new List<PromotionRequirement>
            {
                new PromotionRequirement(
                    "NPPPPR §29(1)",
                    "Offered and completed the assessment requirements in not fewer than seven subjects",
                    subjects.Count(s => s.SchoolBasedAssessmentComplete) >= 7,
                    $"{subjects.Count(s => s.SchoolBasedAssessmentComplete)} of {subjects.Count} subjects complete"),

                new PromotionRequirement(
                    "NPPPPR §29(1)(a)",
                    "40% in three subjects, one of which is an official language at Home Language level",
                    homeLanguageAt40 != null && countedAt40.Count >= 3,
                    homeLanguageAt40 == null
                        ? "No official language at Home Language level reaches 40%"
                        : Describe(countedAt40)),

                new PromotionRequirement(
                    "NPPPPR §29(1)(a)",
                    "30% in three further subjects",
                    at30.Count >= 3,
                    Describe(at30)),

                new PromotionRequirement(
                    "NPPPPR §29(1)(a)",
                    "The School-Based Assessment component is submitted in every subject failed",
                    failedWithoutSba.Count == 0,
                    failedWithoutSba.Count == 0
                        ? null
                        : "Outstanding: " + string.Join(", ", failedWithoutSba.Select(s => s.SubjectName))),
            };
        }

        // ── shared evaluation helpers ────────────────────────────────────────

        /// <summary>
        /// Requires <paramref name="count"/> subjects in a given role to reach a
        /// level.
        /// </summary>
        private static PromotionRequirement AtLevel(
            IReadOnlyList<PromotionSubject> subjects,
            SubjectRole role,
            CapsAchievementLevel level,
            int count,
            string clause,
            string description)
        {
            var candidates = subjects.Where(s => s.Role == role).ToList();
            var passing = candidates.Where(s => s.Reaches(level)).ToList();

            var detail = candidates.Count == 0
                ? "No subject on this report is recorded in that role"
                : Describe(passing.Count > 0 ? passing : candidates);

            return new PromotionRequirement(clause, description, passing.Count >= count, detail);
        }

        /// <summary>
        /// Requires <paramref name="count"/> of the remaining required subjects
        /// — anything that is not a language or Mathematics — to reach a level,
        /// excluding any already counted towards a higher clause.
        /// </summary>
        private static PromotionRequirement OtherSubjectsAtLevel(
            IReadOnlyList<PromotionSubject> subjects,
            CapsAchievementLevel level,
            int count,
            IEnumerable<string> alreadyCounted,
            string clause,
            string description)
        {
            var spent = new HashSet<string>(alreadyCounted ?? Enumerable.Empty<string>());

            var passing = Others(subjects)
                .Where(s => s.Reaches(level) && !spent.Contains(s.SubjectName))
                .OrderByDescending(s => s.FinalMark)
                .Take(count)
                .ToList();

            return new PromotionRequirement(clause, description, passing.Count >= count, Describe(passing));
        }

        /// <summary>
        /// The subject names that satisfy an "any N of the others" clause, so a
        /// later clause over the same pool does not count them twice.
        /// </summary>
        private static IEnumerable<string> TakeOthersAtLevel(
            IReadOnlyList<PromotionSubject> subjects,
            CapsAchievementLevel level,
            int count) =>
            Others(subjects)
                .Where(s => s.Reaches(level))
                .OrderByDescending(s => s.FinalMark)
                .Take(count)
                .Select(s => s.SubjectName)
                .ToList();

        /// <summary>
        /// The required subjects that are neither a language nor Mathematics.
        /// </summary>
        private static IEnumerable<PromotionSubject> Others(IReadOnlyList<PromotionSubject> subjects) =>
            subjects.Where(s => s.Role == SubjectRole.Other);

        private static string Describe(IEnumerable<PromotionSubject> subjects)
        {
            var items = subjects?.ToList() ?? new List<PromotionSubject>();

            return items.Count == 0
                ? "None"
                : string.Join(", ", items.Select(s => s.FinalMark.HasValue
                    ? s.SubjectName + " "
                        + s.FinalMark.Value.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)
                        + "%"
                    : s.SubjectName + " (no mark)"));
        }
    }
}
