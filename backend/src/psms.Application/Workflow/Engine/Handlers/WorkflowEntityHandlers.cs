using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Academic.Entities;
using psms.Domain.Activities.Entities;
using psms.Domain.Admissions.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Discipline.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.HR.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Engine.Decisions;
using psms.Workflow.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Engine.Handlers;

/// <summary>
/// Shared plumbing for the terminal write-back. Loads the aggregate, applies the
/// domain call, saves. A domain-rule violation is surfaced as a
/// UserFriendlyException so the workflow transition rolls back — the workflow
/// must never show Completed while the record was untouched.
/// </summary>
public abstract class EntityHandlerBase<TEntity> : IWorkflowEntityHandler
    where TEntity : class, Abp.Domain.Entities.IEntity<Guid>
{
    protected readonly IRepository<TEntity, Guid> Repository;
    protected EntityHandlerBase(IRepository<TEntity, Guid> repository) { Repository = repository; }

    public abstract WorkflowEntityType EntityType { get; }

    protected abstract void Completed(TEntity entity, WorkflowEffectContext ctx);
    protected abstract void Rejected(TEntity entity, WorkflowEffectContext ctx);
    /// <summary>Default: the record returns to the requester as a draft.</summary>
    protected abstract void Reopened(TEntity entity, WorkflowEffectContext ctx);

    public Task OnCompletedAsync(WorkflowEffectContext ctx) => RunAsync(ctx, Completed, "approve");
    public Task OnRejectedAsync(WorkflowEffectContext ctx) => RunAsync(ctx, Rejected, "reject");
    public Task OnCancelledAsync(WorkflowEffectContext ctx) => RunAsync(ctx, Reopened, "reopen");
    public Task OnRecalledAsync(WorkflowEffectContext ctx) => RunAsync(ctx, Reopened, "reopen");

    private async Task RunAsync(WorkflowEffectContext ctx, Action<TEntity, WorkflowEffectContext> action, string verb)
    {
        var entity = await Repository.GetAll().FirstOrDefaultAsync(e => e.Id == ctx.EntityId);
        if (entity == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.EntityWriteBackFailed,
                $"The linked {typeof(TEntity).Name} no longer exists, so the workflow cannot {verb} it.");
        try
        {
            action(entity, ctx);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(WorkflowExceptionCodes.EntityWriteBackFailed,
                $"Cannot {verb} the {typeof(TEntity).Name}: {ex.Message}");
        }
        await Repository.UpdateAsync(entity);
    }

    protected static string ReasonOr(WorkflowEffectContext ctx, string fallback) =>
        string.IsNullOrWhiteSpace(ctx.Comment) ? fallback : ctx.Comment.Trim();
}

public class ApplicationWorkflowHandler : EntityHandlerBase<Application>, ITransientDependency
{
    private readonly IRepository<Waitlist, Guid> _waitlists;

    public ApplicationWorkflowHandler(IRepository<Application, Guid> r, IRepository<Waitlist, Guid> waitlists) : base(r)
    {
        _waitlists = waitlists;
    }

    public override WorkflowEntityType EntityType => WorkflowEntityType.Application;

    private static void EnsureUnderConsideration(Application a)
    {
        if (a.Status == ApplicationStatus.UnderConsideration) return;
        if (a.Status == ApplicationStatus.InterviewScheduled || a.Status == ApplicationStatus.AssessmentScheduled)
            a.RevertToUnderReview();
        if (a.Status == ApplicationStatus.DocumentsRequired)
            a.MarkDocumentsComplete();
        a.MoveToConsideration();
    }

    protected override void Completed(Application a, WorkflowEffectContext ctx)
    {
        EnsureUnderConsideration(a);
        var decision = ctx.Decision.GetEnum<AdmissionDecision>("decision") ?? AdmissionDecision.Accepted;
        var reason = ctx.Decision.GetString("reason") ?? ctx.Comment;
        var days = ctx.Decision.GetInt("offerExpiryDays") ?? 14;

        switch (decision)
        {
            case AdmissionDecision.ConditionalAcceptance:
                a.ApproveWithConditions(ctx.ActorUserId, reason, days);
                break;
            case AdmissionDecision.Rejected:
                a.Reject(ctx.ActorUserId, reason);
                break;
            case AdmissionDecision.Waitlisted:
                a.PlaceOnWaitlist(ctx.ActorUserId, reason);
                // Position is FIFO among active entries for the applied grade (ADM-021).
                var position = _waitlists.GetAll().Count(w =>
                    w.TenantId == ctx.TenantId && w.GradeId == a.AppliedGradeId && w.Status == WaitlistStatus.Active) + 1;
                _waitlists.Insert(new Waitlist(Guid.NewGuid(), ctx.TenantId, a.Id, a.AppliedGradeId, position) { Notes = reason });
                break;
            default:
                a.Approve(ctx.ActorUserId, reason, days);
                break;
        }
    }

