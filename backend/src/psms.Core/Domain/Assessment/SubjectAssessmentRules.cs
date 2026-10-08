using psms.Domain.Shared.Enums;
using System;

namespace psms.Domain.Assessment
{
    /// <summary>
    /// RC-15. Subject-level exceptions to the grade band's assessment split.
    /// <para>
    /// The band table (see <see cref="AssessmentWeightingDefaults"/>) is per
    /// grade band, so it cannot express a rule that applies to one subject. The
    /// national policy has one.
    /// </para>
    /// </summary>
    public static class SubjectAssessmentRules
    {
        /// <summary>
        /// Whether this subject is assessed entirely by the school in this
        /// grade, regardless of the band's split.
        /// <para>
        /// <b>NPPPPR §31(2)</b>: "the weighting for assessment in the subject
        /// <i>life orientation in grade 12 is an exception. The school-based
        /// assessment component will be 100% of the total mark.</i> In the
        /// National Senior Certificate examination the final promotion mark in
        /// Life Orientation will be based on internal assessment which must be
        /// externally moderated, as well as a Common Assessment Task which is
        /// externally set and moderated." The same exception applies in Grades
        /// 10 and 11.
        /// </para>
        /// <para>
        /// This is national policy, not a school setting, so it is applied from
        /// the subject's identity rather than waiting for somebody to tick a
        /// box. A school whose Life Orientation subject is recorded under a name
        /// this does not recognise will fall back to the band split — see the
        /// note on <see cref="IsLifeOrientation"/>.
        /// </para>
        /// </summary>
        public static bool IsFullySchoolBased(
            string subjectName,
            string subjectCode,
            SouthAfricanGradeLevel grade)
        {
            var band = AssessmentWeightingDefaults.BandFor(grade);

            if (band != AssessmentWeightingBand.Grade10And11 && band != AssessmentWeightingBand.Grade12)
                return false;

            return IsLifeOrientation(subjectName, subjectCode);
        }

        /// <summary>
        /// Whether a subject is Life Orientation, by the names and codes South
        /// African schools actually use for it — including the Afrikaans
        /// Lewensoriëntering, with and without its diacritic.
        /// <para>
        /// Matching on identity is unavoidable here: nothing in the data model
        /// says which national subject a school's row is. It is deliberately
        /// narrow — a false positive would wrongly drop a subject's examination
        /// — and a school that names the subject something else keeps the band
        /// split until a per-subject override exists.
        /// </para>
        /// </summary>
        public static bool IsLifeOrientation(string subjectName, string subjectCode)
        {
            var code = (subjectCode ?? string.Empty).Trim();

            if (code.Equals("LO", StringComparison.OrdinalIgnoreCase)
                || code.Equals("LIFO", StringComparison.OrdinalIgnoreCase)
                || code.Equals("LFOR", StringComparison.OrdinalIgnoreCase)
                || code.Equals("LORI", StringComparison.OrdinalIgnoreCase))
                return true;

            var name = Normalise(subjectName);

            return name.Contains("life orientation")
                || name.Contains("lewensorientering")
                || name.Contains("lewens orientering");
        }

        /// <summary>
        /// RC-23. Whether a Practical Assessment Task is a compulsory part of
        /// this subject's examination mark.
        /// <para>
        /// <b>National Protocol §7(1)</b>: "A Practical Assessment Task mark is a
        /// compulsory component of the final promotion mark for all candidates
        /// registered for the following National Senior Certificate subjects:
        /// (a) Agricultural Management Practices and Agricultural Technology;
        /// (b) Dance Studies, Design, Dramatic Arts, Music and Visual Arts;
        /// (c) Languages: Oral mark; (d) Technology: Civil, Electrical,
        /// Mechanical, and Engineering Graphics and Design; (e) Life
        /// Orientation; (f) Computer Applications Technology and Information
        /// Technology; and (g) Consumer Studies, Hospitality Studies and
        /// Tourism." <b>§7(2)</b>: "The Practical Assessment Tasks mark must
        /// count 25% of the end-of-year examination mark."
        /// </para>
        /// <para>
        /// The rule is about National Senior Certificate subjects, so it applies
        /// in the FET phase. Below Grade 10 a practical is ordinary school work
        /// and stays in the School-Based Assessment total.
        /// </para>
        /// <para>
        /// Life Orientation is on the §7(1) list but never reaches this rule in
        /// practice: NPPPPR §31(2) makes it 100% school-based in Grades 10-12,
        /// so it has no examination mark for a task to be 25% of. Its practical
        /// component — the Physical Education Task — is one of its school-based
        /// tasks. It is matched here anyway so the list is the policy's list
        /// rather than an edited one, and <see cref="IsFullySchoolBased"/>
        /// decides first.
        /// </para>
        /// <para>
        /// Matched on identity, like <see cref="IsLifeOrientation"/>, and for
        /// the same reason: nothing in the data model says which national
        /// subject a school's row is. Deliberately narrow — a false positive
        /// moves a quarter of a learner's examination mark.
        /// </para>
        /// </summary>
        public static bool RequiresPracticalAssessmentTask(
            string subjectName,
            string subjectCode,
            SouthAfricanGradeLevel grade)
        {
            var band = AssessmentWeightingDefaults.BandFor(grade);

            if (band != AssessmentWeightingBand.Grade10And11 && band != AssessmentWeightingBand.Grade12)
                return false;

            if (IsLifeOrientation(subjectName, subjectCode))
                return true;

            var name = Normalise(subjectName);
            if (name.Length == 0)
                return false;

            // §7(1)(c): every language carries an oral mark.
            if (name.Contains("home language")
                || name.Contains("first additional language")
                || name.Contains("second additional language"))
                return true;

            foreach (var subject in PracticalSubjects)
            {
                if (name.Contains(subject))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// The §7(1) subjects, normalised. Spelled out rather than abbreviated
        /// so the list can be read against the policy line by line.
        /// </summary>
        private static readonly string[] PracticalSubjects =
        {
            // (a)
            "agricultural management practices",
            "agricultural technology",
            // (b)
            "dance studies",
            "design",
            "dramatic arts",
            "music",
            "visual arts",
            // (d)
            "civil technology",
            "electrical technology",
            "mechanical technology",
            "engineering graphics and design",
            // (f)
            "computer applications technology",
            "information technology",
            // (g)
            "consumer studies",
            "hospitality studies",
            "tourism",
        };

        /// <summary>
        /// Lower-cased with the diacritics folded, so "Lewensoriëntering" and
        /// "Lewensorientering" are the same word.
        /// </summary>
        private static string Normalise(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var decomposed = value.Trim().ToLowerInvariant()
                .Normalize(System.Text.NormalizationForm.FormD);

            var builder = new System.Text.StringBuilder(decomposed.Length);

            foreach (var character in decomposed)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character)
                    != System.Globalization.UnicodeCategory.NonSpacingMark)
                    builder.Append(character);
            }

            return builder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }
    }
}
