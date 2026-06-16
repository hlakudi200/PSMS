using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
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
}
