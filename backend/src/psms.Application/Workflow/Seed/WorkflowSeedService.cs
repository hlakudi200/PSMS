using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Shared;
using psms.Workflow.WorkflowDefinitions;
using psms.Workflow.WorkflowDefinitions.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Seed;

[AbpAuthorize(PermissionNames.Workflow_Definitions_Create)]
public class WorkflowSeedService : ApplicationService
{
    /// <summary>
    /// Two working days on each report approval step. Report cards move in
    /// batches at the end of a term, so the SLA is what surfaces the one class
    /// still sitting unreviewed while the rest have gone out.
    /// </summary>
    private const int ReportReviewSlaHours = 48;

    /// <summary>Key of the decision form rendered on the principal's step.</summary>
    private const string ReportApprovalDecisionKey = "report.approval";

    /// <summary>
    /// RC-12. The guard on the review step: every subject on the card has a
    /// final mark. Matches ReportSubjectsCompleteGuard.Key.
    /// </summary>
    private const string ReportSubjectsCompleteGuardKey = "report.subjects-complete";

    private readonly IRepository<WorkflowDefinition, Guid> _definitionRepository;
    private readonly IRepository<WorkflowStep, Guid> _stepRepository;
    private readonly IRepository<WorkflowInstance, Guid> _instanceRepository;
    private readonly IRepository<WorkflowTransition, Guid> _transitionRepository;

    public WorkflowSeedService(
        IRepository<WorkflowDefinition, Guid> definitionRepository,
        IRepository<WorkflowStep, Guid> stepRepository,
        IRepository<WorkflowInstance, Guid> instanceRepository,
        IRepository<WorkflowTransition, Guid> transitionRepository)
    {
        _definitionRepository = definitionRepository;
        _stepRepository = stepRepository;
        _instanceRepository = instanceRepository;
        _transitionRepository = transitionRepository;
    }

    /// <summary>
    /// Seeds default workflow definitions for the current tenant.
    /// Skips any entity type that already has a definition.
    /// </summary>
    public async Task SeedDefaultsAsync()
    {
        await SeedAdmissionsWorkflow();
        await SeedReportApprovalWorkflow();
        await SeedFeeWaiverApprovalWorkflow();
        await SeedDisciplinaryWorkflow();
        await SeedStudentTransferWorkflow();
        await SeedStaffLeaveWorkflow();
        await SeedFieldTripWorkflow();
        await SeedExpenseWorkflow();
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    private const string InterviewNotRequiredSkipKey = "application.interview-not-required";
    private const string AssessmentNotRequiredSkipKey = "application.assessment-not-required";
    private const string AssessmentStepName = "Placement Assessment";
    private const string PrincipalDecisionStepName = "Principal Decision";

    /// <summary>
    /// The form the principal fills in to decide an application: accept, accept
    /// with conditions, waitlist, or reject. Matches ApplicationDecisionSchema.Key.
    /// </summary>
    private const string ApplicationDecisionKey = "application.decision";
    private const string InterviewRecordedGuardKey = "application.interview-recorded";
    private const string AssessmentRecordedGuardKey = "application.assessment-recorded";
    private const string DocumentsVerifiedGuardKey = "application.documents-verified";
    private const string AdminReviewStepName = "Admin Review";

    private async Task SeedAdmissionsWorkflow()
    {
        var existing = await _definitionRepository
            .GetAll()
            .Include(d => d.Steps)
            .FirstOrDefaultAsync(d => d.TenantId == AbpSession.TenantId
                && d.EntityType == WorkflowEntityType.Application);

        if (existing != null)
        {
            await UpgradeAdmissionsWorkflow(existing);
            return;
        }

        var definition = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = AbpSession.TenantId,
            Name = "Admissions Approval",
            Description = "Default admissions workflow for SA private schools. Covers the full application lifecycle from submission to enrollment.",
            EntityType = WorkflowEntityType.Application,
            IsActive = true
        };

        await _definitionRepository.InsertAsync(definition);

        var steps = new[]
        {
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 1,
                Name = AdminReviewStepName,
                Description = "Admin reviews the submitted application for completeness.",
                AssignedRole = "Admin",
                ActionType = WorkflowActionType.Review,
                /* Every document the grade asks for has to have been looked at
                   and marked verified before the application goes any further.
                   Nothing could mark one until now, so this would have been a
                   guard nobody could satisfy. */
                GuardKey = DocumentsVerifiedGuardKey
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 2,
                Name = "Interview",
                Description = "Schedule and conduct parent/student interview.",
                // The teacher does the interview, so the step belongs to them.
                AssignedRole = "Teacher",
                ActionType = WorkflowActionType.Review,
                // A school that does not interview for this grade never sees it.
                SkipWhenKey = InterviewNotRequiredSkipKey,
                // And one that normally does can still waive it for a case.
                IsOptional = true,
                /* The step is named after a thing that has to happen. Without
                   this it recorded nothing: whoever held it clicked forward and
                   the principal decided with no interview behind the decision,
                   while the school's own settings said one was required. */
                GuardKey = InterviewRecordedGuardKey
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 3,
                Name = AssessmentStepName,
                Description = "Set and mark the placement assessment for the grade applied for.",
                // Whoever sets the paper marks it; it is a different paper for
                // every grade, which is why this is a per-grade decision.
                AssignedRole = "Teacher",
                ActionType = WorkflowActionType.Review,
                SkipWhenKey = AssessmentNotRequiredSkipKey,
                IsOptional = true,
                GuardKey = AssessmentRecordedGuardKey
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 4,
                Name = PrincipalDecisionStepName,
                Description = "Principal makes the final admission decision.",
                AssignedRole = "Principal",
                ActionType = WorkflowActionType.Approve,
                IsTerminal = true,
                // Sending it back means another look at the review, which is
                // where the committee step used to point before it was removed.
                NextStepOnReject = 1,
                // Without this the step has no decision to make: every completion
                // fell through to Accepted, so nobody could waitlist or reject.
                DecisionSchemaKey = ApplicationDecisionKey
            }
        };

