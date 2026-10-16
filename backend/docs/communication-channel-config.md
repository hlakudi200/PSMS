# Communication Channel Configuration

How to turn each notification channel from a **scaffold no-op** into a **live** channel.
Every external channel ships with a `NotConfigured*Gateway` that logs and returns
"not configured" — so nothing is sent until you (1) implement a real gateway and
(2) replace its single DI registration in `psmsApplicationModule.cs`.

> **Status:** in-app is live, and email is wired to Resend (set the keys below to turn it on).
> WhatsApp, SMS and Push are still the not-configured default.

---

## How a channel goes live (the pattern)

1. Implement the channel's gateway interface against your provider's SDK/HTTP API
   (read config/secrets from `appsettings`/env — **never hard-code secrets**).
2. Replace the registration in `psmsApplicationModule.cs`:
   ```csharp
   // from:
   IocManager.Register<ISmsGateway, NotConfiguredSmsGateway>(DependencyLifeStyle.Transient);
   // to:
   IocManager.Register<ISmsGateway, ClickatellSmsGateway>(DependencyLifeStyle.Transient);
   ```
3. Add the channel to the dispatcher's routing (COMM-11) so it's actually selected.
4. (Where applicable) expose the inbound/delivery webhook and register its route.

Channels are gated upstream regardless of provider: **consent** (COMM-05) for external
channels, the channel **routing** (COMM-11), and per-category **preferences** (COMM-05).

---

## Channel: In-app  ✅ live
- No external provider. Real-time push (SignalR) is COMM-13 (currently polling).

## Channel: WhatsApp  — gateway: `IWhatsAppGateway`  (COMM-07 scaffold; live = COMM-14)
- **Providers:** Meta WhatsApp Cloud API (recommended), CM.com, Clickatell.
- **Config keys (fill in):**
  | Key | Example / notes |
  | --- | --- |
  | `Communication:WhatsApp:Provider` | `Meta` \| `CM` \| `Clickatell` |
  | `Communication:WhatsApp:PhoneNumberId` | Meta business sender phone-number-id |
  | `Communication:WhatsApp:AccessToken` | Meta system-user token (**secret**) |
  | `Communication:WhatsApp:ApiBaseUrl` | `https://graph.facebook.com/v20.0` |
  | `Communication:WhatsApp:WebhookVerifyToken` | your chosen verify token (**secret**) |
  | `Communication:WhatsApp:AppSecret` | for webhook signature verification (**secret**) |
- **Templates:** must be **pre-approved by Meta**; store the Meta template name in
  `NotificationTemplate.ProviderTemplateName` and the ordered body-param variable
  names in `ProviderParameterKeys` (CSV today; JSON when typed/header/button params
  are needed — see COMM-14).
- **Webhook (COMM-14):** `POST /api/whatsapp/webhook` (delivery statuses + inbound;
  `STOP` → revoke consent). `GET` for Meta verify-token challenge.
- **Notes:** normalize the recipient number to E.164 digits-only; map ISO language →
  Meta language code; validate body params non-blank before send.

## Channel: SMS  — gateway: `ISmsGateway`  (COMM-08)
- **Providers:** Clickatell (SA-strong), Twilio, Infobip.
- **Config keys (fill in):**
  | Key | Example / notes |
  | --- | --- |
  | `Communication:Sms:Provider` | `Clickatell` \| `Twilio` |
  | `Communication:Sms:ApiKey` | provider API key (**secret**) |
  | `Communication:Sms:ApiSecret` | Twilio auth token, if applicable (**secret**) |
  | `Communication:Sms:SenderId` | alphanumeric/short-code sender |
  | `Communication:Sms:ApiBaseUrl` | provider endpoint |
  | `Communication:Sms:DeliveryWebhookSecret` | for status callbacks (**secret**) |
- **Notes:** body-only, ~160 chars/segment (cost per segment); normalize number to
  E.164. Delivery receipts → update the delivery log by provider message id.

## Channel: Email  — gateway: `IEmailGateway`  (COMM-09)  ✅ wired (SMTP or Resend)

`ConfiguredEmailGateway` is the registered implementation and picks whichever provider
this deployment has been given: **SMTP wins when it is configured**, otherwise Resend.
With neither it reports **NotConfigured** — exactly as the scaffold did — so the same
build runs with and without email, and there is nothing to switch when settings arrive.

### Option A — SMTP (Gmail, Workspace, or the school's own host)

