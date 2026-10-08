using psms.Domain.Shared.Enums;
using System;

namespace psms.Domain.Assessment
{
    /// <summary>
    /// RC-20. What level a language subject is offered at.
    /// <para>
    /// National Protocol §17(6): "In the case of Languages, each language that
    /// the learner offers should be recorded and reported on separately
    /// according to the different levels on which they are offered."
    /// </para>
    /// <para>
    /// The level is stored on the subject. Where it has not been set, it is read
    /// out of the subject's name, because that is where every existing school
    /// has already put it — "English Home Language", "IsiZulu First Additional
    /// Language". That fallback is what lets this ship without a data migration
    /// guessing at thousands of rows: a school that corrects a subject's level
    /// overrides the guess permanently, and one that never touches it keeps
    /// working.
    /// </para>
    /// </summary>
    public static class SubjectLanguageRules
    {
        /// <summary>
        /// The level this subject is offered at: what the school set, or failing
        /// that what its name says, or null when it is not a language.
        /// </summary>
        public static LanguageLevel? ResolveLevel(
            string subjectName,
            LanguageLevel? storedLevel)
        {
            if (storedLevel.HasValue)
                return storedLevel.Value;

            return InferFromName(subjectName);
        }

        /// <summary>
        /// The level a subject's name carries, or null.
        /// <para>
        /// Deliberately ordered: "second additional" has to be tested before
        /// "additional", or every Second Additional Language reads as a First
        /// Additional one. Afrikaans is matched alongside English because a
        /// school's subject list is written in whichever the school uses.
        /// </para>
        /// </summary>
        public static LanguageLevel? InferFromName(string subjectName)
        {
            var name = Normalise(subjectName);
            if (name.Length == 0)
                return null;

            if (name.Contains("second additional")
                || name.Contains("tweede addisionele"))
                return LanguageLevel.SecondAdditionalLanguage;

            if (name.Contains("first additional")
                || name.Contains("eerste addisionele"))
                return LanguageLevel.FirstAdditionalLanguage;

            if (name.Contains("home language")
                || name.Contains("huistaal"))
                return LanguageLevel.HomeLanguage;

            return null;
        }

        /// <summary>
        /// How a level is written on a report card — the Protocol's own form.
        /// </summary>
        public static string LabelFor(LanguageLevel? level) => level switch
        {
            LanguageLevel.HomeLanguage => "Home Language",
            LanguageLevel.FirstAdditionalLanguage => "First Additional Language",
            LanguageLevel.SecondAdditionalLanguage => "Second Additional Language",
            _ => null,
        };

        /// <summary>
        /// The subject as it should read on a card: its name, with the level
        /// appended only when the name does not already carry it.
        /// <para>
        /// §17(6) wants the level reported. Most schools have already put it in
        /// the name, and "English Home Language (Home Language)" helps nobody —
        /// so the level is added only where it would otherwise be missing.
        /// </para>
        /// </summary>
        public static string NameWithLevel(string subjectName, LanguageLevel? storedLevel)
        {
            var level = ResolveLevel(subjectName, storedLevel);
            if (!level.HasValue)
                return subjectName;

            // Already in the name — which is the usual case.
            if (InferFromName(subjectName) == level)
                return subjectName;

            var label = LabelFor(level);
            return string.IsNullOrWhiteSpace(subjectName) ? label : $"{subjectName} ({label})";
        }

        /// <summary>
        /// Lower-cased with diacritics folded, so "Afrikaans Huistaal" and an
        /// accented spelling are the same word. Mirrors
        /// <c>SubjectAssessmentRules</c>, which matches subjects the same way
        /// and for the same reason.
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
