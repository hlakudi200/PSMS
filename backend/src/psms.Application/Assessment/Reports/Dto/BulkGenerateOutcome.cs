namespace psms.Assessment.Reports.Dto;

/// <summary>
/// What happened to one learner in a bulk generation run, or what would happen
/// to them on a preview.
/// </summary>
public enum BulkGenerateOutcome
{
    /// <summary>Preview only: this learner would get a report card.</summary>
    Eligible = 1,

    /// <summary>A report card was created.</summary>
    Generated = 2,

    /// <summary>A report already exists for this learner, term and type. Left alone.</summary>
    SkippedExisting = 3,

    /// <summary>A business rule blocks this learner — incomplete marks, for example.</summary>
    Blocked = 4,

    /// <summary>Generation was attempted and threw.</summary>
    Failed = 5,
}
