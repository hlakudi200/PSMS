using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using psms.Controllers;
using psms.Domain.Learning.Entities;
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

    private readonly ILiveKitTokenService _liveKit;
    private readonly IRepository<OnlineLesson, Guid> _lessonRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public LiveKitWebhookController(
        ILiveKitTokenService liveKit,
        IRepository<OnlineLesson, Guid> lessonRepository,
        IFileStorageService fileStorage,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _liveKit = liveKit;
        _lessonRepository = lessonRepository;
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
            int num;
            bool hasNum;
            using (var doc = JsonDocument.Parse(body))
            {
                var root = doc.RootElement;
                evt = root.TryGetProperty("event", out var e) ? e.GetString() : null;
                lessonId = ParseLessonId(ExtractRoomName(root));
                hasNum = TryGetNumParticipants(root, out num);
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
                    if (hasNum)
                        await UpdateAttendanceAsync(lessonId.Value, num);
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

    /// <summary>Track the peak concurrent participant count as the attendee count.</summary>
    private async Task UpdateAttendanceAsync(Guid lessonId, int numParticipants)
    {
        using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
        {
            var lesson = await _lessonRepository.FirstOrDefaultAsync(lessonId);
            if (lesson == null) return;

            var peak = Math.Max(lesson.AttendeeCount ?? 0, numParticipants);
            if (peak == (lesson.AttendeeCount ?? 0)) return; // no change
            lesson.AttendeeCount = peak;
            await _unitOfWorkManager.Current.SaveChangesAsync();
        }
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

    private static bool TryGetNumParticipants(JsonElement root, out int num)
    {
        num = 0;
        return root.TryGetProperty("room", out var room)
            && room.TryGetProperty("numParticipants", out var n)
            && n.TryGetInt32(out num);
    }
}
}
