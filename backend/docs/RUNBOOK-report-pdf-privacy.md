# Runbook: making report card PDFs private

Report card PDFs live in the Supabase `reports` bucket. That bucket is
**public**, which means every object in it is served by

```
{ProjectUrl}/storage/v1/object/public/reports/{key}
```

to anyone who has the link, with no authentication and no expiry — regardless
of the ACL set on the individual object.

## Why this matters

The storage key contains the learner's admission number:

```
reports/{tenantId}/{academicYearId}/{termId}/{admissionNumber}_{reportId}.pdf
```

and the file is a full South African report card: subject marks, achievement
levels, class position, attendance, and the teacher's and principal's comments
about a named child.

Verified against the live database on 22 September 2026: **9 stored report URLs,
all returning `HTTP 200 application/pdf` to an unauthenticated request.**

## Code side (done in #273)

- Uploads no longer set `S3CannedACL.PublicRead`.
- `Report.PdfObjectKey` stores the storage key instead of a URL.
- `GetReportPdfUrlAsync` mints a **5-minute signed URL** per request, gated on
  `Assessment.ReportCards.Download` and scoped to the caller's own children.
- `PdfUrl` is no longer exposed on any DTO, so the application never hands out
  a durable link.

**This is necessary but not sufficient.** While the bucket is public the object
is reachable by its public URL whatever the application does.

## Infrastructure side (must be done by someone with the service key)

Signed URLs work on a public bucket as well as a private one, so the safe order
is:

1. **Merge and deploy #273.** After this the app reads through signed URLs and
   no longer depends on the public path.
2. **Make the bucket private.** Supabase dashboard → Storage → `reports` →
   Settings → turn off *Public bucket*. Or via the API:

   ```bash
   curl -X PUT "$PROJECT_URL/storage/v1/bucket/reports" \
     -H "Authorization: Bearer $SERVICE_KEY" \
     -H "Content-Type: application/json" \
     -d '{"id":"reports","name":"reports","public":false}'
   ```

3. **Verify.** Every stored URL should stop returning 200:

   ```bash
   curl -s -o /dev/null -w "%{http_code}\n" \
     "$PROJECT_URL/storage/v1/object/public/reports/<any known key>"
   ```

   and a signed URL for the same object should still return the PDF.

Doing step 2 before step 1 breaks report downloads in the deployed app, because
the released build still serves the stored public URLs.

## Related buckets

`materials` is also served through `GetPublicUrl` — learning materials are
handed out as public links too. That may be deliberate, since materials are
meant for students, but it is the same mechanism and is worth a deliberate
decision rather than an inherited default. Lesson recordings and admissions
documents already use signed URLs.

## Note on the service key

`SupabaseStorage:ServiceKey` is empty in the committed configuration, which is
correct — it is a secret and belongs in the environment, not in the repository.
Step 2 needs it.
