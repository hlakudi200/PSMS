# Teacher Portal — Build Plan & Tickets

**Author**: Claude Code (Opus 4.7)
**Started**: 2026-05-17
**Audience**: hlakudi200 (repo owner)
**Status**: Foundation needed — current `/teacher` route is a stub.

---

## 1. Findings (steps 1-3 of user's workflow)

### 1.1 Current state of the code

| Area | State |
|---|---|
| `frontend/src/app/teacher/` | Single `page.tsx` that delegates to `TeacherDashboard.tsx` |
| `TeacherDashboard.tsx` | Hardcoded `0` for all stats; no click handlers on the "Quick Actions" buttons; uses deprecated `bordered={false}`; uses `ProtectedRoute` directly instead of a `LayoutShell` |
| Teacher sub-routes | None — no `classes/`, `marks/`, `materials/`, `lessons/`, `timetable/`, `attendance/`, `messages/`, `assessments/` |
| Layout / navigation | None — teacher has no sidebar, no top bar, no nav menu |
| Teacher-scoped components | None — existing `TeachersPageContent`, `TeacherDetailPage`, `TeacherFormModal` are for principals/admins to manage teachers, not for a teacher's own portal |
| **Backend services** | **Already in place**: `ILearningMaterialAppService`, `IOnlineLessonAppService`, `IAssessmentAppService`, `IAssessmentQuestionAppService`, `IMarkAppService`, `IAttendanceAppService`, `IMessageAppService`, `ITimetableAppService`, `ITimetableSlotAppService`, `ITeacherClassAppService`, `ITeacherSubjectAppService` |
| **Frontend providers** | **Already in place**: `providers/learning/{learning_materials, online_lessons}`, `providers/assessment/{assessments, assessment_questions, marks}`, `providers/academic/{attendances, timetables, timetable_slots, teacher_classes, teacher_subjects}`, `providers/communication/messages` |

**Implication**: this is a pure frontend build. No backend work needed. Each ticket wires existing providers into a teacher-facing page filtered to the current user's own classes/subjects.

### 1.2 Business rules relevant to teacher workflows

From `PSMS-Business-Rules-SUPPLEMENTARY.md`:
- **OL-001..006** — Online lesson scheduling, capacity, recording, access, retention, attendance
- **QA-001..007** — Quiz creation, scheduling, attempts, submission, auto-grading, result release, history
- **LM-001..007** — Learning material categorization, file limits, virus scan, versioning, access, downloads, storage quota
- **TT-001..007** — Timetable structure, teacher/room/student conflict detection, workload limits, publication, variations
- **EI-001..004** — Excel import file format, marks validation, preview, atomic transactions
- **TF-001..005** — Teacher feedback (length, inappropriate language scan, edit window, lock, visibility)
- **AT-001..003** — Attendance register: present/absent/late/excused, locking after midnight, parent notification

From `PSMS-User-Stories.md` §2:
- 13 user stories `US-TCH-001` to `US-TCH-013` mapped one-to-one with feature tickets below.

From `PSMS-Permissions-Matrix.md`:
- Teacher role scope is **"Assigned classes/subjects only"** (📚). Every teacher-facing query must filter by the current user's assignments — never expose other teachers' or other classes' data.

### 1.3 Requirements scope (this engagement)

- **In scope**: build the teacher portal (LayoutShell + dashboard + 13 feature pages) using existing backend services and existing frontend providers.
- **Out of scope**: backend changes, new providers, third-party integrations (WebRTC for live video, antivirus scanning, etc.). Where a feature *requires* such integration (e.g. US-TCH-005 "Host Live Online Class"), the ticket either stubs the UI with a "platform integration pending" notice or is deferred.

---

## 2. Workflow (step 7 of user's plan)

For each ticket, in order:

1. `git checkout master && git pull` (if remote available)
2. `git checkout -b <branch-name>`
3. Implement the ticket
4. **Build verification** — `npx tsc --noEmit` (frontend) and `dotnet build` (backend, only if backend was touched)
5. **Senior dev review #1** — spawn `code-reviewer` subagent with diff + ticket context; fix all findings
6. **Senior dev review #2** — spawn another `code-reviewer` subagent for a second independent pass; fix all findings
7. Commit, merge into `master` (`--no-ff`), delete the branch
8. Move to the next ticket

**Local-only branching** (no push). When all tickets land, you push `master` yourself.

---

## 3. Ticket list

Sixteen tickets total. T-T01 → T-T04 are foundation (blocking). T-T05+ are features and can be reordered.

### T-T01 — [Foundation] Teacher LayoutShell + sidebar nav
**Scope**: New `app/teacher/layout.tsx` wrapping the existing `LayoutShell` with teacher-specific menu groups: Dashboard, My Classes, My Schedule, Learning Materials, Online Lessons, Assessments, Attendance, Marks, Messages.
**Done when**: navigation works, role guard restricts to `Teacher`, no console errors, build green.
**Blocks**: every other ticket.

