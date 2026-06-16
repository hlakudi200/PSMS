using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using psms.Controllers;
using psms.Domain.Learning.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Shared.Storage;
using psms.Domain.Shared.LiveStreaming;

namespace psms.Web.Host.Controllers
{
/// <summary>
/// Receives LiveKit Cloud webhooks (LC-04). Anonymous — authenticated instead
/// by the LiveKit-signed JWT in the Authorization header (verified against our
/// API secret + a body-hash claim). Runs outside any tenant/user context, so
/// lesson lookups disable the tenant filter and resolve by the room name
/// (class-{lessonId}). Always acks 2xx for handled/ignored events so LiveKit
/// doesn't retry; only a bad signature is rejected.
/// </summary>
[AllowAnonymous]
[Route("api/livekit")]
public class LiveKitWebhookController : psmsControllerBase
{
    private const string RecordingsBucket = "recordings";

    // Mirror of the EndAsync cap in OnlineLessonAppService: an attendee count
    // beyond this is treated as poisoned reporting data, so the webhook clamps
    // its live head-count to the same ceiling.
    private const int MaxWebhookAttendeeCount = 1000;

    private readonly ILiveKitTokenService _liveKit;
    private readonly IRepository<OnlineLesson, Guid> _lessonRepository;
    private readonly IRepository<LiveClassAttendance, Guid> _attendanceRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public LiveKitWebhookController(
        ILiveKitTokenService liveKit,
        IRepository<OnlineLesson, Guid> lessonRepository,
        IRepository<LiveClassAttendance, Guid> attendanceRepository,
        IFileStorageService fileStorage,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _liveKit = liveKit;
        _lessonRepository = lessonRepository;
        _attendanceRepository = attendanceRepository;
        _fileStorage = fileStorage;
        _unitOfWorkManager = unitOfWorkManager;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook()
    {
        // Read the RAW body — the signature covers the exact bytes.
        byte[] body;
        using (var ms = new MemoryStream())
        {
            await Request.Body.CopyToAsync(ms);
            body = ms.ToArray();
        }

        var auth = Request.Headers["Authorization"].ToString();
        if (!_liveKit.VerifyWebhook(auth, body))
            return Unauthorized();

        try
        {
            string evt;
            Guid? lessonId;
            string participantIdentity = null;
            string participantName = null;
            using (var doc = JsonDocument.Parse(body))
            {
                var root = doc.RootElement;
                evt = root.TryGetProperty("event", out var e) ? e.GetString() : null;
                lessonId = ParseLessonId(ExtractRoomName(root));
                if (root.TryGetProperty("participant", out var p))
                {
                    if (p.TryGetProperty("identity", out var pi) && pi.ValueKind == JsonValueKind.String)
                        participantIdentity = pi.GetString();
                    if (p.TryGetProperty("name", out var pn) && pn.ValueKind == JsonValueKind.String)
                        participantName = pn.GetString();
                }
            }

            if (lessonId == null) return Ok(); // not one of our in-app classes

            switch (evt)
            {
                case "egress_ended":
                    // Only on egress_ended — egress_updated fires on intermediate
                    // states where the MP4 may be partially flushed (non-zero size
                    // but unplayable), which would then block the real ended event
                    // via the HasRecording early-return.
                    await AttachRecordingAsync(lessonId.Value);
                    break;
                case "participant_joined":
                case "participant_left":
                    if (!string.IsNullOrEmpty(participantIdentity))
                        await RecordAttendanceAsync(lessonId.Value, evt, participantIdentity, participantName);
                    break;
            }
        }
        catch (Exception ex)
        {
            // Never fail the webhook on a processing error — LiveKit would retry
            // indefinitely. Log and ack.
            Logger.Warn("LiveKit webhook processing error: " + ex.Message);
        }

        return Ok();
    }

    /// <summary>
    /// Egress finished — if the recording actually landed in storage, attach
    /// its object key to the lesson (private bucket — LC-06). HEAD-confirming
    /// avoids marking a lesson as recorded when egress produced nothing.
    /// </summary>
    private async Task AttachRecordingAsync(Guid lessonId)
    {
        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
        {
            var lesson = await _lessonRepository.FirstOrDefaultAsync(lessonId);
            if (lesson == null || lesson.HasRecording) return;

            var key = $"{lesson.TenantId ?? 0}/{lessonId}/recording.mp4";
            var info = await _fileStorage.GetObjectInfoAsync(RecordingsBucket, key);
            if (info == null || info.SizeBytes <= 0) return; // nothing recorded

            // LC-06: store the object key (private bucket); playback is via a
            // signed URL from GetRecordingDownloadUrl, not a public link.
            lesson.AddRecording(key);
            await _unitOfWorkManager.Current.SaveChangesAsync();
        }
    }

    /// <summary>
    /// LC-05: record DISTINCT attendance. One row per (lesson, participant);
    /// participant_joined creates/refreshes the row, participant_left stamps the
    /// leave time. While the lesson is live, the lesson's AttendeeCount mirrors
    /// the distinct attendee count (a true roll-call, not peak-concurrent).
    /// </summary>
    private async Task RecordAttendanceAsync(Guid lessonId, string evt, string identity, string name)
    {
        var isLeave = evt == "participant_left";

        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
        {
            var lesson = await _lessonRepository.FirstOrDefaultAsync(lessonId);
            if (lesson == null) return;

            var now = DateTime.UtcNow;
            var row = await _attendanceRepository.FirstOrDefaultAsync(
                a => a.OnlineLessonId == lessonId && a.ParticipantIdentity == identity);

            if (row == null)
            {
                // A leave with no prior join means we never recorded this
                // participant joining (dropped/late join webhook, or a leave
                // burst from the room closing after the lesson ended). Don't
                // fabricate a zero-duration attendee — it would inflate the
                // distinct head-count this feature exists to get right.
                if (isLeave)
                {
                    await UpdateLiveAttendeeCountAsync(lesson, lessonId);
                    await _unitOfWorkManager.Current.SaveChangesAsync();
                    return;
                }

                // Insert the first-join row in an ISOLATED unit of work. A
                // concurrent first-join for the SAME participant loses the
                // unique-index (OnlineLessonId, ParticipantIdentity) race; doing
                // it here means that failure stays in the nested DbContext and
                // never poisons ours. We then fall through to update the winning
                // row, so concurrent joins converge instead of 500-ing.
                try
                {
                    using (var uow = _unitOfWorkManager.Begin(new UnitOfWorkOptions
                    {
                        Scope = TransactionScopeOption.RequiresNew,
                    }))
                    {
                        // Begin() makes the nested UoW the ambient Current one.
                        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
                        {
                            await _attendanceRepository.InsertAsync(new LiveClassAttendance(
                                Guid.NewGuid(), lesson.TenantId, lessonId, identity, name, now));
                            await uow.CompleteAsync();
                        }
                    }
                }
                catch (DbUpdateException)
                {
                    // Lost the race — the winning request created the row.
                }

                row = await _attendanceRepository.FirstOrDefaultAsync(
                    a => a.OnlineLessonId == lessonId && a.ParticipantIdentity == identity);
            }

            if (row != null)
            {
                if (!string.IsNullOrEmpty(name)) row.DisplayName = name;
                row.LastLeftAt = isLeave ? now : (DateTime?)null; // a join clears a stale leave
            }

            await UpdateLiveAttendeeCountAsync(lesson, lessonId);
            await _unitOfWorkManager.Current.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Mirror the live distinct head-count onto the lesson — but ONLY while it
    /// is in progress. Once the teacher ends the lesson they confirm an
    /// authoritative figure (EndAsync); trailing participant_left webhooks from
    /// the room closing must not silently overwrite it. Clamped to the same
    /// ceiling EndAsync enforces.
    /// </summary>
    private async Task UpdateLiveAttendeeCountAsync(OnlineLesson lesson, Guid lessonId)
    {
        if (lesson.Status != OnlineLessonStatus.InProgress) return;
        var distinct = await _attendanceRepository
            .GetAll().CountAsync(a => a.OnlineLessonId == lessonId);
        lesson.AttendeeCount = Math.Min(distinct, MaxWebhookAttendeeCount);
    }

    private static string ExtractRoomName(JsonElement root)
    {
        if (root.TryGetProperty("room", out var room)
            && room.TryGetProperty("name", out var rn) && rn.ValueKind == JsonValueKind.String)
            return rn.GetString();
        if (root.TryGetProperty("egressInfo", out var eg)
            && eg.TryGetProperty("roomName", out var ern) && ern.ValueKind == JsonValueKind.String)
            return ern.GetString();
        return null;
    }

    /// <summary>Room names are "class-{lessonGuid}" (see OnlineLessonAppService).</summary>
    private static Guid? ParseLessonId(string roomName)
    {
        const string prefix = "class-";
        if (string.IsNullOrEmpty(roomName) || !roomName.StartsWith(prefix, StringComparison.Ordinal))
            return null;
        return Guid.TryParse(roomName.Substring(prefix.Length), out var id) ? id : (Guid?)null;
    }

}
}
