using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using psms.Domain.Admissions.Entities;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Workflow.Enums;
using System;
using System.Threading.Tasks;

namespace psms.Workflow.Shared;

/// <summary>
/// Bridges workflow status changes to the linked domain entities.
/// Called internally by WorkflowInstanceAppService when a workflow completes or is rejected.
/// NOT an ApplicationService — must not be exposed as an API endpoint.
/// </summary>
public class WorkflowEntityBridgeService : ITransientDependency
{
    private readonly IRepository<Application, Guid> _applicationRepository;
    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    public ILogger Logger { get; set; } = NullLogger.Instance;

    public WorkflowEntityBridgeService(
        IRepository<Application, Guid> applicationRepository,
        IRepository<Report, Guid> reportRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _applicationRepository = applicationRepository;
        _reportRepository = reportRepository;
        _unitOfWorkManager = unitOfWorkManager;
    }

    public async Task OnWorkflowCompletedAsync(WorkflowEntityType entityType, Guid entityId, long approvedByUserId)
    {
        switch (entityType)
        {
            case WorkflowEntityType.Application:
                await HandleApplicationApproved(entityId, approvedByUserId);
                break;
            case WorkflowEntityType.Report:
                await HandleReportApproved(entityId, approvedByUserId);
                break;
            default:
                break;
        }
    }

    public async Task OnWorkflowRejectedAsync(WorkflowEntityType entityType, Guid entityId, long rejectedByUserId, string reason)
    {
        switch (entityType)
        {
            case WorkflowEntityType.Application:
                await HandleApplicationRejected(entityId, rejectedByUserId, reason);
                break;
            case WorkflowEntityType.Report:
                await HandleReportRejected(entityId);
                break;
            default:
                break;
        }
    }

    private async Task HandleApplicationApproved(Guid entityId, long approvedByUserId)
    {
        var application = await _applicationRepository.FirstOrDefaultAsync(a => a.Id == entityId);
        if (application == null) return;

        try
        {
            // Move through required status transitions to reach Approved
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
            // Move through required status transitions to reach rejection
            if (application.Status == ApplicationStatus.UnderReview)
                application.MoveToConsideration();

            var rejectionReason = string.IsNullOrWhiteSpace(reason)
                ? "Application rejected through workflow approval process."
                : reason;
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
