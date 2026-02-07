namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// Fee status
    /// </summary>
    public enum FeeStatus
    {
        Pending = 1,
        Paid = 2,
        PartiallyPaid = 3,
        Overdue = 4,
        Waived = 5,
        Cancelled = 6
    }
}
