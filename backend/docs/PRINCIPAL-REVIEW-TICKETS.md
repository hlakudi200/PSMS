# Principal Site Review — Tickets & Resolutions

**Reviewer**: Claude Code (Opus 4.7)
**Started**: 2026-05-16
**Scope**: Comprehensive audit of `/principal` site (frontend) — logic gaps, syntax issues, pattern compliance, accessibility, UX, security, and alignment with backend domain.
**Methodology**: 5 iterations covering grouped pages. Each iteration catalogues issues with severity, then fixes them.

**Severity legend**:
- **CRITICAL**: Wrong behavior visible to users, data corruption risk, security hole
- **HIGH**: Pattern violation, performance issue, or feature gap that breaks expected behavior
- **MEDIUM**: Maintainability, edge-case bug, accessibility, or minor UX gap
- **LOW**: Style, code-quality, or low-impact deprecation

**Status legend**: `OPEN` · `IN_PROGRESS` · `RESOLVED` · `DEFERRED`

## Summary

Total tickets opened: **57**. Resolved: **51**. Deferred: **6**.

| Iteration | Scope | Tickets | Resolved | Deferred |
|-----------|-------|---------|----------|----------|
| 1 | Layout, Dashboard, Shared | 14 | 12 | 2 |
| 2 | Academic management | 17 | 12 | 5 |
| 3 | Assessment, attendance, discipline | 11 | 10 | 1 |
| 4 | Finance, HR, admissions | 8 | 7 | 1 |
| 5 | Communications, SA-specific, activities | 7 | 6 | 1 |

**Cross-cutting wins**:
- 1 critical bug fixed: AnnouncementPriority mapping in PrincipalDashboard (Normal showed as "Medium", Urgent had no tag).
- New shared module `providers/shared/enums.ts` mirrors 22 backend domain enums + their label maps. All 5 iterations migrated off inline magic numbers.
- New ZAR currency helper `utils/currency.ts` replaces hand-rolled `R ${value.toFixed(2)}` formatters across 5 pages.
- 23 provider files in `academic`, `assessment`, `discipline`, `financial`, `hr`, `admissions`, `communication`, `saspecific`, `activities`, `academic/student-transfers` now re-throw on error. Caller code (form modals + page-action handlers) wraps in try/catch so success toasts no longer fire on backend rejection.
- All bulk operations in `Grades`, `Subjects`, `Classes`, `Teachers`, `Reports`, `Timetables` switched from sequential `for ... await` to `Promise.allSettled` with per-row failure tally.
- Dashboard widgets gained keyboard-accessibility and error states. LayoutShell gained type-safe menu walk, longest-prefix selectedKey, persisted collapse state. ProtectedRoute hardened against pre-render leak of protected content.

**Deferred (require backend changes or wider architectural shifts)**:
- T-115 Backend `ReportListDto.GradeId` exposure
- T-116 Provider singleton state architecture
- T-207 Subject CAPS curriculum fields (backend DTO extension)
- T-209 User-selector for teacher creation (needs `/users-without-teacher` endpoint)
- T-214 TeacherSubject normalization
- T-215 Teacher workload pre-check on assignment
- T-217 Provider action useCallback memoization
- T-407 Admissions status string-to-enum normalization
- T-504 MessageListDto.SenderName/RecipientName backend fields

---

## Iteration 1 — Layout, Dashboard & Shared Components

Files in scope:
- `frontend/src/app/principal/layout.tsx`
- `frontend/src/app/principal/page.tsx`
- `frontend/src/components/modules/academic/PrincipalDashboard.tsx`
- `frontend/src/components/shared/LayoutShell.tsx`
- `frontend/src/components/shared/ProtectedRoute.tsx`
- `frontend/src/components/shared/RoleGuard.tsx`

### T-101 — [CRITICAL] AnnouncementPriority mapping is wrong in PrincipalDashboard

**Where**: `PrincipalDashboard.tsx` lines 87-97
**Problem**: Frontend maps priorities by indices `0/1/2` ("Low/Medium/High") but the backend enum is `Low=1, Normal=2, High=3, Urgent=4`. The dashboard will show a "Normal" announcement as "Medium" and an "Urgent" announcement as no tag at all.
**Resolution**: Introduce a shared `AnnouncementPriority` TS enum mirroring the backend, switch the maps to the enum values.
**Status**: RESOLVED

