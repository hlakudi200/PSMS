using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using psms.Domain.Shared.LiveStreaming;

namespace psms.Infrastructure.LiveStreaming;

/// <summary>
/// LiveKit token service. A LiveKit access token is just an HS256 JWT signed
/// with the API secret, whose payload carries a "video" grant object. We mint
/// it by hand (System.Text.Json + HMACSHA256) rather than pulling in a
/// third-party SDK — this mirrors the raw-HTTP approach in SupabaseStorageService
/// and gives exact control over the grant shape LiveKit expects.
/// </summary>
public class LiveKitTokenService : ILiveKitTokenService
{
    private readonly IConfiguration _configuration;

    // Recordings land in the same public bucket as uploaded recordings (SF-02).
    private const string RecordingsBucket = "recordings";

    // Shared client — room/egress calls are infrequent but a per-call
    // HttpClient risks socket exhaustion (same rationale as SupabaseStorageService).
    private static readonly HttpClient Http = new HttpClient();

    public ILogger Logger { get; set; } = NullLogger.Instance;

    public LiveKitTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private (string Url, string ApiKey, string ApiSecret) GetConfig()
    {
        var section = _configuration.GetSection("LiveKit");
        return (section["Url"], section["ApiKey"], section["ApiSecret"]);
    }

    public string ServerUrl => GetConfig().Url ?? string.Empty;

    public bool IsConfigured
    {
        get
        {
            var (url, apiKey, apiSecret) = GetConfig();
            return !string.IsNullOrEmpty(url)
                && !string.IsNullOrEmpty(apiKey)
                && !string.IsNullOrEmpty(apiSecret);
        }
    }

    public LiveKitJoinTicket CreateJoinToken(LiveKitJoinRequest request)
    {
        var (url, apiKey, apiSecret) = GetConfig();
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            throw new InvalidOperationException(
                "LiveKit is not configured — set LiveKit__Url, LiveKit__ApiKey and LiveKit__ApiSecret.");

        if (string.IsNullOrWhiteSpace(request.Identity))
            throw new ArgumentException("Identity is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.RoomName))
            throw new ArgumentException("RoomName is required.", nameof(request));

        var now = DateTimeOffset.UtcNow;
        var ttl = request.Ttl ?? TimeSpan.FromHours(6);

        // LiveKit "video" grant — the room-scoped capabilities for this token.
        var grant = new Dictionary<string, object>
        {
            ["room"] = request.RoomName,
            ["roomJoin"] = true,
            ["canPublish"] = request.CanPublish,
            ["canSubscribe"] = true,
            ["canPublishData"] = request.CanPublishData,
        };

        var payload = new Dictionary<string, object>
        {
            ["iss"] = apiKey,            // LiveKit identifies the key via the issuer
            ["sub"] = request.Identity,
            ["nbf"] = now.ToUnixTimeSeconds(),
            ["exp"] = now.Add(ttl).ToUnixTimeSeconds(),
            ["name"] = string.IsNullOrWhiteSpace(request.Name) ? request.Identity : request.Name,
            ["video"] = grant,
        };

        var token = EncodeHs256(payload, apiSecret);

        return new LiveKitJoinTicket
        {
            ServerUrl = url,
            Token = token,
            RoomName = request.RoomName,
            Identity = request.Identity,
            CanPublish = request.CanPublish,
        };
    }

    // Relaxed escaping so a display name containing &, <, > etc. is emitted
    // literally rather than as \u00XX — valid JSON either way, but avoids
    // surprising-looking names in LiveKit tooling once real names are used.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static string EncodeHs256(Dictionary<string, object> payload, string secret)
    {
        var header = new Dictionary<string, object> { ["alg"] = "HS256", ["typ"] = "JWT" };
        var headerSegment = Base64Url(JsonSerializer.SerializeToUtf8Bytes(header, JsonOptions));
        var payloadSegment = Base64Url(JsonSerializer.SerializeToUtf8Bytes(payload, JsonOptions));
        var signingInput = $"{headerSegment}.{payloadSegment}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signature = hmac.ComputeHash(Encoding.ASCII.GetBytes(signingInput));
        return $"{signingInput}.{Base64Url(signature)}";
    }

    private static string Base64Url(byte[] bytes)
        => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    // ── LC-03: room recording via LiveKit auto-egress ─────────────────────

    public bool IsRecordingConfigured
    {
        get
        {
            if (!IsConfigured) return false;
            var s3 = _configuration.GetSection("SupabaseStorage");
            return !string.IsNullOrEmpty(s3["Endpoint"])
                && !string.IsNullOrEmpty(s3["AccessKey"])
                && !string.IsNullOrEmpty(s3["SecretKey"]);
        }
    }

    public async Task EnsureRecordingRoomAsync(string roomName, string recordingObjectKey)
    {
        if (!IsRecordingConfigured) return; // recording disabled — silent no-op
        if (string.IsNullOrWhiteSpace(roomName) || string.IsNullOrWhiteSpace(recordingObjectKey)) return;

        var (url, apiKey, apiSecret) = GetConfig();
        var s3 = _configuration.GetSection("SupabaseStorage");

        // CreateRoom with a RoomEgress config => LiveKit auto-records the
        // composited room to S3 (our Supabase bucket) and stops when the room
        // closes. Egress can ONLY be set at creation, so callers invoke this
        // before the room is first joined (see StartAsync).
        //
        // WARNING: LiveKit's Twirp server decodes with DiscardUnknown=true, so a
        // MISSPELLED field here is silently dropped — CreateRoom still returns
        // 200 and the room appears, but with no/partial egress. If recordings
        // ever stop appearing, suspect a renamed/typo'd proto field below; the
        // egress_started webhook (LC-04) is the real confirmation.
        var body = new Dictionary<string, object>
        {
            ["name"] = roomName,
            // Keep an empty (host started but nobody joined yet) room alive long
            // enough to span a whole lesson, so the egress-configured room isn't
            // reaped and silently re-created without egress by a late first join.
            ["emptyTimeout"] = 7200,    // 2h
            ["departureTimeout"] = 60,
            ["egress"] = new Dictionary<string, object>
            {
                ["room"] = new Dictionary<string, object>
                {
                    ["fileOutputs"] = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["fileType"] = "MP4",
                            ["filepath"] = recordingObjectKey,
                            ["s3"] = new Dictionary<string, object>
                            {
                                ["accessKey"] = s3["AccessKey"],
                                ["secret"] = s3["SecretKey"],
                                ["region"] = string.IsNullOrEmpty(s3["Region"]) ? "us-east-1" : s3["Region"],
                                ["endpoint"] = s3["Endpoint"],
                                ["bucket"] = RecordingsBucket,
                                ["forcePathStyle"] = true,
                            },
                        },
                    },
                },
            },
        };

