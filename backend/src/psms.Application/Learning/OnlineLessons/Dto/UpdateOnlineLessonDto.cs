namespace psms.Learning.OnlineLessons.Dto;

/// <summary>
/// Input DTO for updating an online lesson. All fields nullable for partial updates.
/// Only allowed when Status == Scheduled.
/// </summary>
public class UpdateOnlineLessonDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string MeetingLink { get; set; }
    public string MeetingId { get; set; }
    public string MeetingPassword { get; set; }
}
