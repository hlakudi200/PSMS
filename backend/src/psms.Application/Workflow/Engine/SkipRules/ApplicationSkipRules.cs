using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Admissions.Entities;
using psms.Domain.Workflow.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Engine.SkipRules;

/// <summary>
/// WF-34: the steps of the admissions workflow a school has said it does not do.
/// <para>
/// A school decides per intake whether it interviews and whether it sets a
/// placement assessment — <c>AdmissionSettings.IsInterviewRequired</c> and
/// <c>IsAssessmentRequired</c>. Those answers were recorded and then ignored:
/// every application walked the same five steps, so a school that does not
/// interview still had an Interview step sitting in somebody's queue waiting
/// to be dismissed.
/// </para>
/// </summary>
internal static class AdmissionSettingsLookup
{
    /// <summary>
    /// The settings that govern an application: the ones set for its exact
    /// grade, else the year's defaults. The same precedence the application
    /// service uses — a grade with its own rules overrides the year's.
    /// </summary>
    public static async Task<AdmissionSettings> ForApplicationAsync(
        IRepository<Application, Guid> applications,
        IRepository<AdmissionSettings, Guid> settings,
        Guid applicationId)
    {
        var application = await applications
            .GetAll()
            .Where(a => a.Id == applicationId)
            .Select(a => new { a.AcademicYearId, a.AppliedGradeId })
            .FirstOrDefaultAsync();

        if (application == null)
            return null;

        return await settings.FirstOrDefaultAsync(s =>
                   s.AcademicYearId == application.AcademicYearId
                   && s.GradeId == application.AppliedGradeId)
               ?? await settings.FirstOrDefaultAsync(s =>
                   s.AcademicYearId == application.AcademicYearId && s.GradeId == null);
    }
}

/// <summary>
/// Skips the Interview step for a grade the school does not interview for.
/// </summary>
public class InterviewNotRequiredSkipRule : IWorkflowStepSkipRule, ITransientDependency
{
    private readonly IRepository<Application, Guid> _applications;
    private readonly IRepository<AdmissionSettings, Guid> _settings;

    public InterviewNotRequiredSkipRule(
        IRepository<Application, Guid> applications,
        IRepository<AdmissionSettings, Guid> settings)
    {
        _applications = applications;
        _settings = settings;
    }

    public string Key => "application.interview-not-required";
    public WorkflowEntityType EntityType => WorkflowEntityType.Application;
    public string DisplayName => "Skip when the grade does not require an interview";

    public async Task<WorkflowSkipResult> EvaluateAsync(Guid entityId)
    {
        var settings = await AdmissionSettingsLookup.ForApplicationAsync(_applications, _settings, entityId);

        /* No settings at all is not the same as "no interview needed". Treat
           silence as "the step applies" so a missing configuration surfaces as
           a step somebody has to look at, rather than quietly removing a stage
           of the school's own admissions process. */
        if (settings == null || settings.IsInterviewRequired)
            return WorkflowSkipResult.Applies();

        return WorkflowSkipResult.Skip("The school does not require an interview for this grade.");
    }
}

/// <summary>
/// Skips the placement assessment step for a grade that does not set one.
/// </summary>
public class AssessmentNotRequiredSkipRule : IWorkflowStepSkipRule, ITransientDependency
{
    private readonly IRepository<Application, Guid> _applications;
    private readonly IRepository<AdmissionSettings, Guid> _settings;

    public AssessmentNotRequiredSkipRule(
        IRepository<Application, Guid> applications,
        IRepository<AdmissionSettings, Guid> settings)
    {
        _applications = applications;
        _settings = settings;
    }

    public string Key => "application.assessment-not-required";
    public WorkflowEntityType EntityType => WorkflowEntityType.Application;
    public string DisplayName => "Skip when the grade does not require a placement assessment";

    public async Task<WorkflowSkipResult> EvaluateAsync(Guid entityId)
    {
        var settings = await AdmissionSettingsLookup.ForApplicationAsync(_applications, _settings, entityId);

        if (settings == null || settings.IsAssessmentRequired)
            return WorkflowSkipResult.Applies();

        return WorkflowSkipResult.Skip("The school does not set a placement assessment for this grade.");
    }
}