        // Everything below (including token mint + URL transform) is inside the
        // try so the best-effort contract holds even if config is malformed —
        // a recording-setup hiccup must never stop the class from starting.
        try
        {
            var adminToken = CreateAdminToken(apiKey, apiSecret, roomName);
            var httpBase = ToHttpBase(url);
            using var req = new HttpRequestMessage(
                HttpMethod.Post, $"{httpBase}/twirp/livekit.RoomService/CreateRoom");
            req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + adminToken);
            req.Content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");

            var resp = await Http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                var detail = await resp.Content.ReadAsStringAsync();
                Logger.Warn($"LiveKit CreateRoom(egress) for '{roomName}' failed: {(int)resp.StatusCode} {detail}");
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"LiveKit CreateRoom(egress) for '{roomName}' threw: {ex.Message}");
        }
    }

    public async Task CloseRoomAsync(string roomName)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(roomName)) return;
        var (url, apiKey, apiSecret) = GetConfig();
        try
        {
            var adminToken = CreateAdminToken(apiKey, apiSecret, roomName);
            var httpBase = ToHttpBase(url);
            using var req = new HttpRequestMessage(
                HttpMethod.Post, $"{httpBase}/twirp/livekit.RoomService/DeleteRoom");
            req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + adminToken);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new Dictionary<string, object> { ["room"] = roomName }, JsonOptions),
                Encoding.UTF8, "application/json");

            var resp = await Http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
            {
                var detail = await resp.Content.ReadAsStringAsync();
                Logger.Warn($"LiveKit DeleteRoom for '{roomName}' failed: {(int)resp.StatusCode} {detail}");
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"LiveKit DeleteRoom for '{roomName}' threw: {ex.Message}");
        }
    }

    /// <summary>Server token with room admin + record grants for the Twirp API.</summary>
    private string CreateAdminToken(string apiKey, string apiSecret, string roomName)
    {
        var now = DateTimeOffset.UtcNow;
        var grant = new Dictionary<string, object>
        {
            ["room"] = roomName,
            ["roomCreate"] = true,
            ["roomAdmin"] = true,
            ["roomRecord"] = true,
        };
        var payload = new Dictionary<string, object>
        {
            ["iss"] = apiKey,
            ["sub"] = apiKey,
            ["nbf"] = now.ToUnixTimeSeconds(),
            ["exp"] = now.AddMinutes(10).ToUnixTimeSeconds(),
            ["video"] = grant,
        };
        return EncodeHs256(payload, apiSecret);
    }

    private static string ToHttpBase(string wsUrl)
    {
        if (wsUrl.StartsWith("wss://", StringComparison.OrdinalIgnoreCase))
            return "https://" + wsUrl.Substring(6).TrimEnd('/');
        if (wsUrl.StartsWith("ws://", StringComparison.OrdinalIgnoreCase))
            return "http://" + wsUrl.Substring(5).TrimEnd('/');
        return wsUrl.TrimEnd('/');
    }
}
