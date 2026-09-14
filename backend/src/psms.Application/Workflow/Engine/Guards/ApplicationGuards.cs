using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Workflow.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Engine.Guards;

/// <summary>Admissions Approval step 1: the application fee has been paid (ADM-006).</summary>
public class ApplicationFeePaidGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<ApplicationFee, Guid> _fees;
    public ApplicationFeePaidGuard(IRepository<ApplicationFee, Guid> fees) { _fees = fees; }

    public string Key => "application.fee-paid";
    public WorkflowEntityType EntityType => WorkflowEntityType.Application;
    public string DisplayName => "Application fee paid";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var paid = await _fees.GetAll().AnyAsync(f => f.ApplicationId == entityId && f.Status == PaymentStatus.Completed);
        return paid ? WorkflowGuardResult.Ok() : WorkflowGuardResult.Fail("The application fee has not been paid.");
    }
}

/// <summary>Admissions Approval step 1: at least one parent, with a primary contact and a financially responsible party (ADM-003).</summary>
public class ApplicationParentsCompleteGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<ApplicantParent, Guid> _parents;
    public ApplicationParentsCompleteGuard(IRepository<ApplicantParent, Guid> parents) { _parents = parents; }

    public string Key => "application.parents-complete";
    public WorkflowEntityType EntityType => WorkflowEntityType.Application;
    public string DisplayName => "Parent / guardian details complete";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var parents = await _parents.GetAll().Where(p => p.ApplicationId == entityId)
            .Select(p => new { p.IsPrimaryContact, p.IsFinanciallyResponsible }).ToListAsync();
        if (parents.Count == 0) return WorkflowGuardResult.Fail("No parent or guardian has been captured.");
        if (!parents.Any(p => p.IsPrimaryContact)) return WorkflowGuardResult.Fail("No parent is marked as the primary contact.");
        if (!parents.Any(p => p.IsFinanciallyResponsible)) return WorkflowGuardResult.Fail("No parent is marked as financially responsible.");
        return WorkflowGuardResult.Ok();
    }
}

/// <summary>Admissions Approval step 2: every required document is verified (ADM-010).</summary>
public class ApplicationDocumentsVerifiedGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<ApplicationDocument, Guid> _documents;
    public ApplicationDocumentsVerifiedGuard(IRepository<ApplicationDocument, Guid> documents) { _documents = documents; }

    public string Key => "application.documents-verified";
    public WorkflowEntityType EntityType => WorkflowEntityType.Application;
    public string DisplayName => "All required documents verified";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var required = await _documents.GetAll()
            .Where(d => d.ApplicationId == entityId && d.IsRequired)
            .Select(d => new { d.DocumentName, d.IsVerified }).ToListAsync();
        if (required.Count == 0) return WorkflowGuardResult.Fail("No required documents have been uploaded yet.");
        var pending = required.Where(d => !d.IsVerified).Select(d => d.DocumentName).ToList();
        return pending.Count == 0
            ? WorkflowGuardResult.Ok()
            : WorkflowGuardResult.Fail($"{pending.Count} required document(s) still unverified: {string.Join(", ", pending)}.");
    }
}

/// <summary>Admissions Approval step 3: the interview outcome has been recorded (ADM-013).</summary>
public class ApplicationInterviewRecordedGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<AdmissionInterview, Guid> _interviews;
    public ApplicationInterviewRecordedGuard(IRepository<AdmissionInterview, Guid> interviews) { _interviews = interviews; }

    public string Key => "application.interview-recorded";
    public WorkflowEntityType EntityType => WorkflowEntityType.Application;
    public string DisplayName => "Interview completed and outcome recorded";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var statuses = await _interviews.GetAll().Where(i => i.ApplicationId == entityId).Select(i => i.Status).ToListAsync();
        if (statuses.Count == 0) return WorkflowGuardResult.Fail("No interview has been scheduled.");
        return statuses.Contains(InterviewStatus.Completed)
            ? WorkflowGuardResult.Ok()
            : WorkflowGuardResult.Fail("The interview has not been completed and rated.");
    }
}

/// <summary>Admissions Approval step 4: the placement assessment result has been recorded (ADM-016).</summary>
public class ApplicationAssessmentRecordedGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<AdmissionAssessment, Guid> _assessments;
    public ApplicationAssessmentRecordedGuard(IRepository<AdmissionAssessment, Guid> assessments) { _assessments = assessments; }

    public string Key => "application.assessment-recorded";
    public WorkflowEntityType EntityType => WorkflowEntityType.Application;
    public string DisplayName => "Placement assessment result recorded";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var completed = await _assessments.GetAll().Where(a => a.ApplicationId == entityId).Select(a => a.CompletedDate).ToListAsync();
        if (completed.Count == 0) return WorkflowGuardResult.Fail("No placement assessment has been scheduled.");
        return completed.Any(c => c.HasValue)
            ? WorkflowGuardResult.Ok()
            : WorkflowGuardResult.Fail("The assessment result has not been recorded.");
    }
}
