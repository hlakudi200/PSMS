---
name: abp-realtime
description: Real-time & live-media patterns for an ABP + Next.js app — ABP's built-in SignalR hub (AbpCommonHub) with encrypted-token (enc_auth_token) auth, LiveKit live video (server token minting, room/egress, signed webhooks, the LiveKitRoom client + data-channel signaling), and the polling fallback used for near-live notifications. Use when adding live updates, live audio/video, webhooks, or deciding push-vs-poll.
---

# Real-Time & Live Media (ABP + Next.js)

Three distinct mechanisms, each fit to a different job. Pick deliberately:

| Need | Mechanism | Status in this codebase |
|------|-----------|--------------------------|
| Server→client event push (notifications, presence) | **ABP SignalR** (`AbpCommonHub`) | Infra wired, **dormant** — notifications use polling instead |
| Live audio/video/screen-share + recording | **LiveKit** (SFU) | **Fully implemented** (live classes) |
| "Near-live" counters/badges without push | **HTTP polling** | In use (notification bell, 45s) |

> Honest default: for a simple "keep a badge fresh" need, **polling is what this project ships** and it's reliable. Reach for SignalR only when you need true push at scale; reach for LiveKit only for media.

---

## A. ABP SignalR — built-in hub + token auth

ABP ships `AbpCommonHub`; you usually don't write a hub class for basic broadcast. It's mapped at `/signalr` and the SignalR module is a dependency.

**Wiring (already present):**
```csharp
// <App>.Web.Core module — DependsOn(... typeof(AbpAspNetCoreSignalRModule))
// Startup.cs
app.UseEndpoints(endpoints => { endpoints.MapHub<AbpCommonHub>("/signalr"); });
```

**The critical piece — JWT over WebSocket via encrypted query token.** Browsers can't set an `Authorization` header on a WebSocket, so ABP passes the **encrypted** token as `?enc_auth_token=...` and decrypts it server-side. This is configured in `AuthConfigurer`:

```csharp
options.Events = new JwtBearerEvents { OnMessageReceived = QueryStringTokenResolver };

private static Task QueryStringTokenResolver(MessageReceivedContext context)
{
    var path = context.HttpContext.Request.Path;
    if (!path.HasValue || !path.Value.StartsWith("/signalr")) return Task.CompletedTask;   // only hub paths
    var qsToken = context.HttpContext.Request.Query["enc_auth_token"].FirstOrDefault();
    if (qsToken == null) return Task.CompletedTask;
    context.Token = SimpleStringCipher.Instance.Decrypt(qsToken);   // decrypt → standard JWT validation runs
    return Task.CompletedTask;
}
```

The **encrypted** token is the `EncryptedAccessToken` field returned by `TokenAuth/Authenticate` (keep it at login — see **abp-react-auth**). To go live you'd: build the `@microsoft/signalr` connection on the client with `accessTokenFactory` / the `enc_auth_token` query param, register handlers, and on the server push via `IOnlineClientManager` + the hub. **No frontend SignalR client exists yet** — adding one is the work, the auth plumbing is done.

