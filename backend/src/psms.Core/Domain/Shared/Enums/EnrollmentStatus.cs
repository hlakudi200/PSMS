namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// General enrollment status for services (transport, after-care, etc.)
    /// </summary>
    public enum EnrollmentStatus
    {
        Active = 1,
        Suspended = 2,
        Terminated = 3,
        Pending = 4
    }
}