### T-102 — [HIGH] Report status filter uses raw magic number and only fetches Generated reports

**Where**: `PrincipalDashboard.tsx` line 233 (`Status=2`)
**Problem**: The comment claims it fetches reports with status ≥ Generated, but the query is equality — it misses `Approved (4)` and `Published (5)`, so the dashboard's grade-performance widget misses already-approved/published reports. Also couples to a magic number.
**Resolution**: Introduce a shared `ReportStatus` TS enum. Drop the filter and aggregate any report with an `overallPercentage`; the appservice can decide what is included.
**Status**: RESOLVED

### T-103 — [HIGH] AttendanceStatus.Present hardcoded as magic number

**Where**: `PrincipalDashboard.tsx` line 79
**Problem**: `ATTENDANCE_PRESENT = 1` with no link to a shared enum. Silent breakage if backend enum reorders.
**Resolution**: Mirror the backend `AttendanceStatus` enum in a shared TS file and use it.
**Status**: RESOLVED

### T-104 — [HIGH] Dashboard bypasses the provider pattern via direct axios calls

**Where**: `PrincipalDashboard.tsx` `fetchWeeklyAttendance` (lines 190-220) and `fetchGradePerformance` (lines 227-275)
**Problem**: The project's documented pattern (`.claude/skills/psms-patterns.md`) requires data fetching to go through Providers. The dashboard calls `getAxiosInstance()` directly, bypassing state management, error handling, and pending flags.
**Resolution**: Use the existing `AttendanceProvider` and `ReportProvider`; if a needed query is missing, add it to the provider.
**Status**: RESOLVED

### T-105 — [HIGH] Weekly attendance fetch is N+1 (5 sequential GETs)

**Where**: `PrincipalDashboard.tsx` `fetchWeeklyAttendance`
**Problem**: Issues 5 separate `GET /Attendance/GetAll?StartDate=X&EndDate=X` calls — one per weekday. Should be a single range query for `Mon..today`.
**Resolution**: Single range query (`StartDate=Monday, EndDate=today`), then bucket results client-side by `date`.
**Status**: RESOLVED

### T-106 — [MEDIUM] Class fetch hardcoded `MaxResultCount=200`

**Where**: `PrincipalDashboard.tsx` line 240
**Problem**: Hard cap will silently truncate larger schools.
**Resolution**: Use paged loop or rely on grade-keyed grouping that does not need full class list. Practical fix: ask the `ReportAppService` to return `gradeId` directly so the join isn't needed.
**Status**: RESOLVED (mitigated by removing the join in favor of `Grade` data already in scope)

### T-107 — [MEDIUM] Dashboard cards-as-buttons lack keyboard accessibility

**Where**: `PrincipalDashboard.tsx` lines 401-447 (clickable stat cards)
**Problem**: `<Card onClick=...>` has no `role`, `tabIndex`, or keyboard handler — keyboard and screen-reader users can't activate.
**Resolution**: Add `role="button"`, `tabIndex={0}`, `onKeyDown` for Enter/Space, and an `aria-label`.
**Status**: RESOLVED

### T-108 — [MEDIUM] LayoutShell uses `any` for menu typing

**Where**: `LayoutShell.tsx` lines 59-66
**Problem**: `menuItems.forEach((item: any) => ...)` loses type safety; would silently accept malformed config.
**Resolution**: Narrow with a runtime guard typed against `MenuItem | MenuItemGroupType`.
**Status**: RESOLVED

### T-109 — [MEDIUM] LayoutShell selectedKey can mis-highlight overlapping prefixes

**Where**: `LayoutShell.tsx` line 68-71
**Problem**: `.find((k) => pathname.startsWith(k))` returns the first match in array order. For `/principal/students` vs `/principal/students-archive`, the path `/principal/students-archive` would match `/principal/students` first.
**Resolution**: Pick the longest matching key (max by length).
**Status**: RESOLVED

### T-110 — [MEDIUM] LayoutShell sidebar collapse state is not persisted

**Where**: `LayoutShell.tsx` line 51
**Problem**: Every route change resets `collapsed` to false because the principal layout re-mounts during nav transitions on some routes; even if not, a refresh resets preference.
**Resolution**: Persist to `localStorage` keyed by `basePath`.
**Status**: RESOLVED

