# Creates the 16 teacher portal tickets as GitHub issues.
# Idempotent-ish: will fail loudly if an issue with the same title already exists.
# Run from repo root.

$ErrorActionPreference = 'Stop'
$gh = 'C:\Program Files\GitHub CLI\gh.exe'
$repo = 'hlakudi200/PSMS'

# Issue body wrappers
function NewBody {
    param([string]$Summary, [string]$Scope, [string]$AcceptanceCriteria, [string]$BusinessRules, [string]$Depends)
    @"
## Summary
$Summary

## Scope
$Scope

## Acceptance Criteria
$AcceptanceCriteria

## Business Rules
$BusinessRules

## Depends on
$Depends

---
Source: backend/docs/TEACHER-PORTAL-PLAN.md
"@
}

$issues = @(
    @{
        title = 'T-T01 Foundation: Teacher LayoutShell + sidebar nav'
        labels = 'area:teacher-portal,kind:foundation,priority:must,frontend'
        summary = 'Build the teacher portal LayoutShell so every teacher route renders inside a consistent sidebar + topbar + ProtectedRoute, mirroring the principal portal pattern.'
        scope = "- Add ``frontend/src/app/teacher/layout.tsx`` wrapping ``LayoutShell``.`n- Menu groups: Dashboard, My Classes, My Schedule, Materials, Lessons, Assessments, Attendance, Marks, Messages.`n- ``allowedRoles: ['Teacher']``, accent color from ``roleColors.Teacher``."
        ac = "- [ ] Teacher navigates between sub-routes without re-mounting the shell.`n- [ ] Non-Teacher roles are redirected.`n- [ ] Sidebar collapse persists per the LayoutShell pattern.`n- [ ] ``npx tsc --noEmit`` clean."
        br = 'N/A (foundation)'
        depends = 'none — blocks every other teacher ticket'
    },
    @{
        title = 'T-T02 Foundation: Teacher dashboard with real metrics'
        labels = 'area:teacher-portal,kind:foundation,priority:must,frontend'
        summary = 'Replace the stub TeacherDashboard with real metrics and a recent-activity feed pulled from existing providers.'
        scope = "- Metrics cards: My Classes (TeacherClass scoped to current user), Total Students across my classes, Pending Mark Sheets (assessments with markCount < students), Today''s Lessons (OnlineLessons today + Timetable slots today).`n- Recent activity feed: upcoming lessons, recent announcements targeted to teachers.`n- Error states + loading states."
        ac = "- [ ] No hardcoded zeros — all values come from providers.`n- [ ] Cards are keyboard-accessible (role=button, tabIndex, onKeyDown).`n- [ ] Uses ``variant=`"borderless`"`` not deprecated ``bordered={false}``.`n- [ ] Failed fetches surface via ``<Alert>`` inside the relevant card."
        br = 'SBR-DB-001..004 (Dashboard rules from the supplementary doc).'
        depends = '#T-T01'
    },
    @{
        title = 'T-T03 Foundation: My Classes overview page'
        labels = 'area:teacher-portal,kind:foundation,priority:must,frontend'
        summary = 'List the classes the current teacher is assigned to, with quick links into roster/attendance/marks for each class.'
        scope = "- ``app/teacher/classes/page.tsx``.`n- Provider call: ``ITeacherClassAppService.GetByTeacher`` scoped to current user.`n- Card per class: class name, grade, subject, student count.`n- Quick links: View Roster, Take Attendance, Mark Sheets."
        ac = "- [ ] Only the current teacher''s assigned classes appear.`n- [ ] Empty state when no classes assigned.`n- [ ] Clicking a class opens its detail (separate ticket scope).`n- [ ] tsc clean."
        br = '📚 Permissions Matrix: teacher scope = assigned classes/subjects only.'
        depends = '#T-T01'
    },
    @{
        title = 'T-T04 My Schedule / Timetable view (US-TCH-013)'
        labels = 'area:teacher-portal,kind:feature,priority:must,frontend'
        summary = "Weekly timetable grid showing the teacher's own periods."
        scope = "- ``app/teacher/schedule/page.tsx``.`n- Filter TimetableSlots by ``teacherId = current user``.`n- Tabs for daily / weekly / term view.`n- Range filter.`n- Show total teaching periods per week (workload indicator)."
        ac = "- [ ] Grid renders periods correctly (subject / grade / room / time).`n- [ ] Free periods visible.`n- [ ] Workload count visible.`n- [ ] PDF / iCal export deferred to a follow-up ticket — not required here."
        br = 'SBR-TT-001 (structure), TT-005 (workload limits — max 35 periods/week SA labor law).'
        depends = '#T-T01'
    },
    @{
        title = 'T-T05 Upload Learning Materials (US-TCH-001)'
        labels = 'area:teacher-portal,kind:feature,priority:must,frontend'
        summary = 'Materials list + upload modal with Zod-validated form.'
        scope = "- ``app/teacher/materials/page.tsx`` + ``MaterialUploadModal``.`n- File picker: PDF/DOC/PPT/MP4/MP3/images/ZIP. Size: docs ≤ 50 MB, video ≤ 500 MB.`n- Form (Zod): title 5-200, description 10-1000, grade, subject, term, material type, core/supp, up to 5 tags.`n- Submit via ``LearningMaterialProvider.createAsync``."
        ac = "- [ ] Validation runs before submit.`n- [ ] Server rejection (e.g. virus scan failure) is surfaced via the axios interceptor.`n- [ ] Success toast only on actual success (catch path stays silent — interceptor shows the error).`n- [ ] tsc clean."
        br = 'LM-001..007.'
        depends = '#T-T01'
    },
    @{
        title = 'T-T06 Learning Materials Library (US-TCH-002)'
        labels = 'area:teacher-portal,kind:feature,priority:should,frontend'
        summary = 'Filter, search, edit, archive, view download stats. Storage quota indicator.'
        scope = "- Filter chips: grade / subject / term / material type.`n- Search by title or tag.`n- Edit metadata (title / description / tags).`n- Archive / unarchive.`n- viewCount per material from list DTO.`n- Quota bar (warn at 80%, block at 100% — backend-enforced)."
        ac = "- [ ] Filters and search reflect server state, not just client filtering.`n- [ ] Quota indicator visible.`n- [ ] tsc clean."
        br = 'LM-002, LM-006, LM-007.'
        depends = '#T-T05'
    },
    @{
        title = 'T-T07 Material Versioning (US-TCH-003)'
        labels = 'area:teacher-portal,kind:feature,priority:should,frontend'
        summary = 'Upload-new-version flow with auto-incrementing version, change description, restore previous.'
        scope = "- Action on existing material: ``Upload new version``.`n- Required: change description.`n- Auto-increment displayed version (1.0 → 1.1).`n- Version history drawer with download + restore buttons."
        ac = "- [ ] Old version reachable from history.`n- [ ] Restore creates a new version equal to a chosen old one — never overwrites history.`n- [ ] tsc clean."
        br = 'LM-003 (retain up to 10 versions).'
        depends = '#T-T06'
    },
    @{
        title = 'T-T08 Schedule Online Lesson (US-TCH-004)'
        labels = 'area:teacher-portal,kind:feature,priority:must,frontend'
        summary = "Teacher's online lessons page + schedule modal."
        scope = "- ``app/teacher/lessons/page.tsx`` + ``ScheduleLessonModal``.`n- Form (Zod): subject / class, scheduled date+time (≥ 24 h ahead), duration 30–180 min, title, description, attachments.`n- Conflict pre-check by calling ``getByClassSubjectAsync`` for the chosen class and showing overlap warnings.`n- Validate school hours 07:00–17:00 SA."
        ac = "- [ ] Cannot schedule less than 24 hours ahead.`n- [ ] Conflict warning shows before submit.`n- [ ] Lesson appears in list on success.`n- [ ] tsc clean."
        br = 'OL-001 (scheduling), OL-002 (≤ 100 participants).'
        depends = '#T-T01'
    },
    @{
        title = 'T-T09 Host Live Class shell (US-TCH-005)'
        labels = 'area:teacher-portal,kind:feature,priority:must,frontend'
        summary = 'UI scaffolding for starting / ending a live class. WebRTC embedding deferred.'
        scope = "- ``Start`` button on scheduled lessons → calls ``OnlineLessonProvider.startAsync``.`n- After start: show meeting link with copy-to-clipboard + ``Open meeting`` button.`n- ``End`` button → ``endAsync`` with attendee count.`n- ``Cancel`` button → ``cancelAsync``."
        ac = "- [ ] Start link active only 15 min before scheduled start (server-side check).`n- [ ] End is irreversible; UI confirms.`n- [ ] Live video embed is OUT OF SCOPE — flagged on the PR description."
        br = 'OL-002, OL-006.'
        depends = '#T-T08'
    },
    @{
        title = 'T-T10 Upload Lesson Recording (US-TCH-006)'
        labels = 'area:teacher-portal,kind:feature,priority:must,frontend'
        summary = 'Recording upload modal on completed lessons.'
        scope = "- Available on lessons with status = Completed.`n- File picker: MP4 / MOV / AVI / WebM (5 GB advisory cap, server-enforced).`n- Required: link to lesson, filename includes Subject-Grade-Date-Topic.`n- Submit via ``addRecordingAsync``."
        ac = "- [ ] Cannot attach a recording to a not-yet-completed lesson.`n- [ ] Server-side rejections (size, virus scan) surface via interceptor.`n- [ ] tsc clean."
        br = 'OL-003, OL-004 (access server-side), OL-005 (retention server-side).'
        depends = '#T-T08'
    },
    @{
        title = 'T-T11 Create Quiz / Assessment (US-TCH-007)'
        labels = 'area:teacher-portal,kind:feature,priority:must,frontend'
        summary = 'Assessments page + form modal + inline QuestionBuilder.'
        scope = "- ``app/teacher/assessments/page.tsx`` + ``AssessmentFormModal`` + ``QuestionBuilder`` panel.`n- Assessment fields (Zod): classSubjectId, termId, type, maxMarks, weight, passPercentage, scheduledDate, dueDate, durationMinutes, instructions.`n- QuestionBuilder: 5–100 questions, 2–6 options each, exactly one correct, text length bounds."
        ac = "- [ ] Cannot save with < 5 questions.`n- [ ] Each question requires one correct option.`n- [ ] Saving creates assessment + questions atomically.`n- [ ] tsc clean."
        br = 'QA-001 (structure), QA-002 (scheduling).'
        depends = '#T-T01'
    },
    @{
        title = 'T-T12 Capture Student Marks (US-TCH-008)'
        labels = 'area:teacher-portal,kind:feature,priority:must,frontend'
        summary = 'Spreadsheet-style mark capture per assessment.'
        scope = "- ``app/teacher/mark-sheets/page.tsx`` lists assessments → click in opens mark-capture page.`n- Row per enrolled student: mark / percent (auto) / achievement level (auto, SA 7-level) / feedback / notes.`n- Bulk save via ``BulkRecordMarksAsync``.`n- Locked indicator for published marks."
        ac = "- [ ] Mark range validation client + server.`n- [ ] Achievement level auto-derives from percent.`n- [ ] Cannot edit a published mark unless server returns unlocked.`n- [ ] tsc clean."
        br = 'BR-GR-001..006 (mark range, percentage, 7-level). TF-001..005 (feedback length 20–2000).'
        depends = '#T-T11'
    },
    @{
        title = 'T-T13 Excel Mark Import (US-TCH-009)'
        labels = 'area:teacher-portal,kind:feature,priority:should,frontend'
        summary = 'Bulk mark import via Excel: download template, preview, validate, atomic commit.'
        scope = "- ``Import marks`` button on mark capture page.`n- Step 1: download .xlsx template (headers: StudentIdNumber, SubjectCode, AssessmentType, Mark, MaxMark).`n- Step 2: upload (XLSX/XLS/CSV ≤ 10 MB, ≤ 1000 rows).`n- Step 3: preview first 10 rows + validation summary (total / valid / invalid) + downloadable error report.`n- Step 4: confirm → atomic commit through existing ``BulkRecordMarksAsync``."
        ac = "- [ ] Any invalid row blocks import.`n- [ ] Preview shows correct headers detected.`n- [ ] Success message shows row count imported.`n- [ ] tsc clean."
        br = 'EI-001..004 (file format, validation, preview, atomic transactions).'
        depends = '#T-T12'
    },
    @{
        title = 'T-T14 Student Feedback edit-after-publish (US-TCH-010)'
        labels = 'area:teacher-portal,kind:feature,priority:should,frontend'
        summary = '48-hour edit window for feedback on published marks + edit history.'
        scope = "- Edit-feedback flow on a published mark.`n- Server enforces 48-h window. UI checks ``publishedAt`` and disables edit after 48 h.`n- Edit history drawer with timestamps.`n- Inappropriate-language scan happens server-side; UI shows rejection."
        ac = "- [ ] Within 48 h: edit allowed, history updated.`n- [ ] After 48 h: edit blocked client-side and server-side.`n- [ ] tsc clean."
        br = 'TF-001..005.'
        depends = '#T-T12'
    },
    @{
        title = 'T-T15 Class Attendance Register (US-TCH-011)'
        labels = 'area:teacher-portal,kind:feature,priority:must,frontend'
        summary = 'Daily class attendance register.'
        scope = "- ``app/teacher/attendance/page.tsx``.`n- Select class + date (no future dates, default today).`n- List of enrolled students with quick-mark Present/Absent/Late/Excused + per-row notes for absences.`n- Bulk save via ``BulkCaptureAsync``.`n- Visual lock indicator when current day passes midnight."
        ac = "- [ ] Future-date selection disabled.`n- [ ] After midnight: form locked unless admin override (server-side).`n- [ ] Save success shows summary (e.g. ``28 present, 2 absent``).`n- [ ] tsc clean."
        br = 'AT-001..003 (statuses, midnight lock, parent same-day notification — server-side).'
        depends = '#T-T01'
    },
    @{
        title = 'T-T16 Send Messages to Parents (US-TCH-012)'
        labels = 'area:teacher-portal,kind:feature,priority:should,frontend'
        summary = "Adapt existing MessagesPageContent for the teacher portal with a parent-recipient picker."
        scope = "- ``app/teacher/messages/page.tsx``.`n- Reuse existing ``MessagesPageContent`` + ``ComposeMessageModal``.`n- Compose recipient picker scoped to: individual parent, all parents in one of my classes, parents of selected students.`n- Subject + body + attachment (≤ 10 MB)."
        ac = "- [ ] Recipient picker only shows parents of students in classes I teach.`n- [ ] Threading works (existing pattern).`n- [ ] tsc clean."
        br = 'BR-CM-002 (audit trail server-side).'
        depends = '#T-T01'
    }
)

$created = @()
foreach ($i in $issues) {
    $body = NewBody -Summary $i.summary -Scope $i.scope -AcceptanceCriteria $i.ac -BusinessRules $i.br -Depends $i.depends
    $bodyFile = New-TemporaryFile
    Set-Content -Path $bodyFile -Value $body -Encoding UTF8
    $url = & $gh issue create --repo $repo --title $i.title --body-file $bodyFile.FullName --label $i.labels 2>&1 | Out-String
    Remove-Item $bodyFile -Force
    $url = $url.Trim()
    Write-Output "$($i.title) => $url"
    $created += [PSCustomObject]@{ title = $i.title; url = $url }
}

Write-Output ''
Write-Output "Created $($created.Count) issues."
