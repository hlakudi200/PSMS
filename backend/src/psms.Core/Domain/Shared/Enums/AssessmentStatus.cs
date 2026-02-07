namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// Status for admission assessments/placement tests
    /// </summary>
    public enum AssessmentStatus
    {
        Scheduled = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        NoShow = 5
    }
}