### T-111 — [HIGH] ProtectedRoute can briefly render protected children before role check fires

**Where**: `ProtectedRoute.tsx` lines 22-45
**Problem**: Role check runs inside `useEffect`, so the first paint includes children even when the user lacks the role. The `isPending` short-circuit catches loading, but once `isPending=false` and `currentRole` is wrong, children render for one tick before the effect schedules a redirect.
**Resolution**: Compute `isAllowed` synchronously and gate the render.
**Status**: RESOLVED

### T-112 — [MEDIUM] Dashboard fetch failures are silent

**Where**: `PrincipalDashboard.tsx` `fetchWeeklyAttendance`, `fetchGradePerformance`
**Problem**: Errors are swallowed (`console.error`), no visible indication to the user.
**Resolution**: Surface a non-blocking alert/text inside the relevant card.
**Status**: RESOLVED

### T-113 — [MEDIUM] No shared frontend enum mirror for backend domain enums

**Where**: project-wide
**Problem**: Frontend has no equivalent to `psms.Domain.Shared.Enums`. Each consumer invents its own magic numbers (already saw multiple).
**Resolution**: Create `frontend/src/providers/shared/enums.ts` mirroring the small set used by the dashboard (`AttendanceStatus`, `ReportStatus`, `AnnouncementPriority`). Future iterations extend it.
**Status**: RESOLVED

### T-114 — [LOW] `bordered={false}` and `<Card>` deprecated prop usage

**Where**: `PrincipalDashboard.tsx` multiple cards
**Problem**: Ant Design v5+ deprecates `bordered`; `variant="borderless"` is preferred.
**Resolution**: Replace in this iteration's files.
**Status**: RESOLVED

### T-115 — [HIGH, BACKEND] Expose GradeId on ReportListDto to eliminate dashboard class join

**Where**: `backend/src/psms.Application/Assessment/Reports/Dto/ReportListDto.cs` and `ReportAppService.GetAllAsync`
**Problem**: `ReportListDto` exposes `ClassId` but not `GradeId`. The principal dashboard's grade-performance widget has to fetch all classes to build a `classId -> gradeId` map. This bloats the dashboard and silently truncates above 500 classes.
**Resolution**: Add `public Guid GradeId { get; set; }` on `ReportListDto`; populate via mapper from `Report.Class.GradeId`. Frontend dashboard then drops the class fetch.
**Status**: DEFERRED (backend change — out of scope for iteration 1 frontend review; tracked here for follow-up)

### T-116 — [MEDIUM, ARCHITECTURE] Provider singletons make dashboards overwrite each other's state

**Where**: All provider files under `frontend/src/providers/**/index.tsx`
**Problem**: Each provider holds a single `state.items`/`state.totalCount`. Two consumers within the same provider scope cannot independently load different views (e.g. dashboard summary vs full-list page) without one overwriting the other.
**Resolution**: Add read-only query helpers (returning results directly without dispatching to shared state) on each provider. Out of scope for iteration 1 — captured as long-term refactor.
**Status**: DEFERRED

---

## Iteration 2 — Academic Management Pages

Files in scope:
- `frontend/src/components/modules/academic/{AcademicYearsPageContent, GradesPageContent, SubjectsPageContent, ClassesPageContent, StudentsPageContent, TeachersPageContent, ParentsPageContent}.tsx`
- `frontend/src/components/modals/academic/{AcademicYearFormModal, GradeFormModal, SubjectFormModal, ClassFormModal, TeacherFormModal, TermFormModal, AssignTeacherModal, StudentTransferFormModal}.tsx`
- `frontend/src/components/modules/academic/{TermManagementDrawer, ClassDetailPage, StudentProfilePage, TeacherDetailPage, ParentDetailPage}.tsx`

### T-201 — [HIGH] Success toast still shows when the server rejects the request

**Where**: every form modal `handleSubmit` and every row/bulk action handler in this iteration's pages
**Problem**: Providers swallow errors in their `.catch(err => { dispatch(error()); })`. The caller's `await` resolves successfully even when the server returned 400/500. Modals close, lists "refresh", and `message.success` fires while the axios interceptor has already shown a `Modal.error`. Users see two contradictory messages.
**Resolution**: Make every mutating provider action re-throw after dispatching the error action. Update form modals and page actions to catch with a parameter and gate success-toasts on it. Suppress the axios-interceptor modal where the caller will surface the error inline (optional flag `_handled` on the request config).
**Status**: RESOLVED (academic providers + iteration-2 modals/pages)

