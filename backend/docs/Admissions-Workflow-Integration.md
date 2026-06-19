# Admissions ↔ Workflow Integration

How the **Admissions** module and the generic **Workflow** module relate, why they
can feel disconnected, and where they actually touch. (Written for WF-23.)

## TL;DR

They are connected at **two narrow points**, but they keep **separate status
models** and (until WF-23) had **no UI cross-reference**, which is why they feel
like two separate systems:

1. **Fee payment starts the workflow.** When an application fee is paid,
   `ApplicationFeeAppService` calls `TryStartWorkflowAsync(WorkflowEntityType.Application, …)`
   and moves the application to `UnderReview`.
2. **The workflow's terminal decision writes back.** When the "Admissions Approval"
   workflow reaches its terminal step and is approved/rejected,
   `WorkflowEntityBridgeService.HandleApplicationApproved/Rejected` sets the
   application's status to `Approved` / `Rejected`.

Everything in between (document verification, interview, assessment, waitlist,
enrollment) is driven by the **Admissions module's own services**, not by the
workflow.

## The flow

```
Parent submits ──► PaymentPending
        │
        ▼  fee paid  (ApplicationFeeAppService → TryStartWorkflowAsync)
   UnderReview ─────────────► STARTS "Admissions Approval" workflow
        ▲                       Admin Review → Document Verification →
        │                       Interview → Committee Review → Principal Decision
        │                                   │
        │  bridge writes result back        │ terminal Approve / Reject
   Approved / Rejected ◄────────────────────┘
        (WorkflowEntityBridgeService)
```

Only **paid** applications have a workflow. Unpaid (`PaymentPending`) applications
have none.

## Two status models (the source of the "disconnect")

| Layer | Statuses |
| --- | --- |
| **Application** (`ApplicationStatus`) | Draft, Submitted, PaymentPending, UnderReview, DocumentsRequired, InterviewScheduled, AssessmentScheduled, UnderConsideration, Approved, Rejected, Waitlisted, Enrolled, Withdrawn, Expired |
| **Workflow** (`WorkflowStatus`) | NotStarted, InProgress, Completed, Rejected, Cancelled, Recalled |

The workflow only feeds back **Approved/Rejected at the very end**. While the
workflow advances through its steps, the application just sits at `UnderReview` —
the intermediate workflow steps do **not** update the application status. The
intermediate application statuses (`InterviewScheduled`, `AssessmentScheduled`, …)
are set by the separate admissions services (`AdmissionInterviewAppService`,
`AdmissionAssessmentAppService`, `WaitlistAppService`, `EnrollmentAppService`).

## Important finding — the decision path is workflow-only

The admissions application detail page renders manual **Approve / Reject / Waitlist**
buttons (gated on `ApplicationDto.CanMakeDecision`) that POST to
`Application/MakeDecision`. As of this writing:

- **`ApplicationAppService` has no `MakeDecision` (or Approve/Reject/Waitlist) method** —
  so that POST hits a non-existent endpoint and fails.
- `CanMakeDecision` is only `true` when `Status == UnderConsideration`, but the
  workflow bridge moves an application `UnderReview → UnderConsideration → Approved`
  in a single step, so the application never *rests* at `UnderConsideration`. The
  manual buttons therefore essentially never even appear.

**Net:** the **workflow is effectively the only working approve/reject mechanism**
for applications. Either remove the dead manual-decision UI, or implement a real
`MakeDecision` endpoint — tracked as a follow-up.

## What WF-23 added

- **Workflow instance → application:** the workflow instance detail page now shows a
  **"View full application"** link (for `Application`; "View full report" for
  `Report`) to the record's detail page — principal portal only, where those pages
  exist.
- **Application → workflow:** the application detail page now shows an **"Approval
  Workflow"** card (via the existing `GetByEntity` endpoint) with the current step,
  status, and a "View / take action" link to the workflow instance — or a clear
  "not started yet" note when no workflow is running.

## Known gaps / follow-ups

- **Intermediate sync:** workflow steps don't update the application status. Could
  map steps → admissions statuses if tighter tracking is wanted.
- **Dead decision UI:** remove or wire up `Application/MakeDecision`.
- **Admissions list:** no workflow-stage column yet (needs a batched
  "active instance by entity ids" endpoint).
- **Bridge resilience:** `HandleApplicationApproved/Rejected` silently no-op (log a
  warning) if the application drifted out of `UnderReview`/consideration while the
  workflow was in flight.
