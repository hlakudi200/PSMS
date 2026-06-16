namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// What a client needs to join the in-app live classroom (LC-01): the LiveKit
/// server URL, a signed access token scoped to this lesson's room, and whether
/// this participant may publish (the hosting teacher) or only watch (students).
/// </summary>
public class LiveClassJoinDto
{
    public string ServerUrl { get; set; }
    public string Token { get; set; }
    public string RoomName { get; set; }
    public string Identity { get; set; }
    public bool CanPublish { get; set; }
}