    protected override void Rejected(Application a, WorkflowEffectContext ctx)
    {
        EnsureUnderConsideration(a);
        var reason = ReasonOr(ctx, null);
        if (string.IsNullOrWhiteSpace(reason) || reason.Length < ApplicationDecisionSchema.MinRejectionReasonLength)
            throw new UserFriendlyException(WorkflowExceptionCodes.DecisionInvalid,
                $"A rejection reason of at least {ApplicationDecisionSchema.MinRejectionReasonLength} characters is required (ADM-020).");
        a.Reject(ctx.ActorUserId, reason);
    }

    protected override void Reopened(Application a, WorkflowEffectContext ctx)
    {
        // An application stays with the school; a cancelled approval simply returns
        // it to the review queue.
        if (a.Status == ApplicationStatus.UnderConsideration
            || a.Status == ApplicationStatus.InterviewScheduled
            || a.Status == ApplicationStatus.AssessmentScheduled)
            a.RevertToUnderReview();
    }
}

public class ReportWorkflowHandler : EntityHandlerBase<Report>, ITransientDependency
{
    public ReportWorkflowHandler(IRepository<Report, Guid> r) : base(r) { }
    public override WorkflowEntityType EntityType => WorkflowEntityType.Report;

    protected override void Completed(Report r, WorkflowEffectContext ctx)
    {
        var comment = ctx.Decision.GetString("principalComment");
        if (comment != null) r.PrincipalComment = comment.Length > Report.MaxPrincipalCommentLength ? comment.Substring(0, Report.MaxPrincipalCommentLength) : comment;
        r.Approve(ctx.ActorUserId);
    }

    protected override void Rejected(Report r, WorkflowEffectContext ctx) =>
        r.ReturnForRevision(ReasonOr(ctx, "Returned by the approval workflow."));

    protected override void Reopened(Report r, WorkflowEffectContext ctx)
    {
        if (r.Status == ReportStatus.PendingApproval) r.ReturnForRevision("Approval workflow cancelled.");
    }
}

public class FeeWaiverWorkflowHandler : EntityHandlerBase<FeeWaiver>, ITransientDependency
{
    public FeeWaiverWorkflowHandler(IRepository<FeeWaiver, Guid> r) : base(r) { }
    public override WorkflowEntityType EntityType => WorkflowEntityType.FeeWaiver;

    protected override void Completed(FeeWaiver w, WorkflowEffectContext ctx)
    {
        var amount = ctx.Decision.GetDecimal("approvedAmount") ?? w.RequestedAmount;
        if (amount > w.RequestedAmount)
            throw new UserFriendlyException(WorkflowExceptionCodes.DecisionInvalid, "Approved amount cannot exceed the requested amount.");
        w.Approve(ctx.ActorUserId, amount, ctx.Decision.GetString("notes") ?? ReasonOr(ctx, "Approved via workflow."));
    }

    protected override void Rejected(FeeWaiver w, WorkflowEffectContext ctx) => w.Reject(ctx.ActorUserId, ReasonOr(ctx, "Rejected via workflow."));
    protected override void Reopened(FeeWaiver w, WorkflowEffectContext ctx) => w.ReopenAsDraft();
}

public class DisciplinaryWorkflowHandler : EntityHandlerBase<DisciplinaryCase>, ITransientDependency
{
    public DisciplinaryWorkflowHandler(IRepository<DisciplinaryCase, Guid> r) : base(r) { }
    public override WorkflowEntityType EntityType => WorkflowEntityType.Disciplinary;

