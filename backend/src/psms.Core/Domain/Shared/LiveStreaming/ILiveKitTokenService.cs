using System;
using System.Threading.Tasks;

namespace psms.Domain.Shared.LiveStreaming;

/// <summary>
/// Mints LiveKit access tokens for the in-app live classroom (LC-01). A token
/// is a signed JWT carrying a room-scoped "video" grant; the client uses it to
/// connect to the LiveKit SFU. The server never proxies media — it only issues
/// these tokens with the right per-role capabilities.
/// </summary>
public interface ILiveKitTokenService
{
    /// <summary>The wss:// LiveKit server URL clients connect to (empty if unconfigured).</summary>
    string ServerUrl { get; }

    /// <summary>True when Url + ApiKey + ApiSecret are all configured.</summary>
    bool IsConfigured { get; }

    /// <summary>Creates a signed join token for a participant.</summary>
    LiveKitJoinTicket CreateJoinToken(LiveKitJoinRequest request);

    /// <summary>
    /// True when LiveKit AND an S3 recording destination are both configured
    /// (LC-03) — i.e. live classes can be recorded.
    /// </summary>
    bool IsRecordingConfigured { get; }

    /// <summary>
    /// Ensures the room exists with auto-egress recording configured to write
    /// the composited MP4 to <paramref name="recordingObjectKey"/> in the
    /// recordings bucket. Call this before the host joins (egress must be set
    /// at room creation — it can't be added to an already-active room).
    /// No-op when recording isn't configured. Best-effort: implementations
    /// should swallow/translate transport errors so a recording hiccup never
    /// blocks the class from starting.
    /// </summary>
    Task EnsureRecordingRoomAsync(string roomName, string recordingObjectKey);

    /// <summary>
    /// Closes the LiveKit room (disconnects everyone) so any active auto-egress
    /// finalizes and uploads promptly — call when the class ends. No-op when
    /// LiveKit isn't configured. Best-effort.
    /// </summary>
    Task CloseRoomAsync(string roomName);
}

/// <summary>Inputs for a single participant's join token.</summary>
public class LiveKitJoinRequest
{
    /// <summary>Stable, unique participant identity (e.g. "user-42").</summary>
    public string Identity { get; set; }

    /// <summary>Display name shown on the participant tile.</summary>
    public string Name { get; set; }

    /// <summary>The room to join (derived from the lesson id).</summary>
    public string RoomName { get; set; }

    /// <summary>
    /// Whether this participant may publish audio/video/screen. True for the
    /// hosting teacher; false for students in the broadcast model.
    /// </summary>
    public bool CanPublish { get; set; }

    /// <summary>
    /// Whether this participant may send data messages (chat / raise-hand).
    /// True for everyone in v1.
    /// </summary>
    public bool CanPublishData { get; set; } = true;

    /// <summary>Token lifetime; defaults to 6 hours when null.</summary>
    public TimeSpan? Ttl { get; set; }
}

/// <summary>What the client needs to connect to the room.</summary>
public class LiveKitJoinTicket
{
    public string ServerUrl { get; set; }
    public string Token { get; set; }
    public string RoomName { get; set; }
    public string Identity { get; set; }
    public bool CanPublish { get; set; }
}
