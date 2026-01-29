namespace psms.Domain.Shared.Enums
{

    /// <summary>
    /// Types of calendar events in a term
    /// </summary>
    public enum EventType
    {
        PublicHoliday = 1,
        SchoolHoliday = 2,
        Examination = 3,
        Assessment = 4,
        SportEvent = 5,
        CulturalEvent = 6,
        ParentMeeting = 7,
        StaffMeeting = 8,
        Other = 99
    }
}
