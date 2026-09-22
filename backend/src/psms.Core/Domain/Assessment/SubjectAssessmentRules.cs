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
