using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using psms.Domain.Workflow.Enums;
using System;
using System.Collections.Generic;
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

/// <summary>
/// Admissions Approval step 2: every required document is in and verified (ADM-010).
/// <para>
/// What the application owes comes from <see cref="RequiredDocumentsReader"/>,
/// which is also what the required-documents screen reads. The two used to
/// answer differently — the screen fell back to a default list for the grade
/// and this did not, so a Grade 5 applicant could be shown six outstanding
/// documents while the step blocking on them said there was no list to check
/// against.
/// </para>
/// <para>
/// Before that it asked only about documents that had been uploaded, which
/// cannot answer the question at all: a document nobody uploaded has no row,
/// so an application with nothing read as "none uploaded yet" and one with a
/// single verified upload read as satisfied however much was missing.
/// </para>
/// </summary>
public class ApplicationDocumentsVerifiedGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<ApplicationDocument, Guid> _documents;
    private readonly psms.Admissions.ApplicationDocuments.RequiredDocumentsReader _required;

    public ApplicationDocumentsVerifiedGuard(
        IRepository<ApplicationDocument, Guid> documents,
        psms.Admissions.ApplicationDocuments.RequiredDocumentsReader required)
    {
        _documents = documents;
        _required = required;
    }

    public string Key => "application.documents-verified";
    public WorkflowEntityType EntityType => WorkflowEntityType.Application;
    public string DisplayName => "All required documents verified";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var required = await _required.ForApplicationAsync(entityId);

        // Only an application that has gone. Every live one owes something.
        if (required.Count == 0)
            return WorkflowGuardResult.Fail("The application no longer exists.");

        var uploaded = await _documents.GetAll()
            .Where(d => d.ApplicationId == entityId)
            .Select(d => new { d.Category, d.DocumentName, d.IsVerified })
            .ToListAsync();

        var missing = new List<string>();
        var unverified = new List<string>();

        // Only the ones actually required. A transfer letter "if mid-year" is
        // not something to hold an application up over.
        foreach (var document in required.Where(r => r.IsRequired))
        {
            var match = uploaded.FirstOrDefault(u => u.Category == document.Category);
            if (match == null) missing.Add(document.Description);
            else if (!match.IsVerified) unverified.Add(match.DocumentName ?? document.Description);
        }

        if (missing.Count == 0 && unverified.Count == 0) return WorkflowGuardResult.Ok();

        /* Missing and unverified are different people's jobs — one the family's,
           one the office's — so whoever is holding this step is told which. */
        var parts = new List<string>();
        if (missing.Count > 0)
            parts.Add($"{missing.Count} not uploaded: {string.Join(", ", missing)}");
        if (unverified.Count > 0)
            parts.Add($"{unverified.Count} awaiting verification: {string.Join(", ", unverified)}");

        return WorkflowGuardResult.Fail(string.Join(". ", parts) + ".");
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
        /* Said to whoever is holding the step, which is the teacher who sits in
           the room — and who cannot book it. Telling them an interview is
           missing without telling them who arranges one leaves them stuck on a
           screen with nothing to click. */
        if (statuses.Count == 0)
            return WorkflowGuardResult.Fail(
                "No interview has been arranged yet. The office schedules it from the application, "
                + "and it will appear under your admissions tasks once it has.");

        return statuses.Contains(InterviewStatus.Completed)
            ? WorkflowGuardResult.Ok()
            : WorkflowGuardResult.Fail(
                "The interview is booked but its outcome has not been recorded. "
                + "Record it under your admissions tasks.");
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
        if (completed.Count == 0)
            return WorkflowGuardResult.Fail(
                "No placement assessment has been set yet. The office sets it from the application, "
                + "and it will appear under your admissions tasks once it has.");

        return completed.Any(c => c.HasValue)
            ? WorkflowGuardResult.Ok()
            : WorkflowGuardResult.Fail(
                "The assessment has been set but no result has been recorded. "
                + "Record it under your admissions tasks.");
    }
}
