namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// The level at which a language subject is offered.
    /// <para>
    /// National Protocol for Assessment Grades R–12, §17(6): "In the case of
    /// Languages, each language that the learner offers should be recorded and
    /// reported on separately according to the different levels on which they
    /// are offered. For example, Home Language – English, First Additional
    /// Language – IsiXhosa, Second Additional Language – Afrikaans."
    /// </para>
    /// <para>
    /// This is not a label. The promotion requirements are written in terms of
    /// these levels rather than in terms of subjects — NPPPPR §21(1) asks for
    /// "Adequate achievement (level 4) in one language at <i>Home Language
    /// level</i>" and "Moderate (level 3) in the second official language at
    /// <i>First Additional Language level</i>"; §29(1) asks for 40% in three
    /// subjects, "one of which is an official language at <i>Home Language
    /// level</i>". A rule that cannot tell the levels apart cannot evaluate
    /// either clause.
    /// </para>
    /// </summary>
    public enum LanguageLevel
    {
        /// <summary>The learner's strongest language, taught at the fullest level.</summary>
        HomeLanguage = 1,

        /// <summary>An additional official language, at the first additional level.</summary>
        FirstAdditionalLanguage = 2,

        /// <summary>A further language, at the second additional level.</summary>
        SecondAdditionalLanguage = 3
    }
}