### T-T02 — [Foundation] Teacher dashboard with real metrics
**Scope**: Replace stub `TeacherDashboard.tsx`. Metrics: My Classes count (TeacherClass scoped to current user), Total Students across my classes, Pending Mark Sheets (assessments with `markCount < students`), Today's Lessons (OnlineLessons today + Timetable slots today). Recent activity feed: upcoming lessons, recent announcements targeted to teachers.
**Done when**: all card values pull from providers, no hardcoded zeros, error states present.
**Depends on**: T-T01.

### T-T03 — [Foundation] My Classes overview page
**Scope**: `app/teacher/classes/page.tsx`. Lists the classes the current teacher is assigned to (`ITeacherClassAppService.GetByTeacher`). Card per class with: class name, grade, subject, student count, links to "Roster", "Attendance", "Mark Sheets".
**Done when**: only shows current teacher's classes; click into a class shows the detail.
**Depends on**: T-T01.

### T-T04 — [Foundation, MUST] My Schedule / Timetable view (US-TCH-013)
**Scope**: `app/teacher/schedule/page.tsx`. Weekly timetable grid showing the teacher's periods (TimetableSlots where teacherId = current user). Tabs for daily/weekly/term view. Filter by date range. Export to PDF/iCal deferred to a follow-up ticket.
**Business rules**: SBR-TT-001 (timetable structure), TT-005 (workload limits — show "X periods this week").
**Depends on**: T-T01.

### T-T05 — [MUST] Upload Learning Materials (US-TCH-001)
**Scope**: `app/teacher/materials/page.tsx` + `MaterialUploadModal`. File upload (PDF/DOC/PPT/MP4/MP3/images/ZIP). Form: title (5-200 chars), description (10-1000), grade, subject, term, material type, core/supplementary toggle, up to 5 tags. Validation via Zod. Submit via `LearningMaterialProvider.createAsync`.
**Business rules**: LM-001..007. File size validation (50 MB docs, 500 MB videos). Virus scan happens server-side — surface backend rejection.
**Depends on**: T-T01.

### T-T06 — [SHOULD] Learning Materials Library (US-TCH-002)
**Scope**: List view of all teacher's materials with filter (grade/subject/term/type) + search by title/tags + edit metadata + archive + download stats (`viewCount` from list DTO) + storage quota indicator.
**Business rules**: LM-002, LM-006, LM-007 (quota warning at 80%).
**Depends on**: T-T05.

### T-T07 — [SHOULD] Learning Material Versioning (US-TCH-003)
**Scope**: Upload-new-version flow from existing material; auto-increment version (`1.0 → 1.1`). Required change description. Version history viewer; restore previous.
**Business rules**: LM-003 (retain up to 10 versions).
**Depends on**: T-T06. **Note**: backend already supports update; this ticket is a UI-only wrapper.

### T-T08 — [MUST] Schedule Online Lesson (US-TCH-004)
**Scope**: `app/teacher/lessons/page.tsx` + `ScheduleLessonModal`. Form: subject, class, date+time (≥24 h ahead), duration (30-180 min), title, description, attachments. Conflict pre-check via `getByClassSubjectAsync`. Validate via Zod: school hours 07:00-17:00 SA.
**Business rules**: OL-001 (scheduling), OL-002 (capacity 100).
**Depends on**: T-T01.

### T-T09 — [MUST, PLATFORM-DEPENDENT] Host Live Online Class (US-TCH-005)
**Scope**: "Start Class" button on a scheduled lesson; calls `OnlineLessonProvider.startAsync` which the backend should generate a meeting link for. Frontend embeds the link in an iframe or opens externally. Recording control hooks deferred.
**Business rules**: OL-002, OL-006.
**Decision**: ship a *gated* version of this — UI to start/end + show meeting link copy button, no actual WebRTC embedding. Flag in PR that live-class infra is a separate epic.
**Depends on**: T-T08.

### T-T10 — [MUST] Upload Lesson Recording (US-TCH-006)
**Scope**: Recording upload modal on a completed lesson. File picker (MP4/MOV/AVI/WebM, max 5 GB shown as advisory — actual cap server-side). Required: link to lesson, filename format Subject-Grade-Date-Topic. Submit via `AddRecordingAsync`.
**Business rules**: OL-003, OL-004 (access control server-side), OL-005 (retention server-side).
**Depends on**: T-T08.

