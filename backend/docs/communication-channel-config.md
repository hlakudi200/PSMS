# Communication Channel Configuration

How to turn each notification channel from a **scaffold no-op** into a **live** channel.
Every external channel ships with a `NotConfigured*Gateway` that logs and returns
"not configured" — so nothing is sent until you (1) implement a real gateway and
(2) replace its single DI registration in `psmsApplicationModule.cs`.

> **Status:** all gateways below are currently the not-configured default. In-app is live.

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

## Channel: Email  — gateway: `IEmailGateway`  (COMM-09)
- **Providers:** SendGrid, Amazon SES, Mailgun.
- **Config keys (fill in):**
  | Key | Example / notes |
  | --- | --- |
  | `Communication:Email:Provider` | `SendGrid` \| `SES` \| `Mailgun` |
  | `Communication:Email:ApiKey` | provider API key (**secret**) |
  | `Communication:Email:FromAddress` | `noreply@yourschool.co.za` |
  | `Communication:Email:FromName` | display name |
  | `Communication:Email:ApiBaseUrl` | provider endpoint (or SMTP host) |
  | `Communication:Email:BounceWebhookSecret` | bounce/complaint callbacks (**secret**) |
- **Deliverability:** configure **SPF / DKIM / DMARC** on the sending domain.
- **Notes:** body is HTML — template variable values are HTML-encoded at render time,
  and the template-less literal fallback is HTML-encoded + line-broken too. Validate/
  trim the recipient address in the gateway. Handle bounces/complaints → suppress +
  update the delivery log.

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
