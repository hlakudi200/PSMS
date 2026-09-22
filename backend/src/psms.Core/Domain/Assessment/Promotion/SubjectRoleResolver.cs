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

            // Language levels first: "English Home Language" is a language
            // before it is anything else.
            if (name.Contains("home language") || name.Contains("huistaal")
                || code.EndsWith("HL", StringComparison.Ordinal))
                return SubjectRole.HomeLanguage;

            if (name.Contains("first additional language") || name.Contains("eerste addisionele taal")
                || code.EndsWith("FAL", StringComparison.Ordinal))
                return SubjectRole.FirstAdditionalLanguage;

            if (name.Contains("second additional language") || name.Contains("tweede addisionele taal")
                || code.EndsWith("SAL", StringComparison.Ordinal))
                return SubjectRole.SecondAdditionalLanguage;

            // Mathematics, and the FET-phase Mathematical Literacy, which counts
            // as the Mathematics requirement for a learner who offers it.
            if (name == "mathematics" || name == "wiskunde"
                || name.StartsWith("mathematical literacy", StringComparison.Ordinal)
                || name.StartsWith("wiskundige geletterdheid", StringComparison.Ordinal)
                || code == "MATH" || code == "MATHS" || code == "MLIT" || code == "WISK")
                return SubjectRole.Mathematics;

            return SubjectRole.Other;
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
