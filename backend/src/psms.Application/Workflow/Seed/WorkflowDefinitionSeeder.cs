using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using psms.Authorization.Roles;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Seed;

/// <summary>
/// WF-35: the eight default approval workflows, expressed as data and applied per
/// tenant. Used at tenant creation (TenantAppService) and by the host / tenant
/// backfill endpoints, so it deliberately has no authorization attribute and
/// takes the tenant id explicitly (the caller may be running under a different
/// session tenant).
///
/// Rules per entity type:
///  - no definition yet            → create the default, active;
///  - the default (version 1, same name) with no instances → refresh its steps in
///    place so existing tenants pick up guards, effects, roles and SLAs;
///  - anything else (customised, or already used) → leave untouched.
/// </summary>
public class WorkflowDefinitionSeeder : ITransientDependency
{
    private readonly IRepository<WorkflowDefinition, Guid> _definitions;
    private readonly IRepository<WorkflowStep, Guid> _steps;
    private readonly IRepository<WorkflowInstance, Guid> _instances;

    public WorkflowDefinitionSeeder(
        IRepository<WorkflowDefinition, Guid> definitions,
        IRepository<WorkflowStep, Guid> steps,
        IRepository<WorkflowInstance, Guid> instances)
    {
        _definitions = definitions;
        _steps = steps;
        _instances = instances;
    }

    public async Task<List<WorkflowSeedOutcome>> SeedDefaultsAsync(int? tenantId)
    {
        var outcomes = new List<WorkflowSeedOutcome>();

        foreach (var template in Defaults())
        {
            var existing = await _definitions.GetAll()
                .Include(d => d.Steps)
                .Where(d => d.TenantId == tenantId && d.EntityType == template.EntityType)
                .ToListAsync();

            if (existing.Count == 0)
            {
                var definition = new WorkflowDefinition
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Name = template.Name,
                    Description = template.Description,
                    EntityType = template.EntityType,
                    IsActive = true,
                };
                await _definitions.InsertAsync(definition);
                foreach (var step in template.Steps)
                    await _steps.InsertAsync(ToEntity(definition.Id, step));
                outcomes.Add(new WorkflowSeedOutcome(template.EntityType, template.Name, "Created"));
                continue;
            }

            var seeded = existing.FirstOrDefault(d => d.Version == 1 && d.Name == template.Name);
            if (seeded == null)
            {
                outcomes.Add(new WorkflowSeedOutcome(template.EntityType, existing[0].Name, "Skipped: customised definition"));
                continue;
            }

            var inUse = await _instances.GetAll().AnyAsync(i => i.WorkflowDefinitionId == seeded.Id);
            if (inUse)
            {
                outcomes.Add(new WorkflowSeedOutcome(template.EntityType, seeded.Name, "Skipped: definition has instances"));
                continue;
            }

            foreach (var step in seeded.Steps.ToList())
                await _steps.DeleteAsync(step);
            foreach (var step in template.Steps)
                await _steps.InsertAsync(ToEntity(seeded.Id, step));
            seeded.Description = template.Description;
            if (!existing.Any(d => d.IsActive)) seeded.Activate();
            await _definitions.UpdateAsync(seeded);
            outcomes.Add(new WorkflowSeedOutcome(template.EntityType, seeded.Name, "Updated"));
        }

