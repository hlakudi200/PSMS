namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// RC-17. How a learner's conduct and diligence are rated on a report card.
    /// <para>
    /// RE-002 lists conduct and diligence ratings among the fields a South
    /// African report card must carry. Nothing in the National Protocol or the
    /// NPPPPR prescribes a scale for them — the seven achievement levels are for
    /// subjects — so this is a school convention rather than a national one, and
    /// the wording is chosen to read as behaviour rather than as marks.
    /// </para>
    /// <para>
    /// Seven points, matching the shape of the achievement scale so a reader
    /// is not switching between two different-sized rulers on one page.
    /// </para>
    /// </summary>
    public enum ConductDiligenceRating
    {
        Unsatisfactory = 1,
        Poor = 2,
        NeedsImprovement = 3,
        Satisfactory = 4,
        Good = 5,
        VeryGood = 6,
        Excellent = 7
    }
}
