using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using psms.Domain.Academic.Entities;
using psms.Domain.Activities.Entities;
using psms.Domain.Admissions.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Discipline.Entities;
using psms.Domain.Financial.Entities;
using psms.Domain.HR.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Workflow.Enums;
using System;
using System.Threading.Tasks;

namespace psms.Workflow.Shared;

/// <summary>
/// Bridges workflow status changes to the linked domain entities.
/// Called internally by WorkflowInstanceAppService when a workflow completes or is rejected.
/// </summary>
public class WorkflowEntityBridgeService : ITransientDependency
{
    private readonly IRepository<Application, Guid> _applicationRepository;
    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly IRepository<FeeWaiver, Guid> _feeWaiverRepository;
    private readonly IRepository<DisciplinaryCase, Guid> _disciplinaryCaseRepository;
    private readonly IRepository<StudentTransferRequest, Guid> _transferRepository;
    private readonly IRepository<StaffLeaveRequest, Guid> _leaveRepository;
    private readonly IRepository<FieldTrip, Guid> _fieldTripRepository;
    private readonly IRepository<ExpenseRequest, Guid> _expenseRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    public ILogger Logger { get; set; } = NullLogger.Instance;

    public WorkflowEntityBridgeService(
        IRepository<Application, Guid> applicationRepository,
        IRepository<Report, Guid> reportRepository,
        IRepository<FeeWaiver, Guid> feeWaiverRepository,
        IRepository<DisciplinaryCase, Guid> disciplinaryCaseRepository,
        IRepository<StudentTransferRequest, Guid> transferRepository,
        IRepository<StaffLeaveRequest, Guid> leaveRepository,
        IRepository<FieldTrip, Guid> fieldTripRepository,
        IRepository<ExpenseRequest, Guid> expenseRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _applicationRepository = applicationRepository;
        _reportRepository = reportRepository;
        _feeWaiverRepository = feeWaiverRepository;
        _disciplinaryCaseRepository = disciplinaryCaseRepository;
        _transferRepository = transferRepository;
        _leaveRepository = leaveRepository;
        _fieldTripRepository = fieldTripRepository;
        _expenseRepository = expenseRepository;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task OnWorkflowCompletedAsync(WorkflowEntityType entityType, Guid entityId, long approvedByUserId)
    {
        try
        {
            switch (entityType)
            {
                case WorkflowEntityType.Application:
                    await HandleApplicationApproved(entityId, approvedByUserId);
                    break;
                case WorkflowEntityType.Report:
                    await HandleReportApproved(entityId, approvedByUserId);
                    break;
                case WorkflowEntityType.FeeWaiver:
                    await HandleEntityApproved(_feeWaiverRepository, entityId, e => e.Approve(approvedByUserId, e.RequestedAmount, "Approved via workflow."));
                    break;
                case WorkflowEntityType.Disciplinary:
                    await HandleEntityApproved(_disciplinaryCaseRepository, entityId, e => e.Resolve(approvedByUserId));
                    break;
                case WorkflowEntityType.StudentTransfer:
                    await HandleEntityApproved(_transferRepository, entityId, e => e.Approve(approvedByUserId));
                    break;
                case WorkflowEntityType.StaffLeave:
                    await HandleEntityApproved(_leaveRepository, entityId, e => e.Approve(approvedByUserId));
                    break;
                case WorkflowEntityType.FieldTrip:
                    await HandleEntityApproved(_fieldTripRepository, entityId, e => e.Approve(approvedByUserId, e.EstimatedCost));
                    break;
                case WorkflowEntityType.ExpenseRequest:
                    await HandleEntityApproved(_expenseRepository, entityId, e => e.Approve(approvedByUserId, e.Amount));
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"Bridge OnCompleted failed for {entityType} {entityId}: {ex.Message}");
        }
    }

    public async Task OnWorkflowRejectedAsync(WorkflowEntityType entityType, Guid entityId, long rejectedByUserId, string reason)
    {
        try
        {
            var rejectReason = string.IsNullOrWhiteSpace(reason) ? "Rejected via workflow." : reason;

            switch (entityType)
            {
                case WorkflowEntityType.Application:
                    await HandleApplicationRejected(entityId, rejectedByUserId, rejectReason);
                    break;
                case WorkflowEntityType.Report:
                    await HandleReportRejected(entityId);
                    break;
                case WorkflowEntityType.FeeWaiver:
                    await HandleEntityApproved(_feeWaiverRepository, entityId, e => e.Reject(rejectedByUserId, rejectReason));
                    break;
                case WorkflowEntityType.Disciplinary:
                    await HandleEntityApproved(_disciplinaryCaseRepository, entityId, e => e.Cancel());
                    break;
                case WorkflowEntityType.StudentTransfer:
                    await HandleEntityApproved(_transferRepository, entityId, e => e.Reject(rejectedByUserId, rejectReason));
                    break;
                case WorkflowEntityType.StaffLeave:
                    await HandleEntityApproved(_leaveRepository, entityId, e => e.Reject(rejectedByUserId, rejectReason));
                    break;
                case WorkflowEntityType.FieldTrip:
                    await HandleEntityApproved(_fieldTripRepository, entityId, e => e.Reject(rejectedByUserId, rejectReason));
                    break;
                case WorkflowEntityType.ExpenseRequest:
                    await HandleEntityApproved(_expenseRepository, entityId, e => e.Reject(rejectedByUserId, rejectReason));
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"Bridge OnRejected failed for {entityType} {entityId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Generic handler for entities with simple approve/reject domain methods.
    /// </summary>
    private async Task HandleEntityApproved<TEntity>(IRepository<TEntity, Guid> repository, Guid entityId, Action<TEntity> action)
        where TEntity : class, Abp.Domain.Entities.IEntity<Guid>
    {
        var entity = await repository.FirstOrDefaultAsync(e => e.Id == entityId);
        if (entity == null) return;

        try
        {
            action(entity);
            await repository.UpdateAsync(entity);
            await _unitOfWorkManager.Current.SaveChangesAsync();
        }
        catch (InvalidOperationException ex)
        {
            Logger.Warn($"Bridge action failed for {typeof(TEntity).Name} {entityId}: {ex.Message}");
        }
    }

    private async Task HandleApplicationApproved(Guid entityId, long approvedByUserId)
    {
        var application = await _applicationRepository.FirstOrDefaultAsync(a => a.Id == entityId);
        if (application == null) return;

        try
        {
            if (application.Status == ApplicationStatus.UnderReview)
                application.MoveToConsideration();
            application.Approve(approvedByUserId);
            await _applicationRepository.UpdateAsync(application);
            await _unitOfWorkManager.Current.SaveChangesAsync();
        }
        catch (InvalidOperationException)
        {
            Logger.Warn($"Could not auto-approve application {entityId}: not in correct state ({application.Status})");
        }
    }

    private async Task HandleApplicationRejected(Guid entityId, long rejectedByUserId, string reason)
    {
        var application = await _applicationRepository.FirstOrDefaultAsync(a => a.Id == entityId);
        if (application == null) return;

        try
        {
            if (application.Status == ApplicationStatus.UnderReview)
                application.MoveToConsideration();

            var rejectionReason = reason;
            if (rejectionReason.Length < 50)
                rejectionReason = rejectionReason.PadRight(50, '.');

            application.Reject(rejectedByUserId, rejectionReason);
            await _applicationRepository.UpdateAsync(application);
            await _unitOfWorkManager.Current.SaveChangesAsync();
        }
        catch (InvalidOperationException)
        {
            Logger.Warn($"Could not auto-reject application {entityId}: not in correct state ({application.Status})");
        }
    }

    private async Task HandleReportApproved(Guid entityId, long approvedByUserId)
    {
        var report = await _reportRepository.FirstOrDefaultAsync(r => r.Id == entityId);
        if (report == null) return;

        try
        {
            report.Approve(approvedByUserId);
            await _reportRepository.UpdateAsync(report);
            await _unitOfWorkManager.Current.SaveChangesAsync();
        }
        catch (InvalidOperationException)
        {
            Logger.Warn($"Could not auto-approve report {entityId}: not in correct state ({report.Status})");
        }
    }

    private async Task HandleReportRejected(Guid entityId)
    {
        var report = await _reportRepository.FirstOrDefaultAsync(r => r.Id == entityId);
        if (report == null) return;

        if (report.Status == ReportStatus.PendingApproval)
        {
            report.Status = ReportStatus.Generated;
            report.ApprovedByUserId = null;
            report.ApprovedDate = null;
            await _reportRepository.UpdateAsync(report);
            await _unitOfWorkManager.Current.SaveChangesAsync();
        }
    }
}
