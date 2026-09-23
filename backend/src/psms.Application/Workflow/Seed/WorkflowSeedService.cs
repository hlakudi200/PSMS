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

    public WorkflowSeedService(
        IRepository<WorkflowDefinition, Guid> definitionRepository,
        IRepository<WorkflowStep, Guid> stepRepository)
    {
        _definitionRepository = definitionRepository;
        _stepRepository = stepRepository;
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

    private async Task SeedAdmissionsWorkflow()
    {
        var exists = await _definitionRepository
            .GetAll()
            .AnyAsync(d => d.TenantId == AbpSession.TenantId
                && d.EntityType == WorkflowEntityType.Application);

        if (exists) return;

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
                Name = "Admin Review",
                Description = "Admin reviews the submitted application for completeness.",
                AssignedRole = "Admin",
                ActionType = WorkflowActionType.Review
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 2,
                Name = "Document Verification",
                Description = "Verify all required documents are present and valid.",
                AssignedRole = "Admin",
                ActionType = WorkflowActionType.Review
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 3,
                Name = "Interview",
                Description = "Schedule and conduct parent/student interview.",
                AssignedRole = "Admin",
                ActionType = WorkflowActionType.Review
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 4,
                Name = "Committee Review",
                Description = "Admissions committee reviews the complete application.",
                AssignedRole = "Admin",
                ActionType = WorkflowActionType.Review
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 5,
                Name = "Principal Decision",
                Description = "Principal makes the final admission decision.",
                AssignedRole = "Principal",
                ActionType = WorkflowActionType.Approve,
                IsTerminal = true,
                NextStepOnReject = 4 // Send back to committee for reconsideration
            }
        };

        foreach (var step in steps)
        {
            await _stepRepository.InsertAsync(step);
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
