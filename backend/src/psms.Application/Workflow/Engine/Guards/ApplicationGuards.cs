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
/// This used to ask only about documents that had been uploaded —
/// <c>IsRequired</c> on an <c>ApplicationDocument</c> row — which cannot answer
/// the question. A document nobody uploaded has no row, so an application with
/// nothing at all read as "no required documents have been uploaded yet", and
/// one with a single verified upload read as satisfied however much was still
/// missing. The requirements are the school's list; it is what the guard
/// measures against now.
/// </para>
/// </summary>
public class ApplicationDocumentsVerifiedGuard : IWorkflowStepGuard, ITransientDependency
{
    private readonly IRepository<ApplicationDocument, Guid> _documents;
    private readonly IRepository<Application, Guid> _applications;
    private readonly IRepository<AdmissionSettings, Guid> _settings;

    public ApplicationDocumentsVerifiedGuard(
        IRepository<ApplicationDocument, Guid> documents,
        IRepository<Application, Guid> applications,
        IRepository<AdmissionSettings, Guid> settings)
    {
        _documents = documents;
        _applications = applications;
        _settings = settings;
    }

    public string Key => "application.documents-verified";
    public WorkflowEntityType EntityType => WorkflowEntityType.Application;
    public string DisplayName => "All required documents verified";

    public async Task<WorkflowGuardResult> EvaluateAsync(Guid entityId)
    {
        var application = await _applications.GetAll()
            .Where(a => a.Id == entityId)
            .Select(a => new { a.AcademicYearId, a.AppliedGradeId })
            .FirstOrDefaultAsync();

        if (application == null) return WorkflowGuardResult.Fail("The application no longer exists.");

        var stored = await _settings.GetAll()
            .Where(x => x.AcademicYearId == application.AcademicYearId
                        && (x.GradeId == application.AppliedGradeId || x.GradeId == null))
            .OrderBy(x => x.GradeId == null ? 1 : 0)
            .Select(x => x.RequiredDocuments)
            .FirstOrDefaultAsync();

        var required = AdmissionSettings.ParseRequiredDocuments(stored);

        var uploaded = await _documents.GetAll()
            .Where(d => d.ApplicationId == entityId)
            .Select(d => new { d.Category, d.DocumentName, d.IsVerified, d.IsRequired })
            .ToListAsync();

        /* No list set. Fall back to what is actually on the application, which
           is the old behaviour and the best available answer — but say so, so
           that "verified" is not mistaken for "complete". */
        if (required.Count == 0)
        {
            var markedRequired = uploaded.Where(d => d.IsRequired).ToList();
            if (markedRequired.Count == 0)
                return WorkflowGuardResult.Fail(
                    "No required documents have been uploaded yet, and this grade has no "
                    + "document list set — so nothing can be checked against it.");

            var unverified = markedRequired.Where(d => !d.IsVerified).Select(d => d.DocumentName).ToList();
            return unverified.Count == 0
                ? WorkflowGuardResult.Ok()
                : WorkflowGuardResult.Fail(
                    $"{unverified.Count} required document(s) still unverified: {string.Join(", ", unverified)}.");
        }

        var missing = new List<string>();
        var unverifiedNames = new List<string>();

        foreach (var category in required)
        {
            var document = uploaded.FirstOrDefault(d => d.Category == category);
            if (document == null) missing.Add(category.ToString());
            else if (!document.IsVerified) unverifiedNames.Add(document.DocumentName ?? category.ToString());
        }

        if (missing.Count == 0 && unverifiedNames.Count == 0) return WorkflowGuardResult.Ok();

        // Missing and unverified are different jobs for different people — one
        // is the family's to do, the other the office's — so they are told apart.
        var parts = new List<string>();
        if (missing.Count > 0)
            parts.Add($"{missing.Count} not uploaded: {string.Join(", ", missing)}");
        if (unverifiedNames.Count > 0)
            parts.Add($"{unverifiedNames.Count} awaiting verification: {string.Join(", ", unverifiedNames)}");

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