        foreach (var step in steps)
        {
            await _stepRepository.InsertAsync(step);
        }
    }

    /// <summary>
    /// WF-34: brings a tenant seeded before this onto the current Interview step.
    /// <para>
    /// Two changes, both in place so instances already running keep their step
    /// order: the step moves from Admin to the Teacher who actually conducts
    /// the interview, and it gains the rule that steps over it for a grade the
    /// school does not interview for. Only touched where it still holds the old
    /// values — a school that has since configured its own is left alone.
    /// </para>
    /// </summary>
    /// <summary>
    /// Brings a tenant seeded earlier onto the current shape. Each pass stands
    /// on its own and leaves alone anything the school has made its own.
    /// </summary>
    private async Task UpgradeAdmissionsWorkflow(WorkflowDefinition definition)
    {
        await GiveTheInterviewToTeachersAsync(definition);
        await AddAssessmentStepAsync(definition);
        await GiveThePrincipalADecisionToMakeAsync(definition);
        await TheLastStepCanFinishAsync(definition);
        await RemoveTheStepsThatDecideNothingAsync(definition);
        await MakeTheInterviewAndAssessmentMeanSomethingAsync(definition);
    }

    /// <summary>
    /// The step that ends the workflow is marked as ending it.
    /// <para>
    /// A live school was found with its Principal Decision step carrying
    /// <c>IsTerminal = false</c>. Completion still worked — the engine finishes
    /// when a step has nothing after it — so nothing looked wrong. What it
    /// quietly broke was every pass that looks for the terminal step:
    /// <see cref="GiveThePrincipalADecisionToMakeAsync"/> searches by it, and
    /// <see cref="RemoveTheStepsThatDecideNothingAsync"/> refuses to remove
    /// anything unless a terminal step would survive. So the tidy-up declined,
    /// silently, and the school kept a Document Verification step the seeder
    /// had been trying to retire for three releases.
    /// </para>
    /// <para>
    /// Only the last step, and only when it is the decision step: a school that
    /// has deliberately added stages after the principal is not being corrected
    /// here.
    /// </para>
    /// </summary>
    private async Task TheLastStepCanFinishAsync(WorkflowDefinition definition)
    {
        await CurrentUnitOfWork.SaveChangesAsync();

        var ordered = await _stepRepository.GetAll()
            .Where(s => s.WorkflowDefinitionId == definition.Id)
            .OrderBy(s => s.StepOrder)
            .ToListAsync();

        var last = ordered.LastOrDefault();
        if (last == null || last.IsTerminal || last.Name != PrincipalDecisionStepName) return;

        last.IsTerminal = true;
        await _stepRepository.UpdateAsync(last);
        await CurrentUnitOfWork.SaveChangesAsync();

        Logger.Info($"Marked '{last.Name}' as the terminal step of definition {definition.Id}.");
    }

    /// <summary>
    /// The two steps named after something that has to happen now wait for it
    /// to have happened.
    /// <para>
    /// Both were plain review steps: the actor was offered an action and a
    /// comment, clicked forward, and nothing was recorded. Walking a Grade 5
    /// application — a grade whose own settings require both — through to a
    /// decision left <c>AdmissionInterview</c> and <c>AdmissionAssessment</c>
    /// empty, and the principal decided on nothing.
    /// </para>
    /// <para>
    /// The guards for exactly this were written, registered and attached to no
    /// step. Attaching them is the whole fix. A grade that requires neither
    /// still never sees the steps — the skip rules run first — and a school
    /// with a reason can still waive, which is recorded with the reason.
    /// </para>
    /// <para>
    /// Runs last and requeries, because the pass before this one renumbers and
    /// deletes: a stale navigation property here would write the guard onto a
    /// step that no longer exists.
    /// </para>
    /// </summary>
    private async Task MakeTheInterviewAndAssessmentMeanSomethingAsync(WorkflowDefinition definition)
    {
        await CurrentUnitOfWork.SaveChangesAsync();

        var steps = await _stepRepository.GetAll()
            .Where(s => s.WorkflowDefinitionId == definition.Id)
            .ToListAsync();

        var changed = false;

        foreach (var (name, guardKey) in new[]
                 {
                     ("Interview", InterviewRecordedGuardKey),
                     (AssessmentStepName, AssessmentRecordedGuardKey),
                     (AdminReviewStepName, DocumentsVerifiedGuardKey),
                 })
        {
            var step = steps.FirstOrDefault(s => s.Name == name);

            // Only where the school has not chosen its own: a guard somebody
            // picked deliberately is theirs, not ours to overwrite.
            if (step == null || !string.IsNullOrWhiteSpace(step.GuardKey)) continue;

            step.GuardKey = guardKey;
            await _stepRepository.UpdateAsync(step);
            changed = true;
        }

        if (changed) await CurrentUnitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// The interview belongs to the teacher who conducts it, and steps itself
    /// over for a grade that does not interview.
    /// <para>
    /// This used to insist the step was third, which it was when the pass was
    /// written. Removing the two steps that decided nothing moved it to second
    /// — and because the whole upgrade was gated on finding it there, every
    /// later pass stopped running the moment that happened. A pass matches on
    /// what a step <i>is</i>, never on where it sits.
    /// </para>
    /// </summary>
    private async Task GiveTheInterviewToTeachersAsync(WorkflowDefinition definition)
    {
        var interview = definition.Steps?.FirstOrDefault(s => s.Name == "Interview");
        if (interview == null) return;

        var changed = false;

        if (string.IsNullOrWhiteSpace(interview.SkipWhenKey))
        {
            interview.SkipWhenKey = InterviewNotRequiredSkipKey;
            interview.IsOptional = true;
            changed = true;
        }

        if (interview.AssignedRole == "Admin")
        {
            interview.AssignedRole = "Teacher";
            changed = true;
        }

        if (changed)
            await _stepRepository.UpdateAsync(interview);
    }

    /// <summary>
    /// Steps a school was seeded with that never decided anything.
    /// <para>
    /// <b>Document Verification</b> was a step assigned to Admin that verified
    /// nothing: the guard written for it was attached to no step, and there was
    /// no way to mark a document verified in the first place. It belongs as a
    /// condition on the review it was sitting next to, not as a stage of its
    /// own.
    /// </para>
    /// <para>
    /// <b>Committee Review</b> named a committee this system cannot express —
    /// a step has one role and, at most, one fixed person, so there is no panel
    /// and no quorum. It sat between two other Admin steps and the deliberation
    /// it claimed already happens at the principal's decision.
    /// </para>
    /// <para>
    /// Only ever removes a step still holding its seeded name, role and action,
    /// so a school that has made either step its own keeps it.
    /// </para>
    /// </summary>
    private async Task RemoveTheStepsThatDecideNothingAsync(WorkflowDefinition definition)
    {
        /* The passes before this one insert a step and renumber others, and the
           definition's own Steps collection was loaded before any of that — so
           it can be a step short. Flush, then ask the database what is actually
           there: renumbering against a stale list collides two steps onto one
           number and the save fails with nothing useful to say. */
        await CurrentUnitOfWork.SaveChangesAsync();

        var ordered = await _stepRepository
            .GetAll()
            .Where(s => s.WorkflowDefinitionId == definition.Id)
            .OrderBy(s => s.StepOrder)
            .ToListAsync();

        if (ordered.Count == 0) return;

        var doomed = ordered.Where(StillTheSeededPlaceholder).ToList();
        if (doomed.Count == 0) return;

        var surviving = ordered.Except(doomed).ToList();
        if (surviving.Count == 0 || !surviving.Any(s => s.IsTerminal))
        {
            // Never leave a definition with nothing, or with no way to finish.
            Logger.Warn($"Refusing to remove steps from definition {definition.Id}: nothing usable would remain.");
            return;
        }

        /* Capture the old numbering before touching anything — routing targets
           and instance positions are both recorded as step *numbers*, so every
           one of them has to be translated through this map. */
        var oldOrderOf = ordered.ToDictionary(s => s.Id, s => s.StepOrder);
        var newOrderOf = surviving
            .Select((s, i) => (s.Id, Order: i + 1))
            .ToDictionary(x => x.Id, x => x.Order);

        // Where each old number now points. A removed step forwards to the next
        // step that survived it, which is also where anything sitting on it goes.
        var landsOn = new Dictionary<int, WorkflowStep>();
        foreach (var step in ordered)
        {
            var destination = newOrderOf.ContainsKey(step.Id)
                ? step
                : surviving.FirstOrDefault(s => oldOrderOf[s.Id] > oldOrderOf[step.Id]) ?? surviving.Last();
            landsOn[oldOrderOf[step.Id]] = destination;
        }

        /* Applications caught on a step that is about to disappear. They move
           forward to the next surviving step rather than back: the work of the
           earlier stages has been done, and making a family wait while a school
           redoes it would be the unkinder reading of a tidy-up they did not
           ask for. Each move leaves a line in the history saying why. */
        var stranded = await _instanceRepository
            .GetAll()
            .Where(i => i.WorkflowDefinitionId == definition.Id
                        && i.CurrentStepId.HasValue
                        && (i.Status == WorkflowStatus.NotStarted || i.Status == WorkflowStatus.InProgress))
            .ToListAsync();

        foreach (var instance in stranded)
        {
            var wasOn = ordered.FirstOrDefault(s => s.Id == instance.CurrentStepId.Value);
            if (wasOn == null) continue;

            var destination = landsOn[oldOrderOf[wasOn.Id]];

            if (destination.Id != wasOn.Id)
            {
                await _transitionRepository.InsertAsync(new WorkflowTransition
                {
                    Id = Guid.NewGuid(),
                    TenantId = definition.TenantId,
                    WorkflowInstanceId = instance.Id,
                    FromStepId = wasOn.Id,
                    ToStepId = destination.Id,
                    Action = WorkflowActionType.Waive,
                    ActorUserId = AbpSession.UserId ?? 0,
                    ActorUserName = "System",
                    Comment = $"'{wasOn.Name}' was removed from this workflow — it decided nothing. Moved on to '{destination.Name}'.",
                    TransitionDate = DateTime.UtcNow,
                    IsWaived = true,
                });
            }

            instance.CurrentStepId = destination.Id;
            instance.CurrentStepOrder = newOrderOf[destination.Id];
            await _instanceRepository.UpdateAsync(instance);
        }

        // Renumber what is left, translating every routing target with it.
        foreach (var step in surviving)
        {
            step.StepOrder = newOrderOf[step.Id];

            if (step.NextStepOnApprove.HasValue && landsOn.TryGetValue(step.NextStepOnApprove.Value, out var onApprove))
                step.NextStepOnApprove = newOrderOf[onApprove.Id];

            /* A reject target that survived keeps pointing at it. One that was
               removed goes back to the first step rather than forward with the
               others: "send this back" means another look, and forwarding it
               would have aimed the principal's send-back at the terminal step
               below them — completing the workflow instead of returning it. */
            if (step.NextStepOnReject.HasValue)
            {
                var target = ordered.FirstOrDefault(s => oldOrderOf[s.Id] == step.NextStepOnReject.Value);
                step.NextStepOnReject = target != null && newOrderOf.TryGetValue(target.Id, out var kept)
                    ? kept
                    : 1;
            }

            await _stepRepository.UpdateAsync(step);
        }

        foreach (var step in doomed)
            await _stepRepository.DeleteAsync(step);
    }

    /// <summary>
    /// Whether this is one of the two seeded steps being retired, untouched —
    /// same name, same role, same action as it was seeded with. A school that
    /// has renamed, reassigned or repurposed either one has made it theirs.
    /// </summary>
    private static bool StillTheSeededPlaceholder(WorkflowStep step) =>
        step.ActionType == WorkflowActionType.Review
        && step.AssignedRole == "Admin"
        && !step.IsTerminal
        && (step.Name == "Document Verification" || step.Name == "Committee Review");

    /// <summary>
    /// Attaches the admission decision form to the principal's step.
    /// <para>
    /// The form was written — accept, accept with conditions, place on
    /// waitlist, reject, with the reason and offer-validity rules ADM-020 and
    /// ADM-018 are about — registered with the engine, and attached to nothing.
    /// So the step had no decision on it, and completing it read:
    /// </para>
    /// <code>
    /// var decision = ctx.Decision.GetEnum&lt;AdmissionDecision&gt;("decision")
    ///               ?? AdmissionDecision.Accepted;
    /// </code>
    /// <para>
    /// Every application the principal passed was therefore accepted, whatever
    /// they meant. Nobody could be waitlisted — which is why the waiting list
    /// was always empty — nobody could be rejected with the reason the rule
    /// demands, and an offer could not be given a life of its own.
    /// </para>
    /// <para>
    /// Only attached where the step still carries none, so a school that has
    /// configured its own form keeps it.
    /// </para>
    /// </summary>
    private async Task GiveThePrincipalADecisionToMakeAsync(WorkflowDefinition definition)
    {
        var decision = definition.Steps?
            .FirstOrDefault(s => s.IsTerminal && s.Name == PrincipalDecisionStepName);

        if (decision == null || !string.IsNullOrWhiteSpace(decision.DecisionSchemaKey))
            return;

        decision.DecisionSchemaKey = ApplicationDecisionKey;
        await _stepRepository.UpdateAsync(decision);
    }

    /// <summary>
    /// Adds the placement assessment to a workflow seeded before it existed.
    /// <para>
    /// A school decides per grade whether applicants sit one, and that answer
    /// had nowhere to land: the workflow went straight from the interview to
    /// the committee, so <c>IsAssessmentRequired</c> and the rule that reads it
    /// governed a step that was never there.
    /// </para>
    /// <para>
    /// It belongs between the interview and the committee, which means the two
    /// steps after it move down one. Applications already part-way through are
    /// the reason this is done carefully rather than by re-seeding: an instance
    /// tracks the step it is on by id, so renumbering does not move it, but its
    /// recorded step number would be left describing the wrong position and the
    /// decision step's "send back" target would point at whatever took its old
    /// number. Both are corrected here.
    /// </para>
    /// </summary>
    private async Task AddAssessmentStepAsync(WorkflowDefinition definition)
    {
        var steps = definition.Steps?.OrderBy(s => s.StepOrder).ToList() ?? new List<WorkflowStep>();

        if (steps.Count == 0 || steps.Any(s => s.Name == AssessmentStepName))
            return;

        var interview = steps.FirstOrDefault(s => s.Name == "Interview");
        if (interview == null) return;

        var insertAt = interview.StepOrder + 1;

        // Everything from the insertion point down shifts by one. Descending,
        // so no two steps momentarily share an order.
        foreach (var step in steps.Where(s => s.StepOrder >= insertAt).OrderByDescending(s => s.StepOrder))
        {
            step.StepOrder += 1;

            if (step.NextStepOnApprove >= insertAt) step.NextStepOnApprove += 1;
            if (step.NextStepOnReject >= insertAt) step.NextStepOnReject += 1;

            await _stepRepository.UpdateAsync(step);
        }

        // A step before the insertion point can still route forward past it.
        foreach (var step in steps.Where(s => s.StepOrder < insertAt))
        {
            var touched = false;
            if (step.NextStepOnApprove >= insertAt) { step.NextStepOnApprove += 1; touched = true; }
            if (step.NextStepOnReject >= insertAt) { step.NextStepOnReject += 1; touched = true; }
            if (touched) await _stepRepository.UpdateAsync(step);
        }

        await _stepRepository.InsertAsync(new WorkflowStep
        {
            Id = Guid.NewGuid(),
            WorkflowDefinitionId = definition.Id,
            StepOrder = insertAt,
            Name = AssessmentStepName,
            Description = "Set and mark the placement assessment for the grade applied for.",
            AssignedRole = "Teacher",
            ActionType = WorkflowActionType.Review,
            SkipWhenKey = AssessmentNotRequiredSkipKey,
            IsOptional = true,
        });

        // The instances in flight: their step is unchanged, its number is not.
        var running = await _instanceRepository
            .GetAll()
            .Where(i => i.WorkflowDefinitionId == definition.Id
                        && i.CurrentStepOrder >= insertAt
                        && (i.Status == WorkflowStatus.NotStarted || i.Status == WorkflowStatus.InProgress))
            .ToListAsync();

        foreach (var instance in running)
        {
            instance.CurrentStepOrder += 1;
            await _instanceRepository.UpdateAsync(instance);
        }
    }

    /// <summary>
    /// RC-09. The report approval workflow, and an in-place upgrade for tenants
    /// that were seeded before its steps carried an SLA and a decision form.
    /// </summary>
    private async Task SeedReportApprovalWorkflow()
    {
        var existing = await _definitionRepository
            .GetAll()
            .FirstOrDefaultAsync(d => d.TenantId == AbpSession.TenantId
                && d.EntityType == WorkflowEntityType.Report);

        if (existing != null)
        {
            await UpgradeReportApprovalStepsAsync(existing.Id);
            return;
        }

        var definition = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = AbpSession.TenantId,
            Name = "Report Approval",
            Description = "Report card approval workflow. HOD reviews then principal approves before publishing to parents.",
            EntityType = WorkflowEntityType.Report,
            IsActive = true
        };

        await _definitionRepository.InsertAsync(definition);

        var steps = new[]
        {
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 1,
                Name = "HOD Review",
                Description = "Head of Department reviews the report card for accuracy.",
                // Was "Teacher", which contradicted the step's own name and sent
                // report cards to a role holding neither Generate nor Publish.
                AssignedRole = "HOD",
                ActionType = WorkflowActionType.Review,
                IsCommentRequired = true,
                SlaHours = ReportReviewSlaHours,
                // RC-12: a report card whose subjects have no marks must not
                // reach a reviewer. Left off until there was a way to capture
                // the marks; there is one now.
                GuardKey = ReportSubjectsCompleteGuardKey
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 2,
                Name = "Principal Approval",
                Description = "Principal approves the report card for publishing.",
                AssignedRole = "Principal",
                ActionType = WorkflowActionType.Approve,
                IsTerminal = true,
                NextStepOnReject = 1, // Send back to HOD
                // ReportWorkflowHandler reads "principalComment" off the decision
                // and writes it to the report. Without the schema attached the
                // form never rendered the field, so the handler was reading a
                // value the UI had no way to collect.
                DecisionSchemaKey = ReportApprovalDecisionKey,
                SlaHours = ReportReviewSlaHours
            }
        };

        foreach (var step in steps)
        {
            await _stepRepository.InsertAsync(step);
        }
    }

    /// <summary>
    /// Brings an already-seeded Report definition up to date. The seeder is
    /// invoked per tenant and previously bailed out whenever a definition
    /// existed, so a school seeded earlier would never pick up the decision form
    /// or the corrected reviewer role.
    /// <para>
    /// Only fills in what is missing: a school that has deliberately retuned its
    /// own steps keeps its changes.
    /// </para>
    /// </summary>
    private async Task UpgradeReportApprovalStepsAsync(Guid definitionId)
    {
        var steps = await _stepRepository
            .GetAll()
            .Where(s => s.WorkflowDefinitionId == definitionId)
            .ToListAsync();

        foreach (var step in steps)
        {
            var changed = false;

            if (step.StepOrder == 1 && step.AssignedRole == "Teacher")
            {
                step.AssignedRole = "HOD";
                changed = true;
            }

            if (step.ActionType == WorkflowActionType.Approve
                && string.IsNullOrWhiteSpace(step.DecisionSchemaKey))
            {
                step.DecisionSchemaKey = ReportApprovalDecisionKey;
                changed = true;
            }

            if (!step.SlaHours.HasValue)
            {
                step.SlaHours = ReportReviewSlaHours;
                changed = true;
            }

            // RC-12: schools seeded before the capture screen existed have a
            // review step with no guard on it.
            if (step.StepOrder == 1 && string.IsNullOrWhiteSpace(step.GuardKey))
            {
                step.GuardKey = ReportSubjectsCompleteGuardKey;
                changed = true;
            }

            if (changed)
            {
                await _stepRepository.UpdateAsync(step);
            }
        }
    }

    private async Task SeedFeeWaiverApprovalWorkflow()
    {
        var exists = await _definitionRepository
            .GetAll()
            .AnyAsync(d => d.TenantId == AbpSession.TenantId
                && d.EntityType == WorkflowEntityType.FeeWaiver);

        if (exists) return;

        var definition = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = AbpSession.TenantId,
            Name = "Fee Waiver Approval",
            Description = "Fee waiver approval workflow. Finance reviews then principal approves the discount.",
            EntityType = WorkflowEntityType.FeeWaiver,
            IsActive = true
        };

        await _definitionRepository.InsertAsync(definition);

        var steps = new[]
        {
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 1,
                Name = "Finance Review",
                Description = "Finance department reviews the fee waiver request and supporting documents.",
                AssignedRole = "Admin",
                ActionType = WorkflowActionType.Review
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 2,
                Name = "Principal Approval",
                Description = "Principal approves or rejects the fee waiver.",
                AssignedRole = "Principal",
                ActionType = WorkflowActionType.Approve,
                IsTerminal = true
            }
        };

        foreach (var step in steps)
        {
            await _stepRepository.InsertAsync(step);
        }
    }

    private async Task SeedDisciplinaryWorkflow()
    {
        if (await _definitionRepository.GetAll().AnyAsync(d => d.TenantId == AbpSession.TenantId && d.EntityType == WorkflowEntityType.Disciplinary))
            return;

        var def = new WorkflowDefinition { Id = Guid.NewGuid(), TenantId = AbpSession.TenantId, Name = "Disciplinary Process", Description = "Disciplinary case workflow. Teacher reports → HOD investigates → Committee hearing → Principal decision.", EntityType = WorkflowEntityType.Disciplinary, IsActive = true };
        await _definitionRepository.InsertAsync(def);

        var steps = new[]
        {
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowDefinitionId = def.Id, StepOrder = 1, Name = "Teacher Report", Description = "Teacher reports the incident with details.", AssignedRole = "Teacher", ActionType = WorkflowActionType.Submit, IsCommentRequired = true },
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowDefinitionId = def.Id, StepOrder = 2, Name = "HOD Investigation", Description = "HOD investigates the incident, gathers evidence.", AssignedRole = "HOD", ActionType = WorkflowActionType.Review, IsCommentRequired = true },
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowDefinitionId = def.Id, StepOrder = 3, Name = "Disciplinary Hearing", Description = "Disciplinary committee conducts formal hearing.", AssignedRole = "Admin", ActionType = WorkflowActionType.Review, IsCommentRequired = true },
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowDefinitionId = def.Id, StepOrder = 4, Name = "Principal Decision", Description = "Principal makes final decision on sanction.", AssignedRole = "Principal", ActionType = WorkflowActionType.Approve, IsTerminal = true, NextStepOnReject = 3, IsCommentRequired = true }
        };
        foreach (var s in steps) await _stepRepository.InsertAsync(s);
    }

    private async Task SeedStudentTransferWorkflow()
    {
        if (await _definitionRepository.GetAll().AnyAsync(d => d.TenantId == AbpSession.TenantId && d.EntityType == WorkflowEntityType.StudentTransfer))
            return;

        var def = new WorkflowDefinition { Id = Guid.NewGuid(), TenantId = AbpSession.TenantId, Name = "Student Transfer Approval", Description = "Student transfer request workflow. Admin reviews → Principal approves.", EntityType = WorkflowEntityType.StudentTransfer, IsActive = true };
        await _definitionRepository.InsertAsync(def);

        var steps = new[]
        {
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowDefinitionId = def.Id, StepOrder = 1, Name = "Admin Review", Description = "Admin reviews transfer request and verifies documentation.", AssignedRole = "Admin", ActionType = WorkflowActionType.Review },
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowDefinitionId = def.Id, StepOrder = 2, Name = "Principal Approval", Description = "Principal approves or rejects the transfer.", AssignedRole = "Principal", ActionType = WorkflowActionType.Approve, IsTerminal = true, NextStepOnReject = 1 }
        };
        foreach (var s in steps) await _stepRepository.InsertAsync(s);
    }

    private async Task SeedStaffLeaveWorkflow()
    {
        if (await _definitionRepository.GetAll().AnyAsync(d => d.TenantId == AbpSession.TenantId && d.EntityType == WorkflowEntityType.StaffLeave))
            return;

        var def = new WorkflowDefinition { Id = Guid.NewGuid(), TenantId = AbpSession.TenantId, Name = "Staff Leave Approval", Description = "Staff leave request workflow. HOD reviews → Principal approves.", EntityType = WorkflowEntityType.StaffLeave, IsActive = true };
        await _definitionRepository.InsertAsync(def);

        var steps = new[]
        {
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowDefinitionId = def.Id, StepOrder = 1, Name = "HOD Review", Description = "Head of Department reviews leave request and substitute arrangements.", AssignedRole = "HOD", ActionType = WorkflowActionType.Review },
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowDefinitionId = def.Id, StepOrder = 2, Name = "Principal Approval", Description = "Principal gives final approval for leave.", AssignedRole = "Principal", ActionType = WorkflowActionType.Approve, IsTerminal = true, NextStepOnReject = 1 }
        };
        foreach (var s in steps) await _stepRepository.InsertAsync(s);
    }

    private async Task SeedFieldTripWorkflow()
    {
        if (await _definitionRepository.GetAll().AnyAsync(d => d.TenantId == AbpSession.TenantId && d.EntityType == WorkflowEntityType.FieldTrip))
            return;

        var def = new WorkflowDefinition { Id = Guid.NewGuid(), TenantId = AbpSession.TenantId, Name = "Field Trip Approval", Description = "Field trip approval workflow. HOD reviews → Finance checks budget → Principal approves.", EntityType = WorkflowEntityType.FieldTrip, IsActive = true };
        await _definitionRepository.InsertAsync(def);

        var steps = new[]
        {
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowDefinitionId = def.Id, StepOrder = 1, Name = "HOD Review", Description = "HOD reviews educational value and logistics.", AssignedRole = "HOD", ActionType = WorkflowActionType.Review },
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowDefinitionId = def.Id, StepOrder = 2, Name = "Finance Review", Description = "Finance checks budget availability.", AssignedRole = "Finance", ActionType = WorkflowActionType.Review },
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowDefinitionId = def.Id, StepOrder = 3, Name = "Principal Approval", Description = "Principal gives final approval.", AssignedRole = "Principal", ActionType = WorkflowActionType.Approve, IsTerminal = true, NextStepOnReject = 1 }
        };
        foreach (var s in steps) await _stepRepository.InsertAsync(s);
    }

    private async Task SeedExpenseWorkflow()
    {
        if (await _definitionRepository.GetAll().AnyAsync(d => d.TenantId == AbpSession.TenantId && d.EntityType == WorkflowEntityType.ExpenseRequest))
            return;

        var def = new WorkflowDefinition { Id = Guid.NewGuid(), TenantId = AbpSession.TenantId, Name = "Expense Approval", Description = "Expense request workflow. Finance reviews → Principal approves.", EntityType = WorkflowEntityType.ExpenseRequest, IsActive = true };
        await _definitionRepository.InsertAsync(def);

        var steps = new[]
        {
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowDefinitionId = def.Id, StepOrder = 1, Name = "Finance Review", Description = "Finance department reviews expense and quotation.", AssignedRole = "Finance", ActionType = WorkflowActionType.Review },
            new WorkflowStep { Id = Guid.NewGuid(), WorkflowDefinitionId = def.Id, StepOrder = 2, Name = "Principal Approval", Description = "Principal approves the expense.", AssignedRole = "Principal", ActionType = WorkflowActionType.Approve, IsTerminal = true, NextStepOnReject = 1 }
        };
        foreach (var s in steps) await _stepRepository.InsertAsync(s);
    }
}
