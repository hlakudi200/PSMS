# Learning Material — Metadata Backend TODO

**Filed**: 2026-09-21
**Related**: teacher-side Learning Material upload ticket ("Grade, term, core/supplementary, tags, size validation")

## Summary

The upload modal (`frontend/src/components/modals/learning/MaterialUploadModal.tsx`) now captures six fields the backend has nowhere to put. Of those, **file-size validation was already fully implemented on both sides before this ticket** (`LearningMaterialAppService.ValidateFileMetadata`, `DocumentSizeCapBytes` = 50MB, `VideoSizeCapBytes` = 500MB — added in PR #86) — no backend work needed there, it's listed below only so it isn't re-raised as a gap.

Everything else needs backend work before the corresponding frontend field does anything beyond client-side validation. The frontend intentionally does **not** send these five to the server yet (`IUploadLearningMaterial`/`UploadLearningMaterialDto` don't declare them) — the modal shows a visible "not saved yet" notice so nobody mistakes a filled-in field for a persisted one.

**Also found while wiring this up, unrelated to any of the above**: `TermAppService` has no `GetAllAsync` method at all — the frontend's `terms` provider (`frontend/src/providers/academic/terms/index.tsx`) calls `GET /api/services/app/Term/GetAll`, which 404s, and always has (this isn't a regression from this ticket). The modal routes around it by using `AcademicYear/GetCurrent`, which already returns the current year's full term list inline, so the Term field works correctly despite the underlying provider bug. Worth either adding a real `GetAllAsync` to `TermAppService` or removing the dead action from the frontend provider — filed here rather than fixed, since it's a pre-existing defect outside this ticket's scope.

## Gaps

### GAP-LM-01: Term should be required, not optional
- **Current**: `LearningMaterial.TermId` is `Guid?` (nullable); `UploadLearningMaterialDto.TermId` / `CreateLearningMaterialDto.TermId` are both `Guid?` with no `[Required]`. `LearningMaterialAppService.UploadAsync` only validates the term *exists* when one is supplied — it never requires one.
- **Needed**:
  - `[Required]` on both DTOs' `TermId`.
  - Entity property `Guid? TermId` → `Guid TermId`.
  - EF Core migration making the `LearningMaterials.TermId` column `NOT NULL`.
  - **Open question, not decided here**: what to do with any existing rows that have a null `TermId` — backfill to a default/inferred term, or leave them and only enforce going forward (migration would then need a `WHERE` guard or a data-fix step first). Whoever picks this up should check current row counts before choosing.
- **Frontend status**: already enforced client-side (`MaterialUploadModal.tsx`'s Zod schema requires `termId`, and a Term `Select` was added — note the modal previously had *no* Term input control at all despite the schema field existing, so uploads never actually carried a term before this ticket).

### GAP-LM-02: Grade categorisation
- **Current**: no `GradeId` (or equivalent) anywhere on `LearningMaterial`, `UploadLearningMaterialDto`, or `CreateLearningMaterialDto`. The only grade signal today is indirect: `ClassSubject → Class.GradeId`.
- **Needed**: new `GradeId` (`Guid`, required) on the entity + both DTOs + migration, **or** — worth deciding rather than defaulting to a new column — derive it server-side from `ClassSubject.Class.GradeId` instead of storing a second, potentially-inconsistent copy. If derived, the frontend's Grade `Select` becomes informational/confirmatory only rather than a value that needs to round-trip.
- **Frontend status**: a required `Select` (`gradeId`) was added, sourced from the existing Grades list. Not sent to the server yet.

### GAP-LM-03: Core / Supplementary flag
- **Current**: no `IsCore`, `MaterialCategory`, or equivalent field anywhere.
- **Needed**: new field on entity + both DTOs + migration — a `bool IsCore` is simplest and matches the existing `GradeSubject.IsCore` naming convention elsewhere in the Academic module; a small enum is the alternative if more than two states end up being needed later (unlikely given the ticket only asks for two).
- **Frontend status**: a required two-option `Radio.Group` (`materialCategory: 'Core' | 'Supplementary'`) was added. Not sent to the server yet.

### GAP-LM-04: Up to 5 tags
- **Current**: no `Tags` field, string, JSON column, or join table on `LearningMaterial`.
- **Needed**: a storage decision — a simple delimited/JSON string column on the entity (simplest, consistent with how small free-form lists are usually done in this codebase) vs. a proper `LearningMaterialTag` join table (more queryable — e.g. "find all materials tagged X" — but more moving parts). Either way: max 5 tags, max 30 characters per tag (bounds carried over from the frontend Zod schema; no existing product spec dictated the per-tag length, so this is our own default — safe to change on the backend side as long as the frontend schema is updated to match).
- **Frontend status**: a `Select mode="tags"` capped at 5 was added, each tag capped at 30 chars via Zod. Not sent to the server yet.

### GAP-LM-05: Scheduled future release date
- **Current**: `LearningMaterial.PublishedDate` exists but is only ever set to `DateTime.UtcNow` inside `Publish()` — there's no forward-dated field, and `GetAllAsync`/`GetByClassSubjectAsync`'s only visibility gate is the `IsPublished` boolean (no time component).
- **Needed**:
  - New `ScheduledPublishDate` (`DateTime?`) on the entity + DTOs.
  - A visibility-gating change wherever student-facing queries filter on `IsPublished`, e.g. `IsPublished && (ScheduledPublishDate == null || ScheduledPublishDate <= DateTime.UtcNow)`.
  - **Recommendation** (not a decision — flag for whoever implements): gate at query time rather than adding a background job to flip `IsPublished` at the scheduled moment. Query-time gating needs no scheduler/job infrastructure and can't drift if a job run is missed; the tradeoff is that `IsPublished` alone no longer tells the whole story, so anywhere else in the codebase that reads `IsPublished` directly (not through the gated query) needs auditing.
  - `Announcement` (`backend/src/psms.Core/Domain/Communication/Entities/Announcement.cs`) already has a working `PublishDate`-as-scheduling-field pattern (`AnnouncementAppService.cs`) worth reusing/mirroring.
- **Frontend status**: an optional `DatePicker` (future dates only, `showTime`) was added. Not sent to the server yet.

### GAP-LM-06: Notify students when published
- **Current**: no dispatch mechanism tied to Learning Materials at all. `Announcement` has `SendEmailNotification`/`SendPushNotification` flags that get *persisted*, but no dispatch call was found wired to them either — so this isn't a "copy a working pattern," it's new both here and (apparently) for Announcements.
- **Needed**: a call from wherever `PublishAsync` actually happens into `NotificationAppService.CreateAsync` (`backend/src/psms.Application/Communication/Notifications/NotificationAppService.cs`) — or a proper domain-event handler if fan-out to every enrolled student needs to happen asynchronously/in bulk rather than inline. Either way, needs: resolving the enrolled-student list for the material's `ClassSubject`, and a decision on notification content/template.
- **Frontend status**: an optional `Checkbox` (`notifyStudents`) was added. Not sent to the server yet — there's nothing for the server to act on regardless of whether it's sent, since no dispatch path exists.

## Not a gap — confirmed already correct

### File size validation (50MB documents / 500MB video)
- Client: `MaterialUploadModal.tsx`'s `handleBeforeUpload`, gated by `VIDEO_MAX_BYTES` / `DOCUMENT_MAX_BYTES`, rejects an oversized file at selection time — before `requestUploadUrlAsync` is ever called, i.e. before the transfer starts.
- Server: `LearningMaterialAppService.ValidateFileMetadata` (`DocumentSizeCapBytes` = 50MB, `VideoSizeCapBytes` = 500MB) re-validates the real size read back from storage after the direct-to-storage upload completes, throwing `LearningExceptionCodes.LearningMaterialTooLarge` if exceeded — the source of truth, since the client check can be bypassed by a non-UI caller.
- No changes made or needed.