    protected override void Completed(DisciplinaryCase c, WorkflowEffectContext ctx)
    {
        if (c.Status != DisciplinaryStatus.HearingCompleted)
            throw new UserFriendlyException(WorkflowExceptionCodes.EntityWriteBackFailed,
                "Record the hearing outcome before the case can be resolved.");
        c.Resolve(ctx.ActorUserId);
        if (ctx.Decision.GetBool("parentNotified") == true) c.NotifyParent();
    }

    // WF-50 will redefine rejection as "dismiss" (outcome Dismissed + Resolve). Until
    // then the seeded definition routes Reject back a step, so this rarely fires.
    protected override void Rejected(DisciplinaryCase c, WorkflowEffectContext ctx) => c.Cancel();
    protected override void Reopened(DisciplinaryCase c, WorkflowEffectContext ctx)
    {
        if (c.Status == DisciplinaryStatus.Reported || c.Status == DisciplinaryStatus.UnderInvestigation) c.ReopenAsDraft();
    }
}

public class StudentTransferWorkflowHandler : EntityHandlerBase<StudentTransferRequest>, ITransientDependency
{
    public StudentTransferWorkflowHandler(IRepository<StudentTransferRequest, Guid> r) : base(r) { }
    public override WorkflowEntityType EntityType => WorkflowEntityType.StudentTransfer;
    protected override void Completed(StudentTransferRequest t, WorkflowEffectContext ctx) => t.Approve(ctx.ActorUserId);
    protected override void Rejected(StudentTransferRequest t, WorkflowEffectContext ctx) => t.Reject(ctx.ActorUserId, ReasonOr(ctx, "Rejected via workflow."));
    protected override void Reopened(StudentTransferRequest t, WorkflowEffectContext ctx) => t.ReopenAsDraft();
}

public class StaffLeaveWorkflowHandler : EntityHandlerBase<StaffLeaveRequest>, ITransientDependency
{
    public StaffLeaveWorkflowHandler(IRepository<StaffLeaveRequest, Guid> r) : base(r) { }
    public override WorkflowEntityType EntityType => WorkflowEntityType.StaffLeave;
    protected override void Completed(StaffLeaveRequest l, WorkflowEffectContext ctx) => l.Approve(ctx.ActorUserId);
    protected override void Rejected(StaffLeaveRequest l, WorkflowEffectContext ctx) => l.Reject(ctx.ActorUserId, ReasonOr(ctx, "Rejected via workflow."));
    protected override void Reopened(StaffLeaveRequest l, WorkflowEffectContext ctx) => l.ReopenAsDraft();
}

public class FieldTripWorkflowHandler : EntityHandlerBase<FieldTrip>, ITransientDependency
{
    public FieldTripWorkflowHandler(IRepository<FieldTrip, Guid> r) : base(r) { }
    public override WorkflowEntityType EntityType => WorkflowEntityType.FieldTrip;
    protected override void Completed(FieldTrip t, WorkflowEffectContext ctx) =>
        t.Approve(ctx.ActorUserId, ctx.Decision.GetDecimal("approvedBudget") ?? t.EstimatedCost);
    protected override void Rejected(FieldTrip t, WorkflowEffectContext ctx) => t.Reject(ctx.ActorUserId, ReasonOr(ctx, "Rejected via workflow."));
    protected override void Reopened(FieldTrip t, WorkflowEffectContext ctx) => t.ReopenAsDraft();
}

public class ExpenseRequestWorkflowHandler : EntityHandlerBase<ExpenseRequest>, ITransientDependency
{
    public ExpenseRequestWorkflowHandler(IRepository<ExpenseRequest, Guid> r) : base(r) { }
    public override WorkflowEntityType EntityType => WorkflowEntityType.ExpenseRequest;

    protected override void Completed(ExpenseRequest e, WorkflowEffectContext ctx)
    {
        var amount = ctx.Decision.GetDecimal("approvedAmount") ?? e.Amount;
        if (amount > e.Amount)
            throw new UserFriendlyException(WorkflowExceptionCodes.DecisionInvalid, "Approved amount cannot exceed the requested amount.");
        e.Approve(ctx.ActorUserId, amount);
    }

    protected override void Rejected(ExpenseRequest e, WorkflowEffectContext ctx) => e.Reject(ctx.ActorUserId, ReasonOr(ctx, "Rejected via workflow."));
    protected override void Reopened(ExpenseRequest e, WorkflowEffectContext ctx) => e.ReopenAsDraft();
}
