using System;

namespace psms.Domain.Assessment.Promotion
{
    /// <summary>
    /// RC-16. Works out what part a subject plays in the promotion requirements,
    /// from what the school called it.
    /// <para>
    /// <b>This is the weak link, and it is deliberate.</b> The NPPPPR states
    /// every phase's requirements in terms of language levels — "one official
    /// language at home language level", "the second required official language
    /// at first additional language level" — and the data model does not carry a
    /// language level (issue RC-20). Until it does, the level has to be read off
    /// the subject's name, which is how South African schools write it anyway:
    /// "English Home Language", "isiZulu First Additional Language".
    /// </para>
    /// <para>
    /// A subject this cannot place comes back as <see cref="SubjectRole.Other"/>,
    /// which is the safe direction: it is counted among the remaining required
    /// subjects rather than being mistaken for the Home Language that a
    /// promotion turns on. The evaluation reports "no subject on this report is
    /// recorded in that role" rather than quietly passing.
    /// </para>
    /// </summary>
    public static class SubjectRoleResolver
    {
        public static SubjectRole Resolve(string subjectName, string subjectCode)
        {
            var name = Normalise(subjectName);
            var code = (subjectCode ?? string.Empty).Trim().ToUpperInvariant();

            // Mathematics first. A code suffix is a much weaker signal than the
            // subject's own name, and testing the suffixes first meant a subject
            // named "Mathematics" with the code MATHL — Maths Higher Level, a
            // real convention — resolved as a Home Language, satisfied the
            // language clause, and left Mathematics unrepresented.
            if (IsMathematics(name, code))
                return SubjectRole.Mathematics;

            // Language levels, written out or abbreviated. Schools write both
            // "English Home Language" and "English (HL)", and the parenthesised
            // form used to be looked for only in the code, so "English (HL)"
            // resolved to nothing and the learner was advised retained for
            // having no Home Language.
            if (name.Contains("home language") || name.Contains("huistaal")
                || HasToken(name, "hl") || code.EndsWith("HL", StringComparison.Ordinal))
                return SubjectRole.HomeLanguage;

            if (name.Contains("first additional language") || name.Contains("eerste addisionele taal")
                || HasToken(name, "fal") || code.EndsWith("FAL", StringComparison.Ordinal))
                return SubjectRole.FirstAdditionalLanguage;

            if (name.Contains("second additional language") || name.Contains("tweede addisionele taal")
                || HasToken(name, "sal") || code.EndsWith("SAL", StringComparison.Ordinal))
                return SubjectRole.SecondAdditionalLanguage;

            return SubjectRole.Other;
        }

        /// <summary>
        /// Mathematics, however the school writes it — "Maths", "Mathematics
        /// Grade 8", "Core Mathematics", "Wiskunde".
        /// <para>
        /// Mathematical Literacy is <b>not</b> matched: it is a different
        /// subject offered in place of Mathematics in the FET phase, where the
        /// promotion requirements are stated in percentages over all subjects
        /// and never single out Mathematics. Treating it as Mathematics would
        /// only matter in a phase that does not ask.
        /// </para>
        /// </summary>
        private static bool IsMathematics(string name, string code)
        {
            if (name.Contains("mathematical literacy") || name.Contains("wiskundige geletterdheid"))
                return false;

            return name.Contains("mathematics")
                || name.Contains("wiskunde")
                || HasToken(name, "maths")
                || HasToken(name, "math")
                || code == "MATH" || code == "MATHS" || code == "WISK";
        }

        /// <summary>
        /// Whether a short abbreviation appears as a word of its own, so "hl"
        /// matches "english (hl)" and "english hl" but not "highland studies".
        /// </summary>
        private static bool HasToken(string name, string token)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            var index = name.IndexOf(token, StringComparison.Ordinal);

            while (index >= 0)
            {
                var beforeOk = index == 0 || !char.IsLetterOrDigit(name[index - 1]);
                var after = index + token.Length;
                var afterOk = after >= name.Length || !char.IsLetterOrDigit(name[after]);

                if (beforeOk && afterOk)
                    return true;

                index = name.IndexOf(token, index + 1, StringComparison.Ordinal);
            }

            return false;
        }

        /// <summary>
        /// Lower-cased with diacritics folded and runs of whitespace collapsed,
        /// so "Eerste  Addisionele Taal" matches.
        /// </summary>
        private static string Normalise(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var decomposed = value.Trim().ToLowerInvariant()
                .Normalize(System.Text.NormalizationForm.FormD);

            var builder = new System.Text.StringBuilder(decomposed.Length);
            var lastWasSpace = false;

            foreach (var character in decomposed)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(character)
                    == System.Globalization.UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsWhiteSpace(character))
                {
                    if (!lastWasSpace)
                        builder.Append(' ');

                    lastWasSpace = true;
                    continue;
                }

                builder.Append(character);
                lastWasSpace = false;
            }

            return builder.ToString().Trim().Normalize(System.Text.NormalizationForm.FormC);
        }
    }
}
