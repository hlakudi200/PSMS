using System.Collections.Generic;

namespace psms.Communication.Messages.Dto;

/// <summary>
/// #204. Someone a parent may start a conversation with: a teacher of one of
/// their children.
/// </summary>
public class MessageRecipientDto
{
    public long UserId { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// Why this teacher is on the list, one line per role: "Class teacher, 3D
    /// (Ayanda)" or "Mathematics, 3D (Ayanda)".
    /// </summary>
    public List<string> Roles { get; set; } = new();
}