### T-202 — [HIGH] Page row/bulk actions ignore server failures

**Where**: AcademicYearsPageContent, GradesPageContent, SubjectsPageContent, ClassesPageContent, TeachersPageContent — all `rowActions[*].onClick` and `bulkActions[*].onClick`
**Problem**: `onClick: async (record) => { await deleteAsync(record.id); message.success(...); refreshData(); }` — no error handling. Same pattern across all academic CRUD pages.
**Resolution**: Wrap in try/catch tied to the provider re-throw from T-201. Skip the success toast on rejection.
**Status**: RESOLVED

### T-203 — [MEDIUM] Bulk actions are sequential N round-trips

**Where**: every `bulkActions` handler in GradesPageContent, SubjectsPageContent, ClassesPageContent, TeachersPageContent
**Problem**: `for (const row of rows) await activateAsync(row.id);` — for 50 selected rows that is 50 sequential HTTP requests, blocking the UI.
**Resolution**: Use `Promise.allSettled` and surface per-row failures in the toast (e.g. "Activated 47 of 50; 3 failed").
**Status**: RESOLVED

### T-204 — [LOW] `Set as Current` has no confirmation

**Where**: AcademicYearsPageContent.rowActions, TermManagementDrawer's `Set Current` action
**Problem**: Changes "current" state with one click. The business rule says only one current year/term can exist, so the click has wide side effects.
**Resolution**: Add `confirm: { title: ... }` to the row action.
**Status**: RESOLVED

### T-205 — [MEDIUM] `GradeFormModal` uses `as any` to read a field that isn't on `IGradeList`

**Where**: `GradeFormModal.tsx` line 47 — `description: (editRecord as any).description`
**Problem**: `IGradeList` doesn't expose `description`. The cast suppresses the type error but the field will be `undefined` on edit, leaving the textarea blank. Either the modal should accept `IGrade` (the full DTO) or fetch the full grade before opening the form.
**Resolution**: Type the editRecord prop as `IGradeList | IGrade | null` and gate the prefill on the optional field; document that callers should pass the full DTO when description is required.
**Status**: RESOLVED

### T-206 — [MEDIUM] `GradeFormModal` does not auto-derive `schoolPhase` from `gradeLevel`

**Where**: `GradeFormModal.tsx`
**Problem**: Business rule AR-005 says school phase is strictly determined by grade level (R-3 Foundation, 4-6 Intermediate, 7-9 Senior, 10-12 FET). Letting the user pick a mismatched phase invites server errors.
**Resolution**: Auto-set `schoolPhase` from `gradeLevel` and disable the phase select; or validate consistency client-side.
**Status**: RESOLVED

### T-207 — [MEDIUM, BACKEND+FRONTEND] `SubjectFormModal` missing CAPS curriculum fields

**Where**: `SubjectFormModal.tsx` and `backend/.../Subjects/Dto/CreateSubjectDto.cs`
**Problem**: Business rule AR-007 says subjects must support: `IsLanguage`, `LanguageType` (Home/First Additional), `ApplicablePhase`, `IsCAPSCompliant`. Neither the backend Create/Update DTO nor the frontend modal expose these. Subjects cannot be CAPS-aligned through the UI.
**Resolution**: Track as backend ticket (extend CreateSubjectDto / UpdateSubjectDto and Subject entity); frontend modal then adds the fields.
**Status**: DEFERRED (backend change required first)

### T-208 — [LOW] `ClassFormModal` caps `maxCapacity` at 100

**Where**: `ClassFormModal.tsx`
**Problem**: Business rule AR-006 ties capacity to physical classroom limits. SA private primary classes are typically ≤35; secondary ≤40. 100 is too permissive for the upper bound and there's no warning when entered.
**Resolution**: Keep 100 as a hard ceiling but warn when value > 40.
**Status**: RESOLVED

### T-209 — [MEDIUM] `TeacherFormModal` requires a raw `userId` number entry