Nothing to verify and nothing to wait for: an account the school already has.

| Key | Env var (Render) | Notes |
| --- | --- | --- |
| `Smtp:Host` | `Smtp__Host` | `smtp.gmail.com` |
| `Smtp:UserName` | `Smtp__UserName` | the full address |
| `Smtp:Password` | `Smtp__Password` | **secret** — for Gmail an *app password* |
| `Smtp:Port` | `Smtp__Port` | optional, 587 |
| `Smtp:FromName` | `Smtp__FromName` | optional display name |

- **Gmail needs an app password**, not the account password — Google stopped accepting
  those from applications in 2022, and generating one requires 2-Step Verification.
- **Gmail rewrites the From** to the authenticated account, so parents see the Gmail
  address rather than `admissions@theschool.co.za`. `Smtp:From` is therefore pointless
  on Gmail and defaults to the user name.
- **Daily cap:** about 500 messages on a free account, 2000 on Workspace. Fine for
  admissions; not for a school-wide announcement.
- Port 587 with STARTTLS. Port 25 is blocked by most hosts including Render.

### Option B — Resend

Better to grow into: real delivery events, no daily cap, and a sending address on
the school's own domain. One HTTPS `POST https://api.resend.com/emails` per message.
- **Config keys:**

  | Key | Env var (Render) | Notes |
  | --- | --- | --- |
  | `Resend:ApiKey` | `Resend__ApiKey` | starts `re_` (**secret** — never commit it) |
  | `Resend:From` | `Resend__From` | `St. Peters <admissions@yourschool.co.za>` |
  | `Resend:ReplyTo` | `Resend__ReplyTo` | optional, e.g. the school office address |

- **The sending domain is the thing that bites.** Resend only delivers to arbitrary
  recipients from a domain verified in its dashboard. The out-of-the-box
  `onboarding@resend.dev` sender reaches **only the Resend account holder's own
  address** — fine for a smoke test, useless for emailing parents. Add the school's
  domain in Resend, publish the DNS records it gives you (SPF, DKIM, and a DMARC
  record of your own), then point `Resend__From` at it.
- **Rate limit:** the lower plans allow ~2 requests a second. The gateway paces its
  own sends ~550ms apart, so a fan-out to a grade's parents is slow rather than a
  wall of 429s.
- **Failure handling:** a refusal comes back as a failed `GatewaySendResult`, logged
  with the recipient masked (`zz***@example.com`) and never the key or the body. A
  bad key and an unverified domain each say so in plain words. Dispatch is
  idempotent on `(key, channel, recipient)`, so a job-level retry will not
  double-send.
- **Not done yet:** bounce and complaint webhooks — Resend can post delivery events,
  and nothing consumes them, so the delivery log never learns that a message bounced.
- **Body:** HTML. Template variable values are HTML-encoded at render time, and the
  template-less literal fallback is HTML-encoded and line-broken too.

## Channel: Push  — gateway: `IPushGateway`  (COMM-10)
- **Providers:** Firebase Cloud Messaging (Android + web), Apple APNS (iOS). FCM +
  APNS are configured together (no single `Provider` switch — both or neither).
- **Config keys (fill in):**
  | Key | Example / notes |
  | --- | --- |
  | `Communication:Push:Fcm:ServiceAccountJson` | FCM service-account JSON (**secret**) |
  | `Communication:Push:Fcm:ProjectId` | Firebase project id |
  | `Communication:Push:Apns:KeyId` | APNS key id |
  | `Communication:Push:Apns:TeamId` | Apple team id |
  | `Communication:Push:Apns:PrivateKey` | APNS .p8 key (**secret**) |
  | `Communication:Push:Apns:BundleId` | iOS bundle id |
- **Tokens:** device tokens are registered per user via `UserContactAppService`
  (COMM-04). On a provider "invalid token" response, deactivate that token.
- **Notes:** the provider fans out over a recipient's active tokens; success if any
  token is accepted.

---

## Where each gateway is registered
`backend/src/psms.Application/psmsApplicationModule.cs` — search for
`NotConfigured*Gateway` and swap in your implementation.

## Secrets
Put real credentials in environment variables / a secrets store, surfaced through
ABP `IConfiguration` (the `Communication:*` keys above). Do **not** commit secrets;
`.env` is gitignored. Mirror the existing `LiveKit__*` env-var convention for nested
keys (`Communication__Sms__ApiKey`, etc.).