### T-T11 — [MUST] Create Quiz / Assessment (US-TCH-007)
**Scope**: `app/teacher/assessments/page.tsx` + `AssessmentFormModal` + `QuestionBuilder`. Assessment-level fields: title, description, classSubjectId, termId, type, maxMarks, weight, passPercentage, scheduledDate, dueDate, durationMinutes, instructions. Inline question builder: question text (10-1000), 2-6 options (1-500 each), one correct answer. Min 5 / max 100 questions.
**Business rules**: QA-001 (structure), QA-002 (scheduling rules).
**Depends on**: T-T01.

### T-T12 — [MUST] Capture Student Marks (US-TCH-008)
**Scope**: `app/teacher/mark-sheets/page.tsx`. Filterable list of assessments → click into mark capture page. Spreadsheet-style: row per enrolled student, columns for mark, feedback, percent (auto), achievement level (auto), notes. Bulk save via `BulkRecordMarksAsync`.
**Business rules**: BR-GR-001..006 (mark range, percentage calc, 7-level achievement). TF-001..005 (feedback length 20-2000, edit window 48 h server-side).
**Depends on**: T-T11.

### T-T13 — [SHOULD] Excel Mark Import (US-TCH-009)
**Scope**: "Import marks" button on the mark capture page. Two-step flow: 1) Download template (`.xlsx` with headers: StudentIdNumber, SubjectCode, AssessmentType, Mark, MaxMark) → 2) Upload file, preview first 10 rows + validation summary, confirm.
**Business rules**: EI-001..004 (atomic, preview-then-commit).
**Decision**: client-side file parsing via `xlsx` package (already a dependency); server-side validation + atomic insert via existing `BulkRecordMarksAsync`. If backend does not have a dedicated import endpoint, defer.
**Depends on**: T-T12.

### T-T14 — [SHOULD] Provide Student Feedback (US-TCH-010)
**Scope**: Feedback textarea on the mark capture row (already part of T-T12 if backend `RecordMarkDto` accepts it). This ticket covers the *edit-after-publish* flow within the 48-hour edit window, with formatting helpers and edit-history display.
**Business rules**: TF-001..005.
**Depends on**: T-T12.

### T-T15 — [MUST] Class Attendance Register (US-TCH-011)
**Scope**: `app/teacher/attendance/page.tsx`. Select class + date (default today, no future dates) → list of enrolled students with quick-mark buttons (Present/Absent/Late/Excused) and per-row notes. Bulk save via `BulkCaptureAsync`. Lock visual indicator when current day passes midnight.
**Business rules**: AT-001..003. Parent notification on same-day absence is server-side.
**Depends on**: T-T01.

### T-T16 — [SHOULD] Send Messages to Parents (US-TCH-012)
**Scope**: `app/teacher/messages/page.tsx`. Reuse existing `MessagesPageContent` pattern (inbox/sent tabs + Compose). Compose: recipient picker scoped to parents of students in teacher's classes (multi-select). Subject + body + attachment.
**Business rules**: BR-CM-002 (audit trail server-side).
**Depends on**: T-T01. **Note**: existing `MessagesPageContent` and `ComposeMessageModal` are reusable; ticket is mostly nav-wiring + recipient filter.

---

## 4. GH issue labels

When creating issues, apply labels:
- `area:teacher-portal` — all 16
- `kind:foundation` — T-T01..T-T04
- `kind:feature` — T-T05..T-T16
- `priority:must` — MUST stories
- `priority:should` — SHOULD stories
- `blocked` — added/removed as deps land

---

## 5. Status (updated as tickets land)

| # | Ticket | Branch | Status |
|---|---|---|---|
| T-T01 | Teacher LayoutShell + nav | — | OPEN |
| T-T02 | Teacher dashboard | — | OPEN (blocked by T-T01) |
| T-T03 | My Classes | — | OPEN (blocked by T-T01) |
| T-T04 | My Schedule | — | OPEN (blocked by T-T01) |
| T-T05 | Upload Materials | — | OPEN (blocked by T-T01) |
| T-T06 | Materials Library | — | OPEN (blocked by T-T05) |
| T-T07 | Material Versioning | — | OPEN (blocked by T-T06) |
| T-T08 | Schedule Online Lesson | — | OPEN (blocked by T-T01) |
| T-T09 | Host Live Class | — | OPEN (blocked by T-T08, platform-dep) |
| T-T10 | Upload Recording | — | OPEN (blocked by T-T08) |
| T-T11 | Create Quiz | — | OPEN (blocked by T-T01) |
| T-T12 | Capture Marks | — | OPEN (blocked by T-T11) |
| T-T13 | Excel Import | — | OPEN (blocked by T-T12) |
| T-T14 | Student Feedback | — | OPEN (blocked by T-T12) |
| T-T15 | Attendance Register | — | OPEN (blocked by T-T01) |
| T-T16 | Parent Messaging | — | OPEN (blocked by T-T01) |