**Custom hub** (only if you need typed methods): a class `: Hub` (or ABP's `OnlineClientHubBase`), mapped in `Startup`, authorized the same way via the resolver.

---

## B. LiveKit — live video/audio/screen-share + recording

The actual real-time-media implementation (live classes). LiveKit is an external SFU; the backend **mints access tokens** and **manages rooms/egress**, the frontend renders with `@livekit/components-react`.

### Backend token service (`Infrastructure/LiveStreaming/LiveKitTokenService.cs`)
- **`CreateJoinToken(req)`** — mints an HS256 JWT with a `video` grant scoped to the room: `roomJoin`, `canPublish` (true for host/teacher, false for viewers), `canSubscribe`, `canPublishData` (for signaling). Returns `{ serverUrl, token, roomName }`.
- **`EnsureRecordingRoomAsync(room)`** — calls LiveKit RoomService `CreateRoom` with auto-egress to S3 (recording). Call this *before* the session opens to participants.
- **`CloseRoomAsync(room)`** — deletes the room to finalize egress.
- **`VerifyWebhook(body, authHeader)`** — validates the HS256 signature (hash over the raw body). **Always verify before trusting a webhook.**

### Access control lives in the AppService, not the token endpoint
`GetJoinTokenAsync(lessonId)` is where you enforce *who may join as what*: validate the user is signed in, the session is live, the viewer is enrolled/authorized, then set `canPublish = isHost`. The token only carries what the AppService decided.

```csharp
var ticket = _liveKit.CreateJoinToken(new LiveKitJoinRequest {
    Identity = identity, Name = displayName, RoomName = roomName,
    CanPublish = isHost, CanPublishData = true,
});
return new LiveClassJoinDto { ServerUrl = ticket.ServerUrl, Token = ticket.Token,
                              RoomName = roomName, Identity = identity, CanPublish = isHost };
```

### Webhook controller (`/api/livekit/webhook`)
Plain controller (not an AppService — external caller, no ABP session). Verify the signature, then react to events:
- `egress_ended` → attach the recording artifact to the entity,
- `participant_joined` / `participant_left` → track attendance + live head-count.

### Frontend (`@livekit/components-react`, `livekit-client`)
Fetch the token via the provider, then mount `<LiveKitRoom>`. Host gets the full `VideoConference` UI (publish); viewers are subscribe-only and render selected tracks.

```tsx
const join = await getJoinTokenAsync(lessonId);   // { serverUrl, token, roomName, identity, canPublish }
return (
  <LiveKitRoom serverUrl={join.serverUrl} token={join.token}
               video={join.canPublish} audio={join.canPublish} connect>
    {join.canPublish ? <VideoConference /> : <ViewerTracks /> }
  </LiveKitRoom>
);
```

**Data-channel signaling** (raise-hand, custom events) rides LiveKit's data channel via `useDataChannel(TOPIC)` — no extra server needed: publishers send `{type, identity, name}`, subscribers reduce them into UI state. Use this for lightweight in-room signaling instead of SignalR.

### Config / secrets
LiveKit API key/secret, server URL, and the webhook URL come from env (`LiveKit__*` double-underscore convention) — never hard-code. Production needs those vars set plus the webhook URL registered with LiveKit.

---

## C. Polling — the pragmatic near-live fallback

For "keep this counter fresh" the project polls on an interval **and on window focus** — simple, robust, no socket lifecycle to manage:

```tsx
const POLL_INTERVAL_MS = 45000;                 // notification bell cadence
useEffect(() => {
  const tick = () => getUnreadCountRef.current();
  tick();                                        // immediately
  const timer = setInterval(tick, POLL_INTERVAL_MS);
  window.addEventListener('focus', tick);        // extra refresh when tab refocuses
  return () => { clearInterval(timer); window.removeEventListener('focus', tick); };
}, []);
```

This is the deliberate stand-in until SignalR push is wired. Good enough for badges/counts; not for chat or sub-second updates.

---

## Choosing
- **Counts/badges, low urgency** → polling (interval + focus). Ship this first.
- **True push (chat, presence, instant notifications) at scale** → ABP SignalR (`AbpCommonHub` or a custom hub) with the `enc_auth_token` resolver; build the `@microsoft/signalr` client.
- **Audio/video/screen-share, recording** → LiveKit; enforce access in the AppService, verify webhook signatures, keep keys in env.

## Gotchas
- **WebSocket auth ≠ header auth** — SignalR must pass the **encrypted** token via `enc_auth_token` query param; the resolver only fires for `/signalr` paths.
- **Mint LiveKit tokens server-side only** — never expose the API secret; bake `canPublish`/room scope into the token from an authorized AppService.
- **Create the LiveKit room (with egress) before participants join**, and close it to finalize the recording.
- **Always verify webhook signatures** before acting; webhook controllers run without an ABP session, so don't assume `AbpSession`.
- **Don't over-reach for SignalR** — if polling meets the need, the socket lifecycle (reconnect, scale-out backplane) is cost you don't need yet.
- **Secrets via env** (`LiveKit__*`), never committed.