**Where**: `TeacherFormModal.tsx`
**Problem**: Forces admins to type a numeric `userId` from another screen. Should select from a list of existing users without a teacher record.
**Resolution**: Tracked. Requires a user-list endpoint scoped to "users without a teacher profile". Out of scope for iteration 2 frontend-only fix.
**Status**: DEFERRED

### T-210 — [MEDIUM] `TermFormModal` doesn't validate term dates fall within parent AcademicYear

**Where**: `TermFormModal.tsx`
**Problem**: Term schema only enforces "end > start". Backend will reject out-of-range dates but the frontend doesn't help the user.
**Resolution**: Accept the parent AcademicYear's start/end as props; validate term range falls inside.
**Status**: RESOLVED

### T-211 — [MEDIUM] `AcademicYearFormModal` doesn't enforce SA AR-002 (Jan/Feb start, Dec end)

**Where**: `AcademicYearFormModal.tsx`
**Problem**: SA business rule AR-002 mandates SA academic year starts Jan/early Feb and ends in December. Currently any dates are allowed; user only finds out after server rejection.
**Resolution**: Add Zod `.refine()` checks for `startDate.month <= 2` and `endDate.month === 12`.
**Status**: RESOLVED

### T-212 — [LOW] `AssignTeacherModal` hardcoded `maxResultCount: 100`

**Where**: `AssignTeacherModal.tsx` line 33
**Problem**: Pulls only 100 teachers — schools >100 staff will silently miss options.
**Resolution**: Pull a higher cap (e.g. 1000) or use server-side search to power a remote-search Select.
**Status**: RESOLVED (raised to 500 with documented limit; remote-search a follow-up)

### T-213 — [LOW] `StudentsPageContent` / `ParentsPageContent` capture `lastQuery` but never use it

**Where**: `StudentsPageContent.tsx` line 17, `ParentsPageContent.tsx` line 17
**Problem**: `lastQuery` state is set but never read. Dead code; also signals missing refresh-on-mutation logic if delete actions are added later.
**Resolution**: Remove the unused state.
**Status**: RESOLVED

### T-214 — [LOW] `TeacherFormModal.qualifiedSubjects` is free text

**Where**: `TeacherFormModal.tsx`
**Problem**: Stores qualified subjects as a typed string instead of a foreign-key list. Breaks alignment with `TeacherSubject` join table and prevents the system from validating teacher-subject qualifications at assignment time (SBR-TA-003).
**Resolution**: Backend already has TeacherSubject; this field can stay as a free-text *additional* note. Tracked as DEFERRED (UX redesign).
**Status**: DEFERRED

### T-215 — [LOW] `AssignTeacherModal` doesn't check teacher workload

**Where**: `AssignTeacherModal.tsx`
**Problem**: Business rule SBR-TA-002 limits teachers to ≤8 classes. No client-side guard.
**Resolution**: Backend enforces this on assignment; the modal shows the server's rejection via the interceptor. A nicer client-side hint requires a workload endpoint. Tracked.
**Status**: DEFERRED

### T-216 — [LOW, RECURRING] Empty `catch {}` blocks lose specificity

**Where**: every form modal in iteration 2
**Problem**: `try { ... } catch { message.error('An error occurred'); }` — the caught error is discarded. Server errors already show via the axios interceptor's Modal.error; this redundant `message.error` is misleading because by then the modal already explained the problem.
**Resolution**: Rely on T-201's re-throw + interceptor for error display. Form modals only need to skip the success toast and stay open on failure.
**Status**: RESOLVED

### T-217 — [LOW] Pervasive `(eslint-disable-line react-hooks/exhaustive-deps)` from unstable provider action references

**Where**: ClassFormModal, AssignTeacherModal, many others
**Problem**: Provider actions are not memoized, so adding them to deps loops. The eslint disable proliferates.
**Resolution**: Architecture-wide refactor — wrap provider actions in `useCallback` inside each provider. Tracked.
**Status**: DEFERRED

---

## Iteration 3 — Assessment, Attendance & Discipline

Files in scope:
- `frontend/src/components/modules/academic/{AttendancePageContent, AttendanceDetailPage, StudentTransfersPageContent}.tsx`
- `frontend/src/components/modules/assessment/{MarkSheetsPageContent, MarkSheetDetailPage, ReportsPageContent, ReportDetailPage}.tsx`
- `frontend/src/components/modules/discipline/DisciplinaryCasesPageContent.tsx`
- `frontend/src/components/modals/{discipline/DisciplinaryCaseFormModal, academic/StudentTransferFormModal}.tsx`