        return outcomes;
    }

    private static WorkflowStep ToEntity(Guid definitionId, DefaultStep s) => new()
    {
        Id = Guid.NewGuid(),
        WorkflowDefinitionId = definitionId,
        StepOrder = s.Order,
        Name = s.Name,
        Description = s.Description,
        AssignedRole = s.Role,
        ActionType = s.Action,
        IsTerminal = s.IsTerminal,
        NextStepOnReject = s.NextStepOnReject,
        IsCommentRequired = s.IsCommentRequired,
        SlaHours = s.SlaHours,
        GuardKey = s.GuardKey,
        EntryEffectKey = s.EntryEffectKey,
        ExitEffectKey = s.ExitEffectKey,
        DecisionSchemaKey = s.DecisionSchemaKey,
        IsOptional = s.IsOptional,
    };

    // ------------------------------------------------------------------
    // The defaults. Keys must match the registered guards / effects / schemas
    // in psms.Workflow.Engine (WorkflowStepAppService validates the same way).
    // ------------------------------------------------------------------
    private static IEnumerable<DefaultDefinition> Defaults()
    {
        const string admissions = StaticRoleNames.Tenants.AdmissionsOfficer;
        const string principal = StaticRoleNames.Tenants.Principal;
        const string vp = StaticRoleNames.Tenants.VicePrincipal;
        const string hod = StaticRoleNames.Tenants.HOD;
        const string finance = StaticRoleNames.Tenants.Finance;
        const string admin = StaticRoleNames.Tenants.Admin;

        yield return new DefaultDefinition(WorkflowEntityType.Application, "Admissions Approval",
            "Completeness → documents → interview → assessment → Principal decision. Starts when the application fee is paid.",
            new DefaultStep(1, "Completeness Review", "Check applicant details, parents and fee.", admissions, WorkflowActionType.Review)
                { SlaHours = 48, GuardKey = "application.parents-complete" },
            new DefaultStep(2, "Document Verification", "Verify every required document; reject with a reason to request a re-upload.", admissions, WorkflowActionType.Review)
                { SlaHours = 72, GuardKey = "application.documents-verified" },
            new DefaultStep(3, "Interview", "Schedule and record the interview, or waive it where the grade does not require one.", hod, WorkflowActionType.Review)
                { SlaHours = 168, GuardKey = "application.interview-recorded", IsOptional = true },
            new DefaultStep(4, "Placement Assessment", "Schedule and record the assessment, or waive it where not required.", hod, WorkflowActionType.Review)
                { SlaHours = 168, GuardKey = "application.assessment-recorded", IsOptional = true },
            new DefaultStep(5, "Principal Decision", "Accept, accept with conditions, waitlist or reject.", principal, WorkflowActionType.Approve)
                { SlaHours = 72, IsTerminal = true, EntryEffectKey = "application.move-to-consideration", DecisionSchemaKey = "application.decision" });

        yield return new DefaultDefinition(WorkflowEntityType.Report, "Report Approval",
            "HOD checks the subjects are complete; the Principal comments and approves. Starts when a teacher submits a report for approval.",
            new DefaultStep(1, "HOD Review", "Review subject marks and teacher comments.", hod, WorkflowActionType.Review)
                { SlaHours = 72, GuardKey = "report.subjects-complete" },
            new DefaultStep(2, "Principal Approval", "Add the principal's comment and approve, or return with a reason.", principal, WorkflowActionType.Approve)
                { SlaHours = 72, IsTerminal = true, DecisionSchemaKey = "report.approval" });

        yield return new DefaultDefinition(WorkflowEntityType.FeeWaiver, "Fee Waiver Approval",
            "Finance reviews the request and recommends an amount; the Principal approves an amount. Starts on submit.",
            new DefaultStep(1, "Finance Review", "Check the supporting document and fee account; record a recommended amount.", finance, WorkflowActionType.Review)
                { SlaHours = 72, GuardKey = "feewaiver.supporting-document-attached", EntryEffectKey = "feewaiver.start-review", DecisionSchemaKey = "review.recommended-amount" },
            new DefaultStep(2, "Principal Approval", "Approve an amount with notes, or reject with notes.", principal, WorkflowActionType.Approve)
                { SlaHours = 72, IsTerminal = true, DecisionSchemaKey = "feewaiver.approval" });

        yield return new DefaultDefinition(WorkflowEntityType.Disciplinary, "Disciplinary Process",
            "HOD investigates and schedules the hearing; the hearing records the outcome; the Principal decides. Starts when a case is submitted.",
            new DefaultStep(1, "HOD Investigation", "Record investigation notes and schedule the hearing.", hod, WorkflowActionType.Review)
                { SlaHours = 120, IsCommentRequired = true, EntryEffectKey = "discipline.start-investigation", GuardKey = "discipline.hearing-scheduled" },
            // Not optional: resolving a case requires a recorded outcome, so waiving
            // the hearing would strand it at the Principal step. WF-50 adds the
            // Minor-severity shortcut (record the outcome without a hearing).
            new DefaultStep(2, "Disciplinary Hearing", "Hold the hearing and record the outcome and sanction.", vp, WorkflowActionType.Review)
                { SlaHours = 120, IsCommentRequired = true, GuardKey = "discipline.outcome-recorded" },
            new DefaultStep(3, "Principal Decision", "Confirm the outcome and resolve the case, or send it back to the hearing.", principal, WorkflowActionType.Approve)
                { SlaHours = 72, IsTerminal = true, IsCommentRequired = true, NextStepOnReject = 2 });

        yield return new DefaultDefinition(WorkflowEntityType.StudentTransfer, "Student Transfer Approval",
            "Admin verifies the transfer details; the Principal approves. Starts on submit.",
            new DefaultStep(1, "Admin Review", "Verify schools, effective date and the previous report.", admin, WorkflowActionType.Review)
                { SlaHours = 72, EntryEffectKey = "transfer.start-review", GuardKey = "transfer.details-complete" },
            new DefaultStep(2, "Principal Approval", "Approve or reject with notes.", principal, WorkflowActionType.Approve)
                { SlaHours = 72, IsTerminal = true });

        yield return new DefaultDefinition(WorkflowEntityType.StaffLeave, "Staff Leave Approval",
            "HOD confirms the substitute; the Principal approves. Starts on submit.",
            new DefaultStep(1, "HOD Review", "Confirm the substitute teacher and supporting document.", hod, WorkflowActionType.Review)
                { SlaHours = 48, GuardKey = "leave.substitute-arranged", ExitEffectKey = "leave.hod-approved" },
            new DefaultStep(2, "Principal Approval", "Approve, or reject with a reason.", principal, WorkflowActionType.Approve)
                { SlaHours = 48, IsTerminal = true });

        yield return new DefaultDefinition(WorkflowEntityType.FieldTrip, "Field Trip Approval",
            "HOD reviews safety and logistics; Finance checks the budget; the Principal approves a budget. Starts on submit.",
            new DefaultStep(1, "HOD Review", "Check educational value, risk assessment, emergency plan and transport.", hod, WorkflowActionType.Review)
                { SlaHours = 120, EntryEffectKey = "fieldtrip.start-review", GuardKey = "fieldtrip.safety-complete" },
            new DefaultStep(2, "Finance Review", "Check the estimate against budget and comment on a recommended budget.", finance, WorkflowActionType.Review)
                { SlaHours = 72, IsCommentRequired = true },
            new DefaultStep(3, "Principal Approval", "Approve a budget, or reject with a reason.", principal, WorkflowActionType.Approve)
                { SlaHours = 72, IsTerminal = true, DecisionSchemaKey = "fieldtrip.approval" });

        yield return new DefaultDefinition(WorkflowEntityType.ExpenseRequest, "Expense Approval",
            "Finance reviews the quotation; the Principal approves an amount. Starts on submit.",
            new DefaultStep(1, "Finance Review", "Check the quotation, vendor and budget line.", finance, WorkflowActionType.Review)
                { SlaHours = 72, EntryEffectKey = "expense.start-review", GuardKey = "expense.quotation-attached" },
            new DefaultStep(2, "Principal Approval", "Approve an amount, or reject with a reason.", principal, WorkflowActionType.Approve)
                { SlaHours = 72, IsTerminal = true, DecisionSchemaKey = "expense.approval" });
    }

    private sealed class DefaultDefinition
    {
        public DefaultDefinition(WorkflowEntityType entityType, string name, string description, params DefaultStep[] steps)
        {
            EntityType = entityType; Name = name; Description = description; Steps = steps;
        }
        public WorkflowEntityType EntityType { get; }
        public string Name { get; }
        public string Description { get; }
        public DefaultStep[] Steps { get; }
    }

    private sealed class DefaultStep
    {
        public DefaultStep(int order, string name, string description, string role, WorkflowActionType action)
        {
            Order = order; Name = name; Description = description; Role = role; Action = action;
        }
        public int Order { get; }
        public string Name { get; }
        public string Description { get; }
        public string Role { get; }
        public WorkflowActionType Action { get; }
        public bool IsTerminal { get; init; }
        public int? NextStepOnReject { get; init; }
        public bool IsCommentRequired { get; init; }
        public int? SlaHours { get; init; }
        public string GuardKey { get; init; }
        public string EntryEffectKey { get; init; }
        public string ExitEffectKey { get; init; }
        public string DecisionSchemaKey { get; init; }
        public bool IsOptional { get; init; }
    }
}

/// <summary>What the seeder did for one entity type — returned to the caller so a backfill is auditable.</summary>
public sealed class WorkflowSeedOutcome
{
    public WorkflowSeedOutcome(WorkflowEntityType entityType, string definitionName, string result)
    {
        EntityType = entityType; DefinitionName = definitionName; Result = result;
    }
    public WorkflowEntityType EntityType { get; }
    public string DefinitionName { get; }
    public string Result { get; }
}
