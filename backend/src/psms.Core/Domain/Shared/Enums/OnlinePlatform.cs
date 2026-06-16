namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// Online lesson platforms
    /// </summary>
    public enum OnlinePlatform
    {
        Zoom = 1,
        MicrosoftTeams = 2,
        GoogleMeet = 3,
        BigBlueButton = 4,
        WebEx = 5,
        Custom = 6,

        /// <summary>
        /// In-app live classroom powered by LiveKit (WebRTC). The lesson is
        /// hosted inside PSMS rather than an external link; participants join
        /// with a server-minted token and the room name is derived from the
        /// lesson id (no MeetingLink required). See LC-01.
        /// </summary>
        InApp = 7
    }
}