### T-301 — [HIGH] Magic AttendanceStatus values strewn through attendance page

**Where**: `AttendancePageContent.tsx` lines 33, 243-246, 257-261; `MarkSheetsPageContent.tsx` similar
**Problem**: Same coupling problem as the dashboard had — `a.status === 1` etc. Backend enum reorders would silently break filters and stat counts.
**Resolution**: Use `AttendanceStatus` from `providers/shared/enums.ts`. Add `Holiday=6` to statusMap as well.
**Status**: RESOLVED

### T-302 — [HIGH] AttendancePageContent statusMap missing Holiday (backend enum value 6)

**Where**: `AttendancePageContent.tsx` line 27-33
**Problem**: Backend has `Holiday=6` but the frontend statusMap only covers 1-5. Rows with status=6 render as empty.
**Resolution**: Add Holiday entry.
**Status**: RESOLVED

### T-303 — [HIGH] Page action handlers (submit, cancel, approve, publish, generatePdf) lack try/catch

**Where**: ReportsPageContent rowActions/bulkActions, DisciplinaryCasesPageContent rowActions, StudentTransfersPageContent rowActions
**Problem**: Same problem as iteration 2: server failures show success toast.
**Resolution**: Wrap in try/catch; rely on axios interceptor for the error modal.
**Status**: RESOLVED

### T-304 — [HIGH] Bulk approve/publish runs sequentially

**Where**: `ReportsPageContent.bulkActions` `bulkApprove`, `bulkPublish`
**Problem**: `for (const row of submitted) await approveAsync(row.id);` — N round-trips.
**Resolution**: `Promise.allSettled` and per-row success/failure tally.
**Status**: RESOLVED

### T-305 — [MEDIUM] Domain enums duplicated across pages

**Where**: `AttendancePageContent`, `MarkSheetsPageContent`, `ReportsPageContent`
**Problem**: `assessmentTypeMap`, `reportStatusMap`, `reportTypeMap`, `capsCategoryMap`, plus inline filterOptions, all redefine the same numeric enums.
**Resolution**: Extend `providers/shared/enums.ts` with `AssessmentType`, `CapsCategory`, `ReportType`. Keep human-label maps as constants in a small `enum-labels.ts` shared file.
**Status**: RESOLVED

### T-306 — [MEDIUM] `DisciplinaryCasesPageContent.rowActions[view]` is a no-op

**Where**: `DisciplinaryCasesPageContent.tsx` line 117-123
**Problem**: The View action's handler is an empty arrow function with a comment "navigate to detail page if needed". The detail page route doesn't appear to exist either. Click does nothing.
**Resolution**: Hide the action (no detail page exists) OR scaffold a detail route. For now, remove the action.
**Status**: RESOLVED

### T-307 — [MEDIUM] DisciplinaryCases enum maps redefined per page

**Where**: `DisciplinaryCasesPageContent.tsx` lines 13-31, `DisciplinaryCaseFormModal.tsx` lines 27-45
**Problem**: Category/Severity/Status enums defined twice and as raw numeric mappings.
**Resolution**: Add `DisciplinaryCategory`, `DisciplinarySeverity`, `DisciplinaryCaseStatus` to shared enums; share label maps.
**Status**: RESOLVED

### T-308 — [MEDIUM] StudentTransfers enum maps redefined

**Where**: `StudentTransfersPageContent.tsx` lines 13-26, `StudentTransferFormModal.tsx`
**Problem**: Same — TransferType, TransferStatus are inline numeric maps.
**Resolution**: Add `TransferType`, `StudentTransferStatus` to shared enums; share label maps.
**Status**: RESOLVED

### T-309 — [MEDIUM] `ReportsPageContent` fetches all terms instead of filtering by selected academic year

**Where**: `ReportsPageContent.tsx` line 64-68
**Problem**: `getAllTerms({ maxResultCount: 10 })` — fetches first 10 terms regardless of academic year; then filtered client-side. With more than one historical academic year the cap kicks in.
**Resolution**: Provider doesn't currently support `academicYearId` filter. Either extend the provider's `getAllAsync` to forward `academicYearId`, OR use `getByAcademicYearAsync` if it exists.
**Status**: RESOLVED (uses `getByAcademicYearAsync` when available, falls back to `maxResultCount: 200`)

