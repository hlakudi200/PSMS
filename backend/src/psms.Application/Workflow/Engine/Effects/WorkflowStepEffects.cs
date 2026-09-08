using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.UI;
using psms.Domain.Academic.Entities;
using psms.Domain.Activities.Entities;
using psms.Domain.Admissions.Entities;
using psms.Domain.Discipline.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.HR.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Shared;
using System;
using System.Threading.Tasks;

namespace psms.Workflow.Engine.Effects;

/// <summary>
/// Shared plumbing for effects that load one aggregate, call one domain method
/// and save. A domain-rule violation becomes a UserFriendlyException so the
/// transition is rolled back with a readable reason.
/// </summary>
public abstract class EntityEffectBase<TEntity> : IWorkflowStepEffect
    where TEntity : class, Abp.Domain.Entities.IEntity<Guid>
{
    private readonly IRepository<TEntity, Guid> _repository;
    protected EntityEffectBase(IRepository<TEntity, Guid> repository) { _repository = repository; }

    public abstract string Key { get; }
    public abstract WorkflowEntityType EntityType { get; }
    public abstract string DisplayName { get; }

    protected abstract void Apply(TEntity entity, WorkflowEffectContext context);

    public async Task ApplyAsync(WorkflowEffectContext context)
    {
        var entity = await _repository.FirstOrDefaultAsync(e => e.Id == context.EntityId);
        if (entity == null)
            throw new UserFriendlyException(WorkflowExceptionCodes.EntityWriteBackFailed,
                $"The linked {typeof(TEntity).Name} no longer exists.");
        try
        {
            Apply(entity, context);
        }
        catch (InvalidOperationException ex)
        {
            throw new UserFriendlyException(WorkflowExceptionCodes.EntityWriteBackFailed,
                $"{DisplayName} could not be applied: {ex.Message}");
        }
        await _repository.UpdateAsync(entity);
    }
}

/// <summary>Admissions: entering the decision step moves the application to Under Consideration (no-op if already there).</summary>
public class ApplicationMoveToConsiderationEffect : EntityEffectBase<Application>, ITransientDependency
{
    public ApplicationMoveToConsiderationEffect(IRepository<Application, Guid> r) : base(r) { }
    public override string Key => "application.move-to-consideration";
    public override WorkflowEntityType EntityType => WorkflowEntityType.Application;
    public override string DisplayName => "Move application to Under Consideration";
    protected override void Apply(Application a, WorkflowEffectContext ctx)
    {
        if (a.Status == ApplicationStatus.UnderConsideration) return;
        if (a.Status == ApplicationStatus.InterviewScheduled || a.Status == ApplicationStatus.AssessmentScheduled)
            a.RevertToUnderReview();
        if (a.Status == ApplicationStatus.DocumentsRequired)
            a.MarkDocumentsComplete();
        a.MoveToConsideration();
    }
}

/// <summary>Disciplinary: entering the investigation step opens the investigation.</summary>
public class DisciplineStartInvestigationEffect : EntityEffectBase<DisciplinaryCase>, ITransientDependency
{
    public DisciplineStartInvestigationEffect(IRepository<DisciplinaryCase, Guid> r) : base(r) { }
    public override string Key => "discipline.start-investigation";
    public override WorkflowEntityType EntityType => WorkflowEntityType.Disciplinary;
    public override string DisplayName => "Start investigation";
    protected override void Apply(DisciplinaryCase c, WorkflowEffectContext ctx)
    {
        if (c.Status != DisciplinaryStatus.Reported) return; // already past this point
        c.StartInvestigation();
    }
}

public class FeeWaiverStartReviewEffect : EntityEffectBase<FeeWaiver>, ITransientDependency
{
    public FeeWaiverStartReviewEffect(IRepository<FeeWaiver, Guid> r) : base(r) { }
    public override string Key => "feewaiver.start-review";
    public override WorkflowEntityType EntityType => WorkflowEntityType.FeeWaiver;
    public override string DisplayName => "Mark waiver as under review";
    protected override void Apply(FeeWaiver w, WorkflowEffectContext ctx)
    {
        if (w.Status == FeeWaiverStatus.UnderReview) return;
        w.StartReview();
    }
}

public class FieldTripStartReviewEffect : EntityEffectBase<FieldTrip>, ITransientDependency
{
    public FieldTripStartReviewEffect(IRepository<FieldTrip, Guid> r) : base(r) { }
    public override string Key => "fieldtrip.start-review";
    public override WorkflowEntityType EntityType => WorkflowEntityType.FieldTrip;
    public override string DisplayName => "Mark trip as under review";
    protected override void Apply(FieldTrip t, WorkflowEffectContext ctx)
    {
        if (t.Status == FieldTripStatus.UnderReview) return;
        t.StartReview();
    }
}

public class ExpenseStartReviewEffect : EntityEffectBase<ExpenseRequest>, ITransientDependency
{
    public ExpenseStartReviewEffect(IRepository<ExpenseRequest, Guid> r) : base(r) { }
    public override string Key => "expense.start-review";
    public override WorkflowEntityType EntityType => WorkflowEntityType.ExpenseRequest;
    public override string DisplayName => "Mark expense as under review";
    protected override void Apply(ExpenseRequest e, WorkflowEffectContext ctx)
    {
        if (e.Status == ExpenseStatus.UnderReview) return;
        e.StartReview();
    }
}

public class TransferStartReviewEffect : EntityEffectBase<StudentTransferRequest>, ITransientDependency
{
    public TransferStartReviewEffect(IRepository<StudentTransferRequest, Guid> r) : base(r) { }
    public override string Key => "transfer.start-review";
    public override WorkflowEntityType EntityType => WorkflowEntityType.StudentTransfer;
    public override string DisplayName => "Mark transfer as under review";
    protected override void Apply(StudentTransferRequest t, WorkflowEffectContext ctx)
    {
        if (t.Status == TransferStatus.UnderReview) return;
        t.StartReview();
    }
}

/// <summary>Staff Leave: leaving the HOD step records the HOD's approval on the request.</summary>
public class LeaveHodApprovedEffect : EntityEffectBase<StaffLeaveRequest>, ITransientDependency
{
    public LeaveHodApprovedEffect(IRepository<StaffLeaveRequest, Guid> r) : base(r) { }
    public override string Key => "leave.hod-approved";
    public override WorkflowEntityType EntityType => WorkflowEntityType.StaffLeave;
    public override string DisplayName => "Record HOD approval";
    protected override void Apply(StaffLeaveRequest l, WorkflowEffectContext ctx)
    {
        if (l.Status == StaffLeaveStatus.HODApproved) return;
        l.HodApprove();
    }
}
