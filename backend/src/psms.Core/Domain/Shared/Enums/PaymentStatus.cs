namespace psms.Domain.Shared.Enums
{
    // ==================== Financial Module Enums ====================

    /// <summary>
    /// Payment status
    /// </summary>
    public enum PaymentStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4,
        Refunded = 5,
        PartiallyPaid = 6,
        Overdue = 7
    }
}