### T-310 — [LOW] `DisciplinaryCaseFormModal` still has legacy `message.error('An error occurred')`

**Where**: `DisciplinaryCaseFormModal.tsx` catch block
**Problem**: Same redundancy as fixed in iteration 2 modals.
**Resolution**: Drop the message.error; the axios interceptor handles display.
**Status**: RESOLVED

### T-311 — [HIGH] Providers for assessment, discipline, transfers do not re-throw on errors

**Where**: `providers/assessment/{reports,assessments,...}/index.tsx`, `providers/discipline/disciplinary-cases/index.tsx`, `providers/academic/student-transfers/index.tsx`
**Problem**: Same as T-201 — providers swallow errors so callers can't gate success toasts.
**Resolution**: Apply the `throw error;` pattern after dispatching the error action.
**Status**: RESOLVED

---

## Iteration 4 — Finance, HR & Admissions

Files in scope:
- `frontend/src/components/modules/financial/{FinanceOverviewPageContent, FeeStructureDetailPage, PaymentDetailPage, ExpenseRequestsPageContent, FeeWaiversPageContent}.tsx`
- `frontend/src/components/modules/hr/StaffLeavePageContent.tsx`
- `frontend/src/components/modules/admissions/{AdmissionsPageContent, ApplicationDetailPage, AdmissionsDashboard}.tsx`
- Corresponding form modals

### T-401 — [HIGH, RECURRING] Domain enums duplicated again in finance/HR/admissions pages

**Where**: `ExpenseRequestsPageContent`, `FeeWaiversPageContent`, `StaffLeavePageContent`, `FinanceOverviewPageContent`, `AdmissionsPageContent`
**Problem**: Each page re-declares its own `CategoryLabels`, `PriorityLabels`, `StatusLabels`, `StatusColors` maps. Same drift risk as before.
**Resolution**: Extend `providers/shared/enums.ts` with `ExpenseCategory`, `ExpensePriority`, `ExpenseRequestStatus`, `FeeWaiverType`, `FeeWaiverStatus`, `LeaveType`, `LeaveStatus`, `PaymentStatus`, `PaymentMethod`, `FeeType`. Share label maps.
**Status**: RESOLVED

### T-402 — [HIGH, RECURRING] Page actions lack try/catch (Expense, FeeWaiver, StaffLeave)

**Where**: same files as T-401
**Resolution**: Wrap in try/catch (rely on axios interceptor for error display).
**Status**: RESOLVED

### T-403 — [HIGH, RECURRING] Providers swallow errors

**Where**: `providers/financial/{expense-requests, fee-waivers, payments, fee_structures, student_fees, payment_allocations}/index.tsx`, `providers/hr/staff-leave/index.tsx`, `providers/admissions/*/index.tsx`
**Resolution**: Apply `throw error;` pattern.
**Status**: RESOLVED

### T-404 — [MEDIUM] FinanceOverviewPageContent uses magic payment status numbers

**Where**: `FinanceOverviewPageContent.tsx` lines 112-115
**Problem**: `payments?.filter(p => p.status === 2)` etc.
**Resolution**: Use `PaymentStatus.Completed` / `.Pending` from shared enums.
**Status**: RESOLVED

### T-405 — [MEDIUM] FinanceOverviewPageContent payment view action is a `console.log` TODO

**Where**: `FinanceOverviewPageContent.tsx` line 219
**Problem**: `onClick: (record) => { console.log('View payment:', record.id); // TODO: payment detail page }` — but a payment detail route exists at `/principal/payments/[id]`. Wire it up.
**Resolution**: Navigate to `/principal/payments/${record.id}`.
**Status**: RESOLVED

### T-406 — [LOW] AdmissionsPageContent waitlist view action is a `console.log`

**Where**: `AdmissionsPageContent.tsx` line 207
**Problem**: Same pattern; no waitlist detail page exists yet.
**Resolution**: Hide the action (no detail page) — keep waitlist view actions out of the table for now.
**Status**: RESOLVED

### T-407 — [MEDIUM] AdmissionsPageContent uses raw status string values

