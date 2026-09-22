namespace psms.Domain.Assessment.Promotion
{
    /// <summary>
    /// The part a subject plays in the promotion requirements.
    /// <para>
    /// The NPPPPR states every phase's requirements in terms of these roles, not
    /// in terms of subject names — "adequate achievement (level 4) in one
    /// official language at home language level", "moderate achievement (level
    /// 3) in the second required official language at first additional language
    /// level", "moderate achievement (level 3) in mathematics". A rule engine
    /// that cannot tell a Home Language from a First Additional Language cannot
    /// evaluate either clause.
    /// </para>
    /// <para>
    /// The data model does not carry language levels yet (see issue RC-20), so
    /// these are resolved from the subject's name for now. That is the one part
    /// of this that is a guess rather than a rule.
    /// </para>
    /// </summary>
    public enum SubjectRole
    {
        /// <summary>A required subject that is none of the below.</summary>
        Other = 0,

        /// <summary>An official language offered at Home Language level.</summary>
        HomeLanguage = 1,

        /// <summary>An official language offered at First Additional Language level.</summary>
        FirstAdditionalLanguage = 2,

        /// <summary>
        /// A language at Second Additional Language level. Offered as an
        /// optional subject, and NPPPPR §19(9) says an additional language
        /// "will be regarded as an additional subject not to be taken into
        /// account for promotion requirements".
        /// </summary>
        SecondAdditionalLanguage = 3,

        /// <summary>Mathematics, or Mathematical Literacy in the FET phase.</summary>
        Mathematics = 4
    }
}