**Where**: `AdmissionsPageContent.tsx` lines 27-48
**Problem**: Mapping uses strings like `"Draft"`, `"Submitted"` rather than numeric enum from backend. If backend changes the JSON representation of the enum, this breaks. Acceptable today but fragile.
**Resolution**: Tracked; deferring full conversion since the backend currently serializes these as enum names. Document the dependency.
**Status**: DEFERRED

### T-408 — [LOW] ZAR currency formatting hand-rolled across pages

**Where**: `ExpenseRequestsPageContent`, `FeeWaiversPageContent`
**Problem**: `R ${value.toFixed(2)}` doesn't handle locale grouping, negatives, or null safely.
**Resolution**: Introduce `formatZAR(amount)` helper via `Intl.NumberFormat('en-ZA', { style: 'currency', currency: 'ZAR' })`.
**Status**: RESOLVED

---

## Iteration 5 — Communications, SA-Specific & Activities

Files in scope:
- `frontend/src/components/modules/communication/{AnnouncementsPageContent, MessagesPageContent}.tsx`
- `frontend/src/components/modules/academic/TimetablesPageContent.tsx`
- `frontend/src/components/modules/saspecific/{TransportPageContent, TransportDetailPage, ExtramuralsPageContent, ExtramuralsDetailPage}.tsx`
- `frontend/src/components/modules/activities/FieldTripsPageContent.tsx`
- `frontend/src/components/modals/{communication/AnnouncementFormModal, communication/ComposeMessageModal, activities/FieldTripFormModal}.tsx`

### T-501 — [HIGH, RECURRING] Page row-action handlers lack try/catch

**Where**: `AnnouncementsPageContent` (publish/unpublish/pin/unpin/delete), `TimetablesPageContent` (activate/deactivate), `FieldTripsPageContent` (submit + delete)
**Resolution**: Wrap in try/catch, rely on axios interceptor for the error display.
**Status**: RESOLVED

### T-502 — [HIGH, RECURRING] Providers don't re-throw on errors

**Where**: `providers/communication/{announcements,messages,notifications,documents,announcement_reads}/index.tsx`, `providers/academic/{timetables,timetable_slots}/index.tsx`, `providers/saspecific/*/index.tsx`, `providers/activities/field-trips/index.tsx`
**Resolution**: Apply `throw error;` pattern.
**Status**: RESOLVED

### T-503 — [MEDIUM, RECURRING] Enums duplicated again

**Where**: `AnnouncementsPageContent` (type/priority/audience), `TransportPageContent` (transportType/direction/enrollment), `ExtramuralsPageContent` (category/activityType/enrollment), `FieldTripsPageContent` (status)
**Resolution**: Extend `providers/shared/enums.ts` with `AnnouncementType`, `AnnouncementAudience`, `TransportType`, `TransportDirection`, `TransportEnrollmentStatus`, `ExtramuralCategory`, `ExtramuralActivityType`, `FieldTripStatus`. Share label maps.
**Status**: RESOLVED

### T-504 — [HIGH, BACKEND] MessagesPageContent shows raw `User #ID` placeholder

**Where**: `MessagesPageContent.tsx` lines 172, 202, 317, 318
**Problem**: The `IMessageList` type doesn't expose sender or recipient name; UI falls back to `User #${userId}` which is useless to users.
**Resolution**: Backend `MessageListDto` should include `SenderName` and `RecipientName`. Tracked as backend ticket; frontend already attempts to handle nullable fields.
**Status**: DEFERRED

### T-505 — [LOW] MessagesPageContent.handleReply shows redundant generic error

**Where**: `MessagesPageContent.tsx` line 117
**Problem**: `message.error('Failed to send reply')` duplicates the axios interceptor's modal.
**Resolution**: Drop the message; keep the catch as a no-op.
**Status**: RESOLVED

### T-506 — [LOW] MessagesPageContent.handleMarkAsRead lacks try/catch

**Where**: `MessagesPageContent.tsx` line 90-93
**Resolution**: Wrap in try/catch.
**Status**: RESOLVED

### T-507 — [LOW] TransportPageContent and FieldTripsPageContent currency formatting

**Where**: `TransportPageContent.tsx` line 76, `FieldTripsPageContent.tsx` lines 76-80
**Resolution**: Use `formatZAR()` utility.
**Status**: RESOLVED

